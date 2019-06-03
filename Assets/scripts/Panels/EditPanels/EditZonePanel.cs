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
		isDirty = false;
	}

	public void ReFillDropdownOptions() {
		ownerFactionDropdown.ClearOptions();
		if (GameInterface.factionDDownsAreStale) {
			GameInterface.ReBakeFactionDDowns();
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
		ZoneSpot mySpot = dataBeingEdited.MyZoneSpot; //we must get it before applying the new name (we get the spot by name if it's not cached)
		dataBeingEdited.name = nameInput.text;
		dataBeingEdited.extraInfo = eInfoInput.text;
		dataBeingEdited.multTrainingPoints = float.Parse(multTrainingInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multRecruitmentPoints = float.Parse(multRecruitInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.multMaxUnitsInGarrison = float.Parse(multMaxGarrInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.pointsGivenAtGameStart = int.Parse(startPointsInput.text);
		dataBeingEdited.ownerFaction = GameController.GetFactionIDByName(ownerFactionDropdown.captionText.text);
		dataBeingEdited.coords = new Vector2(mySpot.transform.localPosition.x,
			mySpot.transform.localPosition.z);
		mySpot.RefreshDataDisplay();
		gameObject.SetActive(false);
		OnWindowIsClosing();
	}

	public void CloseWithoutSaving() {
		gameObject.SetActive(false);
		dataBeingEdited.MyZoneSpot.transform.position = dataBeingEdited.CoordsForWorld;

		MercCaravan localCaravan = GameController.GetMercCaravanInZone(dataBeingEdited.ID);
		if (localCaravan != null) localCaravan.MeIn3d.InstantlyUpdatePosition();

		foreach (int zoneID in dataBeingEdited.linkedZones) {
			Zone linkedZone = GameController.GetZoneByID(zoneID);
			World.GetLinkLineBetween(dataBeingEdited, linkedZone).UpdatePositions();
		}
		OnWindowIsClosing();
	}

	public override void OnCloseBtnClicked() {
		if (creatingNewEntry) {
			CloseAndSaveChanges();
		}
		else {
			if (isDirty) {
				ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes (including zone positioning) and close the window.", CloseAndSaveChanges, CloseWithoutSaving, null);
			}
			else {
				CloseWithoutSaving();
			}
		}
	}

	public override void OnDeleteBtnClicked() {
		if (creatingNewEntry) {
			OnConfirmDelete();
		}
		else {
			ModalPanel.Instance().YesNoBox("Confirm Zone Deletion", "You are about to delete this zone. Any caravans in this zone will also be deleted. This cannot be undone unless this game is reloaded without saving. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		gameObject.SetActive(false);
		dataBeingEdited.MyZoneSpot.DeleteThisSpot();
		GameController.RemoveZone(dataBeingEdited);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

	public void EditZoneLocation() {
		GameInterface.instance.DisableAndStoreAllOpenOverlayPanels();
		World.BeginCustomZonePlacement(() => {
			dataBeingEdited.MyZoneSpot.transform.position =
			World.instance.zonePlacerScript.zoneBlueprint.position;
			isDirty = true;
			GameInterface.instance.RestoreOpenOverlayPanels();

			MercCaravan localCaravan = GameController.GetMercCaravanInZone(dataBeingEdited.ID);
			if (localCaravan != null) localCaravan.MeIn3d.InstantlyUpdatePosition();

			foreach (int zoneID in dataBeingEdited.linkedZones) {
				Zone linkedZone = GameController.GetZoneByID(zoneID);
				World.GetLinkLineBetween(dataBeingEdited, linkedZone).UpdatePositions();
			}
		});
	}

	public void EditLocalMercCaravan() {
		MercCaravan localCaravan = GameController.GetMercCaravanInZone(dataBeingEdited.ID);

		if(localCaravan != null) {
			GameInterface.instance.EditMercCaravan(localCaravan, false);
		}else {
			ModalPanel.Instance().YesNoBox("No Caravan Found Here", "Create a new Mercenary Caravan in this zone?", () => {
				localCaravan = World.CreateNewMercCaravanAtZone(dataBeingEdited);
				GameInterface.instance.EditMercCaravan(localCaravan, true);
			}, null);
		}
	}
}
