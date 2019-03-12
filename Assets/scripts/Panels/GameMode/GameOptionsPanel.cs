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
public class GameOptionsPanel : GenericOverlayPanel {

	public GameObject mainOpsPanel;

	public FactionsPanel aiFactionsPanel;

	public Toggle fastAiTurnsToggle, autocalcAiBattlesToggle;

	public override void OnEnable() {
		base.OnEnable();
		ResetUI();
	}

	public void ResetUI() {
		aiFactionsPanel.gameObject.SetActive(false);
		GameInfo gameData = GameController.instance.curData as GameInfo;
		fastAiTurnsToggle.isOn = gameData.fastAiTurns;
		autocalcAiBattlesToggle.isOn = gameData.alwaysAutocalcAiBattles;
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

		GameInfo gameData = GameController.instance.curData as GameInfo;
		gameData.fastAiTurns = fastAiTurnsToggle.isOn;
		gameData.alwaysAutocalcAiBattles = autocalcAiBattlesToggle.isOn;
	}
}
