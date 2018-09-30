using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopListEntry : ListPanelEntry<TroopType> {

	public Text tierTxt, costTxt;

	public Dropdown troopTypeDropdown;

	public Button selectEntryBtn;

	public override void SetContent(TroopType targetTroop) {
		if (targetTroop != null) {
			myContent = targetTroop;
			costTxt.text = targetTroop.pointCost.ToString();
		}
	}

	public void SetContentAccordingToDDown(int ddownValue) {
		SetContent(GameController.GetTroopTypeByName(troopTypeDropdown.options[ddownValue].text));
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
		GameInterface.instance.editFactionPanel.troopTreePanel.CreateNewTroopTypeForEntry(this);
		troopTypeDropdown.Hide();
	}

}
