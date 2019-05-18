using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// the game options panel controls some details from the AI options panel
/// (which has no script of its own)
/// </summary>
public class GameOptionsPanel : GenericOverlayPanel {

	public GameObject mainOpsPanel;

	public FactionsPanel aiFactionsPanel;

	public Toggle fastAiTurnsToggle, autocalcAiBattlesToggle, unifyBattlePhaseToggle;

	//TODO unified battles toggle doesn't really match the Ai Options panel,
	//but for now there's not many other game options menus,
	//and not enough general game options to make a new menu

	public override void OnEnable() {
		base.OnEnable();
		ResetUI();
	}

	public void ResetUI() {
		aiFactionsPanel.gameObject.SetActive(false);
		GameInfo gameData = GameController.CurGameData;
		fastAiTurnsToggle.isOn = gameData.fastAiTurns;
		autocalcAiBattlesToggle.isOn = gameData.alwaysAutocalcAiBattles;
		unifyBattlePhaseToggle.isOn = gameData.unifyBattlePhase;
		mainOpsPanel.SetActive(true);
	}

	public void GoToAIPanel() {
		aiFactionsPanel.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}

	public void ApplyAiOptions() {
		for (int i = 0; i < aiFactionsPanel.listContainer.childCount; i++) {
			Transform entry = aiFactionsPanel.listContainer.GetChild(i);
			FactionsAIPanelListEntry entryScript =
				entry.GetComponent<FactionsAIPanelListEntry>();
			if (entryScript) {
				entryScript.myContent.isPlayer = !entryScript.isAIToggle.isOn;
			}
		}

		GameInfo gameData = GameController.CurGameData;
		gameData.fastAiTurns = fastAiTurnsToggle.isOn;
		gameData.alwaysAutocalcAiBattles = autocalcAiBattlesToggle.isOn;
		gameData.unifyBattlePhase = unifyBattlePhaseToggle.isOn;
	}
}
