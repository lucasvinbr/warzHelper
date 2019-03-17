﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// expandable box, displayed only during the command phase, that shows info about the currently selected commander
/// </summary>
public class CmdPhaseCurCmderInfoBox : MonoBehaviour
{
	public GameObject expandArrow, shrinkArrow;

	public GameObject expandedBox;

	public TMP_Text numTroopsTxt;

	public GameObject troopEntryPrefab;

	public Transform troopEntriesContainer;

	public LayoutTooltipTwoTextEntry zoneInfoEntry;

	public void ToggleExpandedBox() {
		bool shouldExpand = !expandedBox.activeSelf;

		expandedBox.SetActive(shouldExpand);
		expandArrow.SetActive(!shouldExpand);
		shrinkArrow.SetActive(shouldExpand);
	}

	public void SetContent(Commander cmder) {
		//clear troop entries first
		ClearTroopEntries();

		numTroopsTxt.text = "Troops: " + cmder.TotalTroopsContained + "/" + cmder.MaxTroopsCommanded;

		foreach(TroopNumberPair tnp in cmder.troopsContained) {
			AddTroopEntry(tnp);
		}


		//and the zone info
		Zone curZone = GameController.GetZoneByID(cmder.zoneIAmIn);
		zoneInfoEntry.leftTxt.text = curZone.multRecruitmentPoints.ToString("0%");
		zoneInfoEntry.rightTxt.text = curZone.multTrainingPoints.ToString("0%");
	}

	public void ClearTroopEntries() {
		for (int i = 0; i < troopEntriesContainer.childCount; i++) {
			Destroy(troopEntriesContainer.GetChild(i).gameObject);
		}
	}

	public void AddTroopEntry(TroopNumberPair tnp) {
		GameObject newEntry = Instantiate(troopEntryPrefab, troopEntriesContainer);
		LayoutTooltipTwoTextEntry entryScript = newEntry.GetComponent<LayoutTooltipTwoTextEntry>();

		entryScript.leftTxt.text = GameController.GetTroopTypeByID(tnp.troopTypeID).name;
		entryScript.rightTxt.text = tnp.troopAmount.ToString();
	}

}
