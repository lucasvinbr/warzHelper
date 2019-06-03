using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditMercCaravanPanel : EditDataPanel<MercCaravan> {

	public ColorPanelUser caravanColorInput;

	public FactionTroopListEntry trainedTroopType;

	private void Start() {
		trainedTroopType.actionOnEditTroopType = UpdateTroopTypeOptions;
	} 

	/// <summary>
	/// opens the edit troop panel for the target entry's Troop type
	/// </summary>
	/// <param name="targetEntry"></param>
	public void EditPickedTroopType() {
		trainedTroopType.OpenEditTroopPanel();
		GameInterface.instance.editTroopPanel.onDoneEditing += UpdateTroopTypeOptions;
	}

	/// <summary>
	/// refreshes troop option names.
	/// should be called when rebuilding the tree or after a troop type has been edited
	/// </summary>
	public void UpdateTroopTypeOptions() {
		trainedTroopType.RefreshInfoLabels();
	}

	public override void Open(MercCaravan editedMC, bool isNewEntry)
    {
		//set data
        base.Open(editedMC, isNewEntry);
        caravanColorInput.colorImg.color = dataBeingEdited.caravanColor;
		trainedTroopType.SetContent(GameController.GetTroopTypeByID(dataBeingEdited.containedTroopType));
		trainedTroopType.ReFillDropdownOptions();
		isDirty = false;
    }


	public override bool DataIsValid() {
		//no checks for now
		return true;
	}


    public void CloseAndSaveChanges()
    {
		if (!DataIsValid()) return;
        dataBeingEdited.caravanColor = caravanColorInput.colorImg.color;
		dataBeingEdited.containedTroopType = trainedTroopType.myContent.ID;
		gameObject.SetActive(false);
		OnWindowIsClosing();
		dataBeingEdited.MeIn3d.RefreshDataDisplay();
	}

	public void JustClose() {
		gameObject.SetActive(false);
		OnWindowIsClosing();
	}

	public override void OnCloseBtnClicked() {
		if (creatingNewEntry) {
			CloseAndSaveChanges();
		}
		else {
			if (isDirty) {
				ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes and close the window.", CloseAndSaveChanges, JustClose, null);
			}
			else {
				JustClose();
			}
			
		}
	}

	public override void OnDeleteBtnClicked() {
		if (creatingNewEntry) {
			OnConfirmDelete();
		}
		else {
			ModalPanel.Instance().YesNoBox("Confirm Caravan Deletion", "You are about to delete this mercenary caravan. This cannot be undone unless this game is reloaded without saving. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		gameObject.SetActive(false);
		GameController.RemoveMercCaravan(dataBeingEdited);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

}
