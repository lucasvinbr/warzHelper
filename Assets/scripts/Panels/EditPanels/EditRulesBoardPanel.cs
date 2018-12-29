using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditRulesBoardPanel : EditDataPanel<Rules> {

    public InputField maxGarrUnitsInput, maxCmderUnitsInput, pointAwardZonesInput, pointAwardCmdersInput,
		maxRandomAutocalcPowerInput, maxCmdersPerFactionInput, boardWidthInput, boardHeightInput;

	public Image boardTextureImg;


	public override void Open(Rules editedRules, bool isNewEntry) {
		base.Open(editedRules, isNewEntry);
		maxGarrUnitsInput.text = dataBeingEdited.baseMaxUnitsInOneGarrison.ToString();
		maxCmderUnitsInput.text = dataBeingEdited.baseMaxUnitsUnderOneCommander.ToString();
		maxCmdersPerFactionInput.text = dataBeingEdited.baseMaxCommandersPerFaction.ToString();
		pointAwardZonesInput.text = dataBeingEdited.baseZonePointAwardOnTurnStart.ToString();
		pointAwardCmdersInput.text = dataBeingEdited.baseCommanderPointAwardOnTurnStart.ToString();
		maxRandomAutocalcPowerInput.text = dataBeingEdited.autoResolveBattleDieSides.ToString();
		boardWidthInput.text = dataBeingEdited.boardDimensions.x.ToString(CultureInfo.InvariantCulture);
		boardHeightInput.text = dataBeingEdited.boardDimensions.y.ToString(CultureInfo.InvariantCulture);
		//zoneIconImg.sprite = TODO get icon by path to file
		isDirty = false;
	}

	public override bool DataIsValid() {
		//not much to test here for now
		return true;
	}

	public void CloseAndSaveChanges() {
		if (!DataIsValid()) return;
		dataBeingEdited.boardDimensions = new Vector2(float.Parse(boardWidthInput.text, CultureInfo.InvariantCulture), 
			float.Parse(boardHeightInput.text, CultureInfo.InvariantCulture));
		dataBeingEdited.autoResolveBattleDieSides = int.Parse(maxRandomAutocalcPowerInput.text);
		dataBeingEdited.baseCommanderPointAwardOnTurnStart = int.Parse(pointAwardCmdersInput.text);
		dataBeingEdited.baseZonePointAwardOnTurnStart = int.Parse(pointAwardZonesInput.text);
		dataBeingEdited.baseMaxCommandersPerFaction = int.Parse(maxCmdersPerFactionInput.text);
		dataBeingEdited.baseMaxUnitsInOneGarrison = int.Parse(maxGarrUnitsInput.text);
		dataBeingEdited.baseMaxUnitsUnderOneCommander = int.Parse(maxCmderUnitsInput.text);
		World.SetupBoardDetails();
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
			if (isDirty) {
				ModalPanel.Instance().YesNoCancelBox("Save Changes?", "Pressing 'No' will discard changes and close the window.", CloseAndSaveChanges, CloseWithoutSaving, null);
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
			ModalPanel.Instance().YesNoBox("Confirm Zone Deletion", "You are about to delete this zone. This cannot be undone unless this game is reloaded without saving. Are you sure?", OnConfirmDelete, null);
		}
	}

	public override void OnConfirmDelete() {
		//no deleting should happen here, but...
		gameObject.SetActive(false);
		dataBeingEdited = null;
		OnWindowIsClosing();
	}


}
