using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditZonePanel : EditDataPanel<Zone> {

    public InputField nameInput, eInfoInput, multTrainingInput, multRecruitInput, multMaxGarrInput, startPointsInput;
    public Dropdown ownerFactionDropdown;

	public ZonePointMultInputPredicter trainingMultPredicter, recruitMultPredicter;
	public ZoneGarrMultInputPredicter garrMultPredicter;

	public Image zoneIconImg;


	public override void Open(Zone editedZone, bool isNewEntry) {
		base.Open(editedZone, isNewEntry);
		nameInput.text = dataBeingEdited.name;
		eInfoInput.text = dataBeingEdited.extraInfo;
		multTrainingInput.text = dataBeingEdited.multTrainingPoints.ToString(CultureInfo.InvariantCulture);
		multRecruitInput.text = dataBeingEdited.multRecruitmentPoints.ToString(CultureInfo.InvariantCulture);
		multMaxGarrInput.text = dataBeingEdited.multMaxUnitsInGarrison.ToString(CultureInfo.InvariantCulture);
		startPointsInput.text = dataBeingEdited.pointsGivenAtGameStart.ToString();
		ReFillDropdownOptions();
		//zoneIconImg.sprite = TODO get icon by path to file
	}

	public void ReFillDropdownOptions() {
		ownerFactionDropdown.ClearOptions();
		if (GameInterface.factionDDownsAreStale) {
			GameInterface.ReBakeTroopTypeDDowns();
		}
		ownerFactionDropdown.AddOptions(GameInterface.factionDDownOptions);
		ownerFactionDropdown.RefreshShownValue();
		ownerFactionDropdown.value = GameInterface.GetDDownIndexForFaction(dataBeingEdited.ownerFaction);
		RefreshFactionForPredicters();
	}

	public void RefreshFactionForPredicters() {
		if (trainingMultPredicter) {
			trainingMultPredicter.curOwnerFaction = ownerFactionDropdown.captionText.text;
		}
		if (recruitMultPredicter) {
			recruitMultPredicter.curOwnerFaction = ownerFactionDropdown.captionText.text;
		}
	}

	public override bool DataIsValid() {
		if (string.IsNullOrEmpty(nameInput.text)) {
			ModalPanel.Instance().OkBox("Name is Empty", "Please provide a name for the zone.");
			return false;
		}

		return true;
	}


	public void CheckIfNameIsAlreadyInUse() {
		Zone sameNameZone = GameController.GetZoneByName(nameInput.text);
		while (sameNameZone != null && sameNameZone != dataBeingEdited && sameNameZone.name != dataBeingEdited.name) {
			ModalPanel.Instance().OkBox("Name already in use", "A suffix will be added to this zone's name.");
			nameInput.text += " copy";
			sameNameZone = GameController.GetZoneByName(nameInput.text);
		}
	}

	public void CloseAndSaveChanges() {
		if (!DataIsValid()) return;
		CheckIfNameIsAlreadyInUse();
		ZoneSpot thisZoneSpot = World.GetZoneSpotByZoneName(dataBeingEdited.name);
		dataBeingEdited.name = nameInput.text;
		dataBeingEdited.extraInfo = eInfoInput.text;
		dataBeingEdited.multTrainingPoints = float.Parse(multTrainingInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multRecruitmentPoints = float.Parse(multRecruitInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multMaxUnitsInGarrison = float.Parse(multMaxGarrInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.pointsGivenAtGameStart = int.Parse(startPointsInput.text);
		dataBeingEdited.ownerFaction = ownerFactionDropdown.captionText.text;
		dataBeingEdited.coords = new Vector2(thisZoneSpot.transform.localPosition.x, thisZoneSpot.transform.localPosition.z);
		thisZoneSpot.RefreshDataDisplay();
		OnWindowIsClosing();
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
			ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes (including zone positioning and links) and close the window.", OnConfirmDelete, CloseWithoutSaving, null);
		}
	}

	public override void OnDeleteBtnClicked() {
		if (creatingNewEntry) {
			OnConfirmDelete();
		}
		else {
			ModalPanel.Instance().YesNoBox("Confirm Zone Deletion", "You are about to delete this zone. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		gameObject.SetActive(false);
		World.GetZoneSpotByZoneName(dataBeingEdited.name).DeleteThisSpot();
		GameController.RemoveZone(dataBeingEdited);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

    public void EditZoneLocation()
    {
        //TODO menu closes, zone movement mode is activated.
		//zone is "glued" to the user's mouse.
		//clicking will set the new position.
        //menu opens again once a spot is defined
    }

	public void EditZoneLinks() {
		//TODO menu closes, zone linking mode is activated.
		//clicking another zone will create a link between this zone and the target.
		//menu opens again once a "done" button is clicked or ESC is pressed
	}




}
