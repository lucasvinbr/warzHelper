using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopListEntry : ListPanelEntry<TroopType> {

	public Text tierTxt, costTxt;

	public Dropdown troopTypeDropdown;

	public Button selectEntryBtn;

	private static List<Dropdown.OptionData> troopDDownOptions = new List<Dropdown.OptionData>();


	/// <summary>
	/// a variable that should always be set to true whenever a troop type is edited.
	/// this makes sure the troop type dropdown options are refreshed
	/// </summary>
	public static bool troopDDownsAreStale = true;

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
		if (troopDDownsAreStale) {
			ReBakeTroopTypeDDowns();
		}
		troopTypeDropdown.AddOptions(troopDDownOptions);
		troopTypeDropdown.RefreshShownValue();
		troopTypeDropdown.value = GetDDownIndexForTType(myContent.name);
	}

	public void OpenEditTroopPanel() {
		Debug.Log("GameInterface.instance.EditTroop(myContent)");
	}

	/// <summary>
	/// rebuilds troop dropdown data in order to make sure it's in sync with the available troop types
	/// </summary>
	public static void ReBakeTroopTypeDDowns() {
		troopDDownOptions.Clear();
		if (GameController.GuardGameDataExist()) {
			List<TroopType> tTypes = GameController.instance.curData.troopTypes;
			for (int i = 0; i < tTypes.Count; i++) {
				troopDDownOptions.Add(new Dropdown.OptionData(tTypes[i].name));
			}
		}
		else {
			troopDDownOptions.Add(new Dropdown.OptionData("?"));
		}

		troopDDownsAreStale = false;
	}
		
	/// <summary>
	/// returns the dropdown index for the troop type with the name specified, or -1 if it isn't found
	/// </summary>
	/// <param name="troopTypeName"></param>
	/// <returns></returns>
	public static int GetDDownIndexForTType(string troopTypeName) {
		if (troopDDownsAreStale) {
			ReBakeTroopTypeDDowns();
		}
		for(int i = 0; i < troopDDownOptions.Count; i++) {
			if (troopDDownOptions[i].text == troopTypeName) {
				return i;
			}
		}
		return -1;
	}	

}
