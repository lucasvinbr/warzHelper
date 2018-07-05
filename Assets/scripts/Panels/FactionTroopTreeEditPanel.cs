using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopTreeEditPanel : ListContainerPanel<TroopType> {

	public Text maxGarrTroopLvlText;

	public Transform maxGarrTroopLvlDelimiter;

	public FactionTroopListEntry selectedEntry;

	public Color selectedEntryColor, unselectedEntryColor;

	public Button editEntryBtn, delEntryBtn, addEntryBtn;


	public void SelectTierEntry(FactionTroopListEntry theEntry) {
		if (selectedEntry) {
			selectedEntry.selectEntryBtn.image.color = unselectedEntryColor;
		}
		selectedEntry = theEntry;
		selectedEntry.selectEntryBtn.image.color = selectedEntryColor;
		editEntryBtn.interactable = true;
		delEntryBtn.interactable = CheckIfCanDeleteMoreEntries();
	}

	/// <summary>
	/// we can only delete more entries if we have more than one tier entry, because we can't have a faction with no troop types
	/// </summary>
	/// <returns></returns>
	public bool CheckIfCanDeleteMoreEntries() {
		return listContainer.childCount > 2;
	}

	public void EditTierEntry() {
		selectedEntry.OpenEditTroopPanel();
	}

	public void StartDeleteTierProcedure() {
		StartCoroutine(DeleteSelectedTierEntry());
	}

	public IEnumerator DeleteSelectedTierEntry() {
		if (selectedEntry) {
			Destroy(selectedEntry.gameObject);
			yield return null;
			UpdateTreeTierValues();
			RefreshMaxGarrLvlText();
			editEntryBtn.interactable = false;
			delEntryBtn.interactable = false;
		}
	}

	/// <summary>
	/// updates tier numbers for all entries
	/// </summary>
	public void UpdateTreeTierValues() {
		int curTierNumber = 1;

		for(int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				entryScript.tierTxt.text = curTierNumber.ToString();
				curTierNumber++;
			}
		}
	}

	/// <summary>
	/// refreshes troop option names.
	/// should be called when rebuilding the tree or after a troop type has been edited
	/// </summary>
	public void UpdateTreeTroopOptions() {
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				entryScript.RefreshInfoLabels();
			}
		}
	}

	public void RefreshMaxGarrLvlText() {
		int delimiterPos = maxGarrTroopLvlDelimiter.GetSiblingIndex();
		maxGarrTroopLvlText.text = delimiterPos.ToString();
	}

	public void IncrementMaxGarrTroopLvl() {
		int delimiterPos = maxGarrTroopLvlDelimiter.GetSiblingIndex();
		if(delimiterPos < listContainer.childCount - 2) { //can't go below the "add tier" btn
			maxGarrTroopLvlDelimiter.SetSiblingIndex(delimiterPos + 1);
			RefreshMaxGarrLvlText();
		}
	}

	public void SetMaxGarrTroopLvl(int tier) {
		if (tier < 0) tier = 0;
		else if (tier > listContainer.childCount - 2) tier = listContainer.childCount - 2;
		maxGarrTroopLvlDelimiter.SetSiblingIndex(tier);
		RefreshMaxGarrLvlText();
	}

	public void DecrementMaxGarrTroopLvl() {
		int delimiterPos = maxGarrTroopLvlDelimiter.GetSiblingIndex();
		if (delimiterPos >= 1) {
			maxGarrTroopLvlDelimiter.SetSiblingIndex(delimiterPos - 1);
			RefreshMaxGarrLvlText();
		}
	}

	public List<string> BakeIntoTroopTree() {
		List<string> returnedTree = new List<string>();
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				returnedTree.Add(entryScript.troopTypeDropdown.captionText.text);
			}
		}

		return returnedTree;
	}

	public void ImportTroopTreeData(List<string> tree, int maxGarrLvl) {
		for(int i = 0; i < tree.Count; i++) {
			AddEntry(GameController.GetTroopTypeByName(tree[i]));
		}

		SetMaxGarrTroopLvl(maxGarrLvl);

		UpdateTreeTierValues();

		addEntryBtn.transform.SetAsLastSibling();
	}

	public override void ClearList() {
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				Destroy(entry.gameObject);
			}
		}
	}

	public override void OnEnable() {
		//do nothing; the faction panel takes care of clearing and refilling the tree
	}

	/// <summary>
	/// adds a new troop tier entry using the same troop type as the previous entry.
	/// if there is no previous entry, use the first troop type in the saved data list
	/// </summary>
	public void AddTroopTier() {
		TroopType newTierData = null;
		//if the container has more children than the maxGarrisonDelimiter and the "add tier" button...
		if(listContainer.childCount > 2) {
			FactionTroopListEntry entryScript = listContainer.GetChild(listContainer.childCount - 2).
				GetComponent<FactionTroopListEntry>();
			if (!entryScript) {//maybe it's the maxGarrLvlDelimiter
				entryScript = listContainer.GetChild(listContainer.childCount - 3).
				GetComponent<FactionTroopListEntry>();
			}
			if (entryScript) {
				newTierData = GameController.GetTroopTypeByName(entryScript.troopTypeDropdown.captionText.text);
			}
		}

		if(newTierData == null) {
			if (GameController.GuardGameDataExist()) {
				newTierData = GameController.instance.curData.troopTypes[0];
			}
			
		}

		AddEntry(newTierData);

		UpdateTreeTierValues();

		addEntryBtn.transform.SetAsLastSibling();
	}

	public override GameObject AddEntry(TroopType entryData) {
		GameObject newEntry = Instantiate(entryPrefab);
		newEntry.transform.SetParent(listContainer, false);
		FactionTroopListEntry entryScript = newEntry.GetComponent<FactionTroopListEntry>();
		entryScript.SetContent(entryData);
		entryScript.selectEntryBtn.onClick.AddListener(()=>SelectTierEntry(entryScript));

		newEntry.transform.SetSiblingIndex(listContainer.childCount - 2);
		return newEntry;
	}
}
