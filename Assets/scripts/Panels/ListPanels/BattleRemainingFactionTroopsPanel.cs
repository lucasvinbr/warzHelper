using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class BattleRemainingFactionTroopsPanel : ListContainerPanel<TroopNumberPair> {

	public TroopList representedTroops;

	public TMP_Text boxHeaderTxt;

	public override void FillEntries() {
		for (int i = 0; i < representedTroops.Count; i++) {
			AddEntry(representedTroops[i]);
		}
	}

	public void MaxAllEntries() {
		for (int i = 0; i < listContainer.childCount; i++) {
			Transform entry = listContainer.GetChild(i);
			BattleResolutionRemainingTroopListEntry entryScript =
				entry.GetComponent<BattleResolutionRemainingTroopListEntry>();
			if (entryScript) {
				entryScript.remainingFieldBtns.MaximizeField();
			}
		}
	}

	public void EmptyAllEntries() {
		for (int i = 0; i < listContainer.childCount; i++) {
			Transform entry = listContainer.GetChild(i);
			BattleResolutionRemainingTroopListEntry entryScript =
				entry.GetComponent<BattleResolutionRemainingTroopListEntry>();
			if (entryScript) {
				entryScript.remainingFieldBtns.MinimizeField();
			}
		}
	}

	/// <summary>
	/// returns a troopNumberPair list with entries using the provided amounts
	/// </summary>
	/// <returns></returns>
	public TroopList BakeIntoArmy() {
		TroopList returnedList = new TroopList();

		for (int i = 0; i < listContainer.childCount; i++) {
			Transform entry = listContainer.GetChild(i);
			BattleResolutionRemainingTroopListEntry entryScript = 
				entry.GetComponent<BattleResolutionRemainingTroopListEntry>();
			if (entryScript) {
				returnedList.Add(entryScript.BakeIntoNewPair());
			}
		}

		return returnedList;
	}

	public void OpenExportOps() {
		SerializableTroopListObj exportedList = 
			new SerializableTroopListObj(JsonHandlingUtils.TroopListToSerializableTroopList(BakeIntoArmy()));

		List<KeyValuePair<string, UnityAction>> exportOptions =
			new List<KeyValuePair<string, UnityAction>>();

		GameInterface GI = GameInterface.instance;

		//add export options now...

		//JSON export!
		exportOptions.Add(new KeyValuePair<string, UnityAction>("Export to JSON", () => {
			string JSONContent = JsonUtility.ToJson(exportedList);
			Debug.Log(JSONContent);
			GI.textInputPanel.SetPanelInfo("JSON Export Result", "", JSONContent, "Copy to Clipboard", () => {

				GameInterface.CopyToClipboard(GI.textInputPanel.theInputField.text);

			});
			GI.textInputPanel.Open();
			GI.exportOpsPanel.gameObject.SetActive(false);
		}));


		//when done preparing options, open the export ops panel
		GI.exportOpsPanel.Open("Remaining Army: Export Options", exportOptions);
	}
}


