using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class EditRulesBoardPanel : EditDataPanel<Rules> {

    public InputField maxGarrUnitsInput, maxCmderUnitsInput, bonusCmdersPerZoneInput, 
		pointAwardZonesInput, pointAwardCmdersInput, pointAwardBattlesInput, winnerBattleDmgInput,
		maxRandomAutocalcPowerInput, maxTroopsInvolvedInOneTurnBattleInput, maxCmdersPerFactionInput, boardWidthInput, boardHeightInput;

	public Image boardTextureImg;


	public override void Open(Rules editedRules, bool isNewEntry) {
		base.Open(editedRules, isNewEntry);
		maxGarrUnitsInput.text = dataBeingEdited.baseMaxUnitsInOneGarrison.ToString();
		maxCmderUnitsInput.text = dataBeingEdited.baseMaxUnitsUnderOneCommander.ToString();
		maxCmdersPerFactionInput.text = dataBeingEdited.baseMaxCommandersPerFaction.ToString();
		pointAwardZonesInput.text = dataBeingEdited.baseZonePointAwardOnTurnStart.ToString();
		pointAwardCmdersInput.text = dataBeingEdited.baseCommanderPointAwardOnTurnStart.ToString();
		maxTroopsInvolvedInOneTurnBattleInput.text = dataBeingEdited.maxTroopsInvolvedInBattlePerTurn.ToString();
		maxRandomAutocalcPowerInput.text = dataBeingEdited.autoResolveBattleDieSides.ToString(CultureInfo.InvariantCulture);
		pointAwardBattlesInput.text = dataBeingEdited.battleWinnerPointAwardFactor.ToString(CultureInfo.InvariantCulture);
		bonusCmdersPerZoneInput.text = dataBeingEdited.baseBonusCommandersPerZone.ToString(CultureInfo.InvariantCulture);
		winnerBattleDmgInput.text = dataBeingEdited.autoResolveWinnerDamageMultiplier.ToString(CultureInfo.InvariantCulture);
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
		dataBeingEdited.autoResolveBattleDieSides = float.Parse(maxRandomAutocalcPowerInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.battleWinnerPointAwardFactor = float.Parse(pointAwardBattlesInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.baseBonusCommandersPerZone = float.Parse(bonusCmdersPerZoneInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.autoResolveWinnerDamageMultiplier = float.Parse(winnerBattleDmgInput.text, CultureInfo.InvariantCulture);
		dataBeingEdited.baseCommanderPointAwardOnTurnStart = int.Parse(pointAwardCmdersInput.text);
		dataBeingEdited.baseZonePointAwardOnTurnStart = int.Parse(pointAwardZonesInput.text);
		dataBeingEdited.baseMaxCommandersPerFaction = int.Parse(maxCmdersPerFactionInput.text);
		dataBeingEdited.baseMaxUnitsInOneGarrison = int.Parse(maxGarrUnitsInput.text);
		dataBeingEdited.baseMaxUnitsUnderOneCommander = int.Parse(maxCmderUnitsInput.text);
		dataBeingEdited.maxTroopsInvolvedInBattlePerTurn = int.Parse(maxTroopsInvolvedInOneTurnBattleInput.text);
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

}
