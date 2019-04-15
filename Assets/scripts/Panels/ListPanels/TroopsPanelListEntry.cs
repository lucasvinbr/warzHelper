using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class TroopsPanelListEntry : ListPanelEntry<TroopType> {

    public Text nameTxt, powerTxt, costTxt;

    public override void SetContent(TroopType theContent)
    {
        base.SetContent(theContent);
        nameTxt.text = myContent.name;
		powerTxt.text = myContent.autoResolvePower.ToString(CultureInfo.InvariantCulture);
		costTxt.text = myContent.pointCost.ToString();
    }

	public void EntryClicked() {
		GameInterface.instance.EditTroopType(myContent, false);
		GameInterface.instance.editTroopPanel.onDoneEditing += GameInterface.instance.troopsPanel.RefillList;
	}
}
