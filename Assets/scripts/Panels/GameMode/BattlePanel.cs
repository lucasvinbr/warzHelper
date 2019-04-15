using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// the battle panel!
/// it will only go away after the battle has been resolved.
/// A battle will only resolve after one of the sides has been completely eliminated
/// </summary>
public class BattlePanel : GrowingOverlayPanel {

	public BattlePhaseMan battlePhase;

	//script for left side and right faction sides
	public BattlePanelFactionSideInfo attackerSide, defenderSide;

	public GameObject mainResolutionOptionsBox, manualResolutionOptionsBox, impExpResolutionOptionsBox;

	public BattleResolutionPercentagePanel percentageResolutionPanel;

	public BattleResolutionManualPanel manualPanel;

	public Text zoneNameTxt;

	public Image zoneImg;

	[HideInInspector]
	public Zone curWarzone;

	public CanvasGroup resolutionBtnsGroup;

	public void OpenWithFilledInfos(Faction attackerFaction, Faction defenderFaction, Zone warZone, bool immediateAutocalc = false) {
		gameObject.SetActive(true);
		
		curWarzone = warZone;

		attackerSide.SetContent(attackerFaction);
		attackerSide.SetArmyData(null,
			GameController.CmdersToTroopContainers
			(GameController.GetCommandersOfFactionAndAlliesInZone(warZone, attackerFaction)));

		defenderSide.SetContent(defenderFaction);
		defenderSide.SetArmyData(warZone,
			GameController.CmdersToTroopContainers
			(GameController.GetCommandersOfFactionAndAlliesInZone(warZone, defenderFaction)));

		//zone info...
		zoneNameTxt.text = warZone.name;
		if (string.IsNullOrEmpty(warZone.pictureFilePath)) {
			zoneImg.gameObject.SetActive(false);
		}
		else {
			zoneImg.gameObject.SetActive(true);
			//TODO set image and all that
		}

		ResetUI();

		if (immediateAutocalc) {
			AutocalcResolution();
		}
	}

	public void OpenPercentageResolution() {
		percentageResolutionPanel.gameObject.SetActive(true);
	}

	public void OpenManualResolution() {
		manualPanel.SetupAndOpen(attackerSide.sideArmy, defenderSide.sideArmy);
	}

	/// <summary>
	/// multiple mini-battles are made between randomized samples of each side's army
	/// </summary>
	public void AutocalcResolution() {
		int sampleSize;

		int attackerSampleSize, defenderSampleSize;

		float armyNumbersProportion;

		float attackerAutoPower, defenderAutoPower;

		bool shouldAnimateBars = false;

		float winnerDmgMultiplier = GameController.instance.curData.rules.autoResolveWinnerDamageMultiplier;

		while(attackerSide.curArmyPower > 0 && defenderSide.curArmyPower > 0) {
			sampleSize = Mathf.Min(attackerSide.curArmyNumbers, defenderSide.curArmyNumbers, 20);

			attackerSampleSize = sampleSize;
			defenderSampleSize = sampleSize;

			armyNumbersProportion = (float)attackerSide.curArmyNumbers / defenderSide.curArmyNumbers;
			//Debug.Log("army num proportion: " + armyNumbersProportion);
			//the size with a bigger army gets a bigger sample
			if (armyNumbersProportion > 1.0f) {
				attackerSampleSize = Mathf.RoundToInt(sampleSize * armyNumbersProportion);
			}
			else {
				defenderSampleSize = Mathf.RoundToInt(sampleSize / armyNumbersProportion);
			}

			Debug.Log("attacker sSize: " + attackerSampleSize);
			Debug.Log("defender sSize: " + defenderSampleSize);

			attackerAutoPower = GameController.
				GetRandomBattleAutocalcPower(attackerSide.sideArmy, attackerSampleSize);
			defenderAutoPower = GameController.
				GetRandomBattleAutocalcPower(defenderSide.sideArmy, defenderSampleSize);

			Debug.Log("attacker auto power: " + attackerAutoPower);
			Debug.Log("defender auto power: " + defenderAutoPower);

			//make the winner lose some power as well (or not, depending on the rules)
			if (attackerAutoPower > defenderAutoPower) {
				defenderAutoPower *= winnerDmgMultiplier;
			}else {
				attackerAutoPower *= winnerDmgMultiplier;
			}

			//animate the power bars if the conflict is over
			shouldAnimateBars = attackerAutoPower >= defenderSide.curArmyPower ||
				defenderAutoPower >= attackerSide.curArmyPower;

			defenderSide.SetPostBattleArmyData_PowerLost(attackerAutoPower, shouldAnimateBars);
			attackerSide.SetPostBattleArmyData_PowerLost(defenderAutoPower, shouldAnimateBars);

		}
		
	}

	/// <summary>
	/// checks if the other side is also done animating;
	/// if so, reactivates the resolution buttons if the battle's not resolved yet
	/// </summary>
	public void OnOneSideDoneAnimating() {
		if(!attackerSide.depletingBar && !defenderSide.depletingBar && !transitioning) {
			resolutionBtnsGroup.interactable = true;
		}
	}

	public void OnBattleResolved(BattlePanelFactionSideInfo loserSide) {
		if(loserSide == attackerSide) {
			defenderSide.SharePointsBetweenConts(loserSide.pointsAwardedToVictor);
		}else {
			attackerSide.SharePointsBetweenConts(loserSide.pointsAwardedToVictor);
		}
		Shrink(TellPhaseManAboutResolution);
	}

	public void TellPhaseManAboutResolution() {
		battlePhase.OnBattleResolved(curWarzone);
	}

	/// <summary>
	/// immediately stops coroutines for both sides and the panel itself.
	/// things that would happen in the end of those coroutines won't happen
	/// </summary>
	public void FullInterrupt() {
		attackerSide.StopAllCoroutines();
		defenderSide.StopAllCoroutines();
		ResetTransitionState();
	}

	public void ResetUI() {
		manualResolutionOptionsBox.SetActive(false);
		impExpResolutionOptionsBox.SetActive(false);
		mainResolutionOptionsBox.SetActive(true);
		resolutionBtnsGroup.interactable = true;
	}

	public void OpenExportOps() {

		List<KeyValuePair<string, UnityAction>> exportOptions =
			new List<KeyValuePair<string, UnityAction>>();

		GameInterface GI = GameInterface.instance;

		//add export options now...

		//JSON basic "all troops together" export!
		exportOptions.Add(new KeyValuePair<string, UnityAction>("Export to JSON (all troops in a simple list - don't use if both sides use the same troop type)", () => {
			SerializableTroopList exportedList = GameController.TroopListToSerializableTroopList
				(GameController.GetCombinedTroopsFromTwoLists
				(attackerSide.sideArmy, defenderSide.sideArmy));
			string JSONContent = JsonUtility.ToJson(exportedList);
			Debug.Log(JSONContent);
			GI.textInputPanel.SetPanelInfo("JSON Export Result", "", JSONContent, "Copy to Clipboard", () => {

				GameInterface.CopyToClipboard(GI.textInputPanel.theInputField.text);

			});
			GI.textInputPanel.Open();
			GI.exportOpsPanel.gameObject.SetActive(false);
		}));


		//when done preparing options, open the export ops panel
		GI.exportOpsPanel.Open("Remaining Armies: Export Options", exportOptions);
	}

	public void OpenImportOps() {
		List<KeyValuePair<string, UnityAction>> importOptions =
			new List<KeyValuePair<string, UnityAction>>();

		GameInterface GI = GameInterface.instance;


		//TODO some import options


		//when done preparing options, open the import ops panel
		GI.exportOpsPanel.Open("Remaining Armies: Import Options", importOptions);
	}
}
