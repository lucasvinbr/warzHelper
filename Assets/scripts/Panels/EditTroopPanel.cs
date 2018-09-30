using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditTroopPanel : EditDataPanel<TroopType> {

    public InputField nameInput, pointCostInput, autocalcPowerInput, extraInfoInput;

	public override void Open(TroopType editedTroop, bool isNewEntry) {
		base.Open(editedTroop, isNewEntry);
		nameInput.text = dataBeingEdited.name;
		pointCostInput.text = dataBeingEdited.pointCost.ToString(CultureInfo.InvariantCulture);
		autocalcPowerInput.text = dataBeingEdited.autoResolvePower.ToString(CultureInfo.InvariantCulture);
		extraInfoInput.text = dataBeingEdited.extraInfo;
	}

	public override bool DataIsValid() {
		if (string.IsNullOrEmpty(nameInput.text)) {
			ModalPanel.Instance().OkBox("Name is Empty", "Please provide a name for the troop type.");
			return false;
		}

		return true;
	}


	public void CheckIfNameIsAlreadyInUse() {
		TroopType sameNameTroop = GameController.GetTroopTypeByName(nameInput.text);
		while (sameNameTroop != null && sameNameTroop != dataBeingEdited && sameNameTroop.name != dataBeingEdited.name) {
			ModalPanel.Instance().OkBox("Name already in use", "A suffix will be added to this troop's name.");
			nameInput.text += " copy";
			sameNameTroop = GameController.GetTroopTypeByName(nameInput.text);
		}
	}

	public void CloseAndSaveChanges() {
		if (!DataIsValid()) return;
		CheckIfNameIsAlreadyInUse();
		dataBeingEdited.name = nameInput.text;
		dataBeingEdited.pointCost = int.Parse(pointCostInput.text);
		dataBeingEdited.autoResolvePower = float.Parse(autocalcPowerInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.extraInfo = extraInfoInput.text;
		GameInterface.troopDDownsAreStale = true;
		GameController.instance.LastRelevantTType = dataBeingEdited;
		gameObject.SetActive(false);
		OnWindowIsClosing();
	}

	public void CloseWithoutSaving() {
		gameObject.SetActive(false);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

	public override void OnCloseBtnClicked() {
		if (creatingNewEntry) {
			CloseAndSaveChanges();
		}
		else {
			ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes and close the window.", CloseAndSaveChanges, CloseWithoutSaving, null);
		}
	}

	public override void OnDeleteBtnClicked() {
		if (creatingNewEntry) {
			OnConfirmDelete();
		}
		else {
			ModalPanel.Instance().YesNoBox("Confirm Troop Type Deletion", "You are about to delete this troop type. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		gameObject.SetActive(false);
		GameController.RemoveTroopType(dataBeingEdited);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

}
