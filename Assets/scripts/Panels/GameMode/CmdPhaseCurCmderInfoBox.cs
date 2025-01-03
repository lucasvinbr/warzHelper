using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// expandable box, displayed only during the command phase, that shows info about the currently selected commander(s)
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

		numTroopsTxt.text = "Troops: " + cmder.troopsContained.TotalTroopAmount + "/" + cmder.MaxTroopsCommanded;

		foreach(TroopNumberPair tnp in cmder.troopsContained) {
			AddTroopEntry(tnp);
		}


		//and the zone info
		Zone curZone = GameController.GetZoneByID(cmder.zoneIAmIn);
		zoneInfoEntry.leftTxt.text = curZone.multRecruitmentPoints.ToString("0%");
		zoneInfoEntry.rightTxt.text = curZone.multTrainingPoints.ToString("0%");
	}

	/// <summary>
	/// variation used for getting info from all owned commanders in the zone where the selected cmder is in
	/// </summary>
	/// <param name="cmdersArmy"></param>
	/// <param name="cmdersCount"></param>
	/// <param name="sampleCmder"></param>
	public void SetContent(TroopList cmdersArmy, int cmdersCount, TroopList commandableCmdersArmy, int commandableCmdersCount, Commander sampleCmder) {
		//clear troop entries first
		ClearTroopEntries();

		numTroopsTxt.text = "Troops: " + cmdersArmy.TotalTroopAmount + "/" + sampleCmder.MaxTroopsCommanded * cmdersCount;

		if(commandableCmdersCount != cmdersCount)
		{
			numTroopsTxt.text += string.Concat("\n(", commandableCmdersArmy.TotalTroopAmount, "/", sampleCmder.MaxTroopsCommanded * commandableCmdersCount);
		}

		foreach (TroopNumberPair tnp in cmdersArmy) {
            if (commandableCmdersCount != cmdersCount)
            {
				AddTroopEntry(tnp, commandableCmdersArmy.GetTroopsOfType(tnp.troopTypeID).troopAmount);
			}
			else
			{
                AddTroopEntry(tnp);
            }
            
		}


		//and the zone info
		Zone curZone = GameController.GetZoneByID(sampleCmder.zoneIAmIn);
		zoneInfoEntry.leftTxt.text = curZone.multRecruitmentPoints.ToString("0%");
		zoneInfoEntry.rightTxt.text = curZone.multTrainingPoints.ToString("0%");
	}

	public void ClearTroopEntries() {
		for (int i = 0; i < troopEntriesContainer.childCount; i++) {
			Destroy(troopEntriesContainer.GetChild(i).gameObject);
		}
	}

	public void AddTroopEntry(TroopNumberPair tnp, int troopAmountInParenthesis = -1) {
		GameObject newEntry = Instantiate(troopEntryPrefab, troopEntriesContainer);
		LayoutTooltipTwoTextEntry entryScript = newEntry.GetComponent<LayoutTooltipTwoTextEntry>();

		entryScript.leftTxt.text = GameController.GetTroopTypeByID(tnp.troopTypeID).name;

        if (troopAmountInParenthesis != -1)
        {
            entryScript.rightTxt.text = string.Concat(tnp.troopAmount.ToString(), " (", troopAmountInParenthesis.ToString(), ")");
        }
        else
        {
            entryScript.rightTxt.text = tnp.troopAmount.ToString();
        }
    }

}
