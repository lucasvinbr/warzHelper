using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleRemainingFactionTroopsPanel : ListContainerPanel<TroopNumberPair> {

	public List<TroopNumberPair> representedTroops;

	public TMP_Text boxHeaderTxt;

	public override void FillEntries() {
		for (int i = 0; i < representedTroops.Count; i++) {
			AddEntry(representedTroops[i]);
		}
	}

	public void MaxAllEntries() {
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			BattleResolutionRemainingTroopListEntry entryScript =
				entry.GetComponent<BattleResolutionRemainingTroopListEntry>();
			if (entryScript) {
				entryScript.remainingFieldBtns.MaximizeField();
			}
		}
	}

	public void EmptyAllEntries() {
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			BattleResolutionRemainingTroopListEntry entryScript =
				entry.GetComponent<BattleResolutionRemainingTroopListEntry>();
			if (entryScript) {
				entryScript.remainingFieldBtns.MinimizeField();
			}
		}
	}

	public List<TroopNumberPair> BakeIntoArmy() {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();

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
}


