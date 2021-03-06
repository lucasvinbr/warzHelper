﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopTreeEditPanel : ListContainerPanel<TroopType> {

	public Text maxGarrTroopLvlText;

	public Transform maxGarrTroopLvlDelimiter;

	public FactionTroopListEntry selectedEntry;

	public Color selectedEntryColor, unselectedEntryColor;

	public Button editEntryBtn, delEntryBtn, moveEntryUpBtn, moveEntryDownBtn, addEntryBtn;


	public void SelectTierEntry(FactionTroopListEntry theEntry) {
		if (selectedEntry) {
			selectedEntry.selectEntryBtn.image.color = unselectedEntryColor;
		}
		selectedEntry = theEntry;
		selectedEntry.selectEntryBtn.image.color = selectedEntryColor;
		SetEntryButtonsInteractable(true);
		delEntryBtn.interactable = CheckIfCanDeleteMoreEntries();
	}

	/// <summary>
	/// set interactable for the delete, edit and move buttons that should only be usable when one of the troop tiers is selected
	/// </summary>
	/// <param name="interactable"></param>
	public void SetEntryButtonsInteractable(bool interactable) {
		editEntryBtn.interactable = interactable;
		delEntryBtn.interactable = interactable;
		moveEntryUpBtn.interactable = interactable;
		moveEntryDownBtn.interactable = interactable;
	}

	/// <summary>
	/// we can only delete more entries if we have more than one tier entry, because we can't have a faction with no troop types
	/// </summary>
	/// <returns></returns>
	public bool CheckIfCanDeleteMoreEntries() {
		return listContainer.childCount > 3;
	}

	/// <summary>
	/// opens the edit troop panel for the selected entry's Troop type
	/// </summary>
	public void EditTierEntry() {
		selectedEntry.OpenEditTroopPanel();
		GameInterface.instance.editTroopPanel.onDoneEditing += UpdateTreeTroopOptions;
	}

	/// <summary>
	/// opens the edit troop panel for the target entry's Troop type
	/// </summary>
	/// <param name="targetEntry"></param>
	public void EditTierEntry(FactionTroopListEntry targetEntry) {
		targetEntry.OpenEditTroopPanel();
		GameInterface.instance.editTroopPanel.onDoneEditing += UpdateTreeTroopOptions;
	}

	/// <summary>
	/// moves the selected entry up (false) or down (true) in the troop tree. Down is actually increasing the entry's tier
	/// </summary>
	/// <param name="down"></param>
	public void MoveSelectedEntry(bool down) {
		if (selectedEntry) {
			int selectedEntryIndex = selectedEntry.transform.GetSiblingIndex();
			int increment = down ? 1 : -1;
			selectedEntryIndex += increment;
			if(selectedEntryIndex < 0 || selectedEntryIndex > listContainer.childCount - 2) {
				//can't move in that direction, already in the limit
				return;
			}
			else {
				//check if we're not going to mess with the garrison delimiter
				//"invalid" here means it's probably the delimiter
				bool replacedEntryIsValid = listContainer.GetChild(selectedEntryIndex).GetComponent<FactionTroopListEntry>();
				if (!replacedEntryIsValid) selectedEntryIndex += increment;
				if (selectedEntryIndex < 0 || selectedEntryIndex > listContainer.childCount - 2) {
					//can't move in that direction, we'd only be messing with the garrison delimiter
					return;
				}

				//if we had to go past the garrison delimiter,
				//we'll probably have to relocate it after moving the selected entry
				if (!replacedEntryIsValid) {
					increment = maxGarrTroopLvlDelimiter.GetSiblingIndex(); //reusing the increment variable here
				}

				selectedEntry.transform.SetSiblingIndex(selectedEntryIndex);
				UpdateTreeTierValues();

				if (!replacedEntryIsValid) {
					SetMaxGarrTroopLvl(increment);
				}

				GameInterface.instance.editFactionPanel.isDirty = true;
			}
		}
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
			SetEntryButtonsInteractable(false);
			GameInterface.instance.editFactionPanel.isDirty = true;
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
			GameInterface.instance.editFactionPanel.isDirty = true;
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
			GameInterface.instance.editFactionPanel.isDirty = true;
		}
	}

	public List<int> BakeIntoTroopTree() {
		List<int> returnedTree = new List<int>();
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				returnedTree.Add(entryScript.myContent.ID);
			}
		}

		return returnedTree;
	}

	public void ImportTroopTreeData(List<int> tree, int maxGarrLvl) {
		for(int i = 0; i < tree.Count; i++) {
			AddEntry(GameController.GetTroopTypeByID(tree[i]));
		}

		addEntryBtn.transform.SetAsLastSibling();

		SetMaxGarrTroopLvl(maxGarrLvl);


		UpdateTreeTierValues();
	}

	public override void ClearList() {
		for (int i = 0; i < listContainer.childCount - 1; i++) {
			Transform entry = listContainer.GetChild(i);
			FactionTroopListEntry entryScript = entry.GetComponent<FactionTroopListEntry>();
			if (entryScript) {
				Destroy(entry.gameObject);
			}
		}

		SetEntryButtonsInteractable(false);
	}

	public override void OnEnable() {
		//do nothing; the faction panel takes care of clearing and refilling the tree, and this is no overlay panel, so we shouldnt increment the overlay level
	}

	/// <summary>
	/// (does a clearList)
	/// </summary>
	public override void OnDisable() {
		//this is no overlay panel, so we shouldnt increment/decrement the overlay level
		//...but, in order for the tiers to be shown correctly the next time this panel is opened,
		//we must clear our list beforehand
		ClearList();
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
				newTierData = GameController.instance.LastRelevantTType;
			}
			
		}
		AddEntry(newTierData);

		addEntryBtn.transform.SetAsLastSibling();

		UpdateTreeTierValues();

		GameInterface.instance.editFactionPanel.isDirty = true;
	}

	public override ListPanelEntry<TroopType> AddEntry(TroopType entryData) {
		GameObject newEntry = Instantiate(entryPrefab);
		newEntry.transform.SetParent(listContainer, false);
		FactionTroopListEntry entryScript = newEntry.GetComponent<FactionTroopListEntry>();
		entryScript.SetContent(entryData);
		entryScript.ReFillDropdownOptions();
		entryScript.selectEntryBtn.onClick.AddListener(()=>SelectTierEntry(entryScript));
		entryScript.parentDirtablePanel = GameInterface.instance.editFactionPanel;
		entryScript.actionOnEditTroopType = UpdateTreeTroopOptions;

		newEntry.transform.SetSiblingIndex(listContainer.childCount - 2);
		return entryScript;
	}

	/// <summary>
	/// called by the "create new troop..." option from the demanding troop tier entry.
	/// should open the edit troop type panel for a new TT and then assign that new TT to the demandingEntry
	/// </summary>
	/// <param name="theDemandingEntry"></param>
	public void CreateNewTroopTypeForEntry(FactionTroopListEntry theDemandingEntry) {
		TroopType newTT = new TroopType(GameController.instance.LastRelevantTType);
		theDemandingEntry.SetContent(newTT);
		theDemandingEntry.RefreshInfoLabels();
		EditTierEntry(theDemandingEntry);
		GameInterface.instance.editFactionPanel.isDirty = true;
	}
}
