using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditFactionPanel : EditDataPanel<Faction> {

    public InputField facNameField, facInfoField, zonePointsField, cmdPointsField, zoneGarrField, cmdGarrField, maxCmdsField, turnPriorityField; //yay garrField

    public Image factionColorImg, factionIconImg;

	FactionTroopTreeEditPanel troopTreePanel;

    public override void Open(Faction editedFaction, bool isNewEntry)
    {
        //set data
        base.Open(editedFaction, isNewEntry);
        facNameField.text = dataBeingEdited.name;
		facInfoField.text = dataBeingEdited.extraInfo;
		zonePointsField.text = dataBeingEdited.multZonePointAwardOnTurnStart.ToString(CultureInfo.InvariantCulture);
		cmdPointsField.text = dataBeingEdited.multCommanderPointAwardOnTurnStart.ToString(CultureInfo.InvariantCulture);
		zoneGarrField.text = dataBeingEdited.multMaxUnitsInOneGarrison.ToString(CultureInfo.InvariantCulture);
		cmdGarrField.text = dataBeingEdited.multMaxUnitsUnderOneCommander.ToString(CultureInfo.InvariantCulture);
		maxCmdsField.text = dataBeingEdited.extraMaxCommanders.ToString();
		turnPriorityField.text = dataBeingEdited.turnPriority.ToString();
        factionColorImg.color = dataBeingEdited.color;
		//factionIconImg.sprite = TODO get icon by path to file
		troopTreePanel.ImportTroopTreeData(dataBeingEdited.troopTree, dataBeingEdited.maxGarrisonedTroopTier);
    }


	public override bool DataIsValid() {
		if (string.IsNullOrEmpty(facNameField.text)) {
			ModalPanel.Instance().OkBox("Name is Empty", "Please provide a name for the faction.");
			return false;
		}else if(facNameField.text == Rules.NO_FACTION_NAME) {
			ModalPanel.Instance().OkBox("Name cannot be used", "The name provided is used as the 'No Faction' identifier. Please provide another name.");
			return false;
		}

		return true;
	}


	public void CheckIfNameIsAlreadyInUse()
    {
        Faction sameNameFaction = GameController.GetFactionByName(facNameField.text);
        while (sameNameFaction != null && sameNameFaction != dataBeingEdited && sameNameFaction.name != dataBeingEdited.name)
        {
            ModalPanel.Instance().OkBox("Name already in use", "A suffix will be added to this faction's name.");
            facNameField.text += " copy";
            sameNameFaction = GameController.GetFactionByName(facNameField.text);
        }
    }

    public void CloseAndSaveChanges()
    {
		if (!DataIsValid()) return;
		CheckIfNameIsAlreadyInUse();
        dataBeingEdited.name = facNameField.text;
        dataBeingEdited.color = factionColorImg.color;
		dataBeingEdited.extraInfo = facInfoField.text;
		dataBeingEdited.multZonePointAwardOnTurnStart = float.Parse(zonePointsField.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multCommanderPointAwardOnTurnStart = float.Parse(cmdPointsField.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multMaxUnitsInOneGarrison = float.Parse(zoneGarrField.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multMaxUnitsUnderOneCommander = float.Parse(cmdGarrField.text, CultureInfo.InvariantCulture);
		dataBeingEdited.extraMaxCommanders = int.Parse(maxCmdsField.text);
		dataBeingEdited.turnPriority = int.Parse(turnPriorityField.text);
		dataBeingEdited.maxGarrisonedTroopTier = int.Parse(troopTreePanel.maxGarrTroopLvlText.text);
		dataBeingEdited.troopTree = troopTreePanel.BakeIntoTroopTree();
		gameObject.SetActive(false);
		OnWindowIsClosing();
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
			ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes and close the window.", OnConfirmDelete, JustClose, null);
		}
	}

	public override void OnDeleteBtnClicked() {
		if (creatingNewEntry) {
			OnConfirmDelete();
		}
		else {
			ModalPanel.Instance().YesNoBox("Confirm Faction Deletion", "You are about to delete this faction. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		gameObject.SetActive(false);
		GameController.RemoveFaction(dataBeingEdited);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

}
