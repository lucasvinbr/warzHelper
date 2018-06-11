using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopListEntry : ListPanelEntry<Troop> {

	public Text tierTxt, costTxt;

	public Image backgroundImg;

	public override void SetContent(Troop targetTroop) {
		myContent = targetTroop;
		tierTxt.text = targetTroop.name;
		costTxt.text = targetTroop.pointCost.ToString();
	}

	public void FillDropdownOptions() {
		Debug.Log("get all troops, add them to the dropdown");
	}

	public void RefreshInfoLabels() {
		Debug.Log("get all troops, add them to the dropdown");
	}

	public void OpenEditTroopPanel() {
		Debug.Log("GameInterface.instance.EditTroop(myContent)");
	}
}
