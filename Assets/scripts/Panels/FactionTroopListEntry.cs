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
			tierTxt.text = targetTroop.name;
			costTxt.text = targetTroop.pointCost.ToString();
		}
	}

	public void RefreshInfoLabels() {
		costTxt.text = myContent.pointCost.ToString();
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
		Debug.Log("GameInterface.instance.EditTroop(myContent)");
	}



}
