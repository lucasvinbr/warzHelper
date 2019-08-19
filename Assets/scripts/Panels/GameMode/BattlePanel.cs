using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Runtime.Serialization.Json;

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

	/// <summary>
	/// sets the info displays to what's in the battle data
	/// (the battle data must be filled before this is called)
	/// </summary>
	/// <param name="battleData"></param>
	public void OpenWithFilledInfo(Battle battleData) {
		gameObject.SetActive(true);
		
		curWarzone = battleData.warZone;

		attackerSide.sideInfo = battleData.attackerSideInfo;
		attackerSide.SetContent(battleData.attackerSideInfo.leadingFaction);
		attackerSide.SetArmyDataDisplay(null);

		defenderSide.sideInfo = battleData.defenderSideInfo;
		defenderSide.SetContent(battleData.defenderSideInfo.leadingFaction);
		defenderSide.SetArmyDataDisplay(battleData.warZone);

		//runs the bar depleting animations once a side has been defeated
		battlePhase.battleData.onBattleEnded += UpdateArmyDisplays;

		//zone info...
		zoneNameTxt.text = battleData.warZone.name;
		if (string.IsNullOrEmpty(battleData.warZone.pictureFilePath)) {
			zoneImg.gameObject.SetActive(false);
		}
		else {
			zoneImg.gameObject.SetActive(true);
			//TODO set image and all that
		}

		ResetUI();

	}

	public void OpenPercentageResolution() {
		percentageResolutionPanel.gameObject.SetActive(true);
	}

	public void OpenManualResolution() {
		manualPanel.SetupAndOpen(battlePhase.battleData.attackerSideInfo.sideArmy, battlePhase.battleData.defenderSideInfo.sideArmy);
	}

	/// <summary>
	/// tells the battleData to run autocalc
	/// </summary>
	public void AutocalcResolution() {
		battlePhase.battleData.AutocalcResolution();
	}

	public void UpdateArmyDisplays() {
		attackerSide.UpdatePostBattleArmyDisplay(true);
		defenderSide.UpdatePostBattleArmyDisplay(true);
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

	public void BattleResolved(BattlePanelFactionSideInfo loserSide) {
		if (!transitioning) {
			Shrink(TellPhaseManAboutResolution);
		}
	}

	public void TellPhaseManAboutResolution() {
		battlePhase.OnBattleResolved();
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

		GameInfo gData = GameController.CurGameData;

		//add export options now...

		//JSON basic "all troops together" export!
		exportOptions.Add(new KeyValuePair<string, UnityAction>("Basic Export to JSON (all troops in a simple list - don't use if both sides use the same troop type)", () => {
			SerializableTroopListObj exportedList = new SerializableTroopListObj(JsonHandlingUtils.TroopListToSerializableTroopList
				(GameController.GetCombinedTroopsFromTwoLists
				(battlePhase.battleData.attackerSideInfo.sideArmy, battlePhase.battleData.defenderSideInfo.sideArmy)));
			string JSONContent = JsonUtility.ToJson(exportedList);
			Debug.Log(JSONContent);
			GI.textInputPanel.SetPanelInfo("JSON Export Result", "", JSONContent, "Copy to Clipboard", () => {

				GameInterface.CopyToClipboard(GI.textInputPanel.theInputField.text);

			});
			GI.textInputPanel.Open();
			GI.exportOpsPanel.gameObject.SetActive(false);
		}));

		//JSON basic "all troops together" export, splitting entries if troop amounts go above a certain limit!
		exportOptions.Add(new KeyValuePair<string, UnityAction>("Basic Export to JSON, splitting large troop entries - don't use if both sides use the same troop type", () => {
			GI.exportOpsPanel.gameObject.SetActive(false);

			GI.customInputPanel.Open();
			NumericInputFieldBtns numBtns = GI.customInputPanel.AddNumericInput("Split Limit", true, gData.lastEnteredExportTroopSplitAmt, 0, 9999, 5,
				"Troop entries with more than this amount of troops will be divided in more than one JSON entry");
			GI.customInputPanel.SetPanelInfo("Set Troop Entry Split Limit...", "Confirm", () => {
				int splitLimit = int.Parse(numBtns.targetField.text);
				gData.lastEnteredExportTroopSplitAmt = splitLimit;
				SerializableTroopListObj exportedList = new SerializableTroopListObj(JsonHandlingUtils.TroopListToSerializableTroopList
					(GameController.GetCombinedTroopsFromTwoLists
						(battlePhase.battleData.attackerSideInfo.sideArmy, battlePhase.battleData.defenderSideInfo.sideArmy), splitLimit));
				string JSONContent = JsonUtility.ToJson(exportedList);
				Debug.Log(JSONContent);
				GI.textInputPanel.SetPanelInfo("JSON Export Result", "", JSONContent, "Copy to Clipboard", () => {

					GameInterface.CopyToClipboard(GI.textInputPanel.theInputField.text);

				});
				GI.customInputPanel.Close();
				GI.textInputPanel.Open();
			});
		}));

		//JSON export separating attackers from defenders with a user-defined "variable" AND splitting entries if troop amounts go above a certain limit!
		exportOptions.Add(new KeyValuePair<string, UnityAction>("Export to JSON, splitting large troop entries and adding a different variable to attackers and defenders", () => {
			GI.exportOpsPanel.gameObject.SetActive(false);
			string JSONContent = "{ \"troops\":[";
			GI.customInputPanel.Open();
			NumericInputFieldBtns numBtns = GI.customInputPanel.AddNumericInput("Split Limit", true, gData.lastEnteredExportTroopSplitAmt, 0, 9999, 5,
				"Troop entries with more than this amount of troops will be divided in more than one JSON entry");
			InputField addedVarName = GI.customInputPanel.AddTextInput("Added Variable Name", gData.lastEnteredExportAddedVariable, "The name of the variable that will be added to all entries");
			InputField varForAttackers = GI.customInputPanel.AddTextInput("Value for Attackers", gData.lastEnteredExportAttackerVariable, "The value of the added variable for all entries of the attacker army. Add quotes if necessary");
			InputField varForDefenders = GI.customInputPanel.AddTextInput("Value for Defenders", gData.lastEnteredExportDefenderVariable, "The value of the added variable for all entries of the defender army. Add quotes if necessary");
			GI.customInputPanel.SetPanelInfo("Set Options...", "Confirm", () => {
				int splitLimit = int.Parse(numBtns.targetField.text);

				gData.lastEnteredExportTroopSplitAmt = splitLimit;
				gData.lastEnteredExportAddedVariable = addedVarName.text;
				gData.lastEnteredExportAttackerVariable = varForAttackers.text;
				gData.lastEnteredExportDefenderVariable = varForDefenders.text;

				List<SerializedTroop> sTroopList = 
					JsonHandlingUtils.TroopListToSerializableTroopList(battlePhase.battleData.attackerSideInfo.sideArmy, splitLimit);

				for(int i = 0; i < sTroopList.Count; i++) {
					JSONContent = string.Concat(JSONContent,
						JsonHandlingUtils.ToJsonWithExtraVariable(sTroopList[i], addedVarName.text, varForAttackers.text),
						",");
				}

				sTroopList =
					JsonHandlingUtils.TroopListToSerializableTroopList(battlePhase.battleData.defenderSideInfo.sideArmy, splitLimit);

				for (int i = 0; i < sTroopList.Count; i++) {
				JSONContent = string.Concat(JSONContent,
					JsonHandlingUtils.ToJsonWithExtraVariable(sTroopList[i], addedVarName.text, varForDefenders.text),
						i < sTroopList.Count - 1 ? "," : "");
				}

				JSONContent += "]}";
				Debug.Log(JSONContent);
				GI.textInputPanel.SetPanelInfo("JSON Export Result", "", JSONContent, "Copy to Clipboard", () => {

					GameInterface.CopyToClipboard(GI.textInputPanel.theInputField.text);

				});
				GI.customInputPanel.Close();
				GI.textInputPanel.Open();
			});
		}));

		//when done preparing options, open the export ops panel
		GI.exportOpsPanel.Open("Remaining Armies: Export Options", exportOptions);
	}

	public void OpenImportOps() {
		List<KeyValuePair<string, UnityAction>> importOptions =
			new List<KeyValuePair<string, UnityAction>>();

		GameInterface GI = GameInterface.instance;


		importOptions.Add(new KeyValuePair<string, UnityAction>("JSON Array - Don't use if attackers and defenders share troop types", ()=> {
			GI.textInputPanel.SetPanelInfo("JSON Text to Import", "insert the JSON string here", "", "Import", () => {

				SerializedTroop[] readTroops = JsonHandlingUtils.JsonToSerializedTroopArray
					(GI.textInputPanel.theInputField.text);
				if(readTroops == null) {
					ModalPanel.Instance().OkBox("Json Import Failed", "Please check your JSON string, it must be something like [{'name':'troop', 'amount':5}]");
					return;
				}

				List<TroopNumberPair> attackersRemaining = new List<TroopNumberPair>(),
					defendersRemaining = new List<TroopNumberPair>();

				foreach(SerializedTroop tnp in readTroops) {
					//Debug.Log("got this kvp: " + tnp.name + " : " + tnp.amount);

					TroopNumberPair convertedTNP = JsonHandlingUtils.SerializedTroopToTroopNumberPair(tnp);
					if(convertedTNP.troopAmount > 0) {
						if(GameController.IndexOfTroopInTroopList(battlePhase.battleData.defenderSideInfo.sideArmy, convertedTNP.troopTypeID) != -1) {
							defendersRemaining.Add(convertedTNP);
						}else if (GameController.IndexOfTroopInTroopList(battlePhase.battleData.attackerSideInfo.sideArmy, convertedTNP.troopTypeID) != -1) {
							attackersRemaining.Add(convertedTNP);
						}
					}
				}

				battlePhase.battleData.defenderSideInfo.SetPostBattleArmyData_RemainingArmy(defendersRemaining);
				battlePhase.battleData.attackerSideInfo.SetPostBattleArmyData_RemainingArmy(attackersRemaining);

				if (!battlePhase.battleData.BattleEndCheck()) {
					//we only need to update side info displays if the battle hasn't ended yet, 
					//because the delegate would take care of it otherwise
					UpdateArmyDisplays();
				}

				GI.textInputPanel.Close();

			});
			GI.textInputPanel.Open();
			GI.exportOpsPanel.gameObject.SetActive(false);
		}));


		//when done preparing options, open the import ops panel
		GI.exportOpsPanel.Open("Remaining Armies: Import Options", importOptions);
	}
}
