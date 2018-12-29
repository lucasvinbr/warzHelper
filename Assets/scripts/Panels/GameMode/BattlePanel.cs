using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// the battle panel!
/// it will only go away after the battle has been resolved.
/// A battle will only resolve after one of the sides has been completely eliminated
/// </summary>
public class BattlePanel : GrowingOverlayPanel {

	public BattlePhaseMan battlePhase;

	//script for left side and right faction sides
	public BattlePanelFactionSideInfo attackerSide, defenderSide;

	public GameObject mainResolutionOptionsBox, manualResolutionOptionsBox;

	public BattleResolutionPercentagePanel percentageResolutionPanel;

	public BattleResolutionManualPanel manualPanel;

	public Text zoneNameTxt;

	public Image zoneImg;

	[HideInInspector]
	public Zone curWarzone;

	public CanvasGroup resolutionBtnsGroup;

	public void OpenWithFilledInfos(Faction attackerFaction, Faction defenderFaction, Zone warZone) {
		gameObject.SetActive(true);
		
		curWarzone = warZone;

		attackerSide.SetContent(attackerFaction);
		attackerSide.SetArmyData(null,
			GameController.CmdersToTroopContainers
			(GameController.GetCommandersOfFactionInZone(warZone, attackerFaction)));

		defenderSide.SetContent(defenderFaction);
		defenderSide.SetArmyData(warZone,
			GameController.CmdersToTroopContainers
			(GameController.GetCommandersOfFactionInZone(warZone, defenderFaction)));

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
	}

	public void OpenPercentageResolution() {
		percentageResolutionPanel.gameObject.SetActive(true);
	}

	public void OpenManualResolution() {
		manualPanel.SetupAndOpen(attackerSide.sideArmy, defenderSide.sideArmy);
	}

	public void AutocalcResolution() {
		//TODO autocalc!
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

	public void OnBattleResolved() {
		Shrink(TellPhaseManAboutResolution);
	}

	public void TellPhaseManAboutResolution() {
		battlePhase.OnBattleResolved(curWarzone);
	}

	public void ResetUI() {
		manualResolutionOptionsBox.SetActive(false);
		mainResolutionOptionsBox.SetActive(true);
		resolutionBtnsGroup.interactable = true;
	}
}
