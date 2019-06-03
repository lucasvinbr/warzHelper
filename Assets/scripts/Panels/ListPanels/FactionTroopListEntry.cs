using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FactionTroopListEntry : ListPanelEntry<TroopType> {

	public Text tierTxt, costTxt;

	public Dropdown troopTypeDropdown;

	/// <summary>
	/// used by parent containers' scripts
	/// </summary>
	public Button selectEntryBtn;

	public DirtableOverlayPanel parentDirtablePanel;

	public UnityAction actionOnEditTroopType; 

	public override void SetContent(TroopType targetTroop) {
		if (targetTroop != null) {
			myContent = targetTroop;
			costTxt.text = targetTroop.pointCost.ToString();
		}
	}

	public void SetContentAccordingToDDown(int ddownValue) {
		SetContent(GameController.GetTroopTypeByName(troopTypeDropdown.options[ddownValue].text));

		if (parentDirtablePanel) parentDirtablePanel.isDirty = true;

	}

	public void RefreshInfoLabels(bool alsoRefillDropdown = true) {
		costTxt.text = myContent.pointCost.ToString();
		if (alsoRefillDropdown) ReFillDropdownOptions();
	}

	public void ReFillDropdownOptions() {
		troopTypeDropdown.ClearOptions();
		if (GameInterface.troopDDownsAreStale) {
			GameInterface.ReBakeTroopTypeDDowns();
		}
		troopTypeDropdown.AddOptions(GameInterface.troopDDownOptions);
		troopTypeDropdown.RefreshShownValue();
		troopTypeDropdown.value = GameInterface.GetDDownIndexForTType(myContent.name);
	}

	public void OpenEditTroopPanel() {
		GameInterface.instance.EditTroopType(myContent, false);
	}

	public void CreateNewTroopTypeForMe() {
		TroopType newTT = new TroopType(GameController.instance.LastRelevantTType);
		SetContent(newTT);
		RefreshInfoLabels();
		OpenEditTroopPanel();
		GameInterface.instance.editTroopPanel.onDoneEditing += actionOnEditTroopType;
		if (parentDirtablePanel) parentDirtablePanel.isDirty = true;
		troopTypeDropdown.Hide();
	}

}
