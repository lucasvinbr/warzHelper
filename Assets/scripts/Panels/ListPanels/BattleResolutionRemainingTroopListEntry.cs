using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// wow, big name
/// </summary>
public class BattleResolutionRemainingTroopListEntry : ListPanelEntry<TroopNumberPair> {

	public Text nameTxt;

	public NumericInputFieldBtns remainingFieldBtns;

	public override void SetContent(TroopNumberPair targetTroop) {
		myContent = targetTroop;
		nameTxt.text = GameController.GetTroopTypeByID(myContent.troopTypeID).name;
		remainingFieldBtns.maxValue = myContent.troopAmount;
		remainingFieldBtns.minValue = 0;
		remainingFieldBtns.targetField.text = myContent.troopAmount.ToString();
	}

	public TroopNumberPair BakeIntoNewPair() {
		return new TroopNumberPair(myContent.troopTypeID, int.Parse(remainingFieldBtns.targetField.text));
	}

}
