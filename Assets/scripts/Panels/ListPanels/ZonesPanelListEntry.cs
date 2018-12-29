using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZonesPanelListEntry : ListPanelEntry<Zone> {

    public Text nameTxt, ownerTxt, garrisonTxt;

	public Image backgroundImg;

    public override void SetContent(Zone theContent)
    {
        base.SetContent(theContent);
        nameTxt.text = myContent.name;
        ownerTxt.text = GameController.GetFactionNameByID(myContent.ownerFaction);
        garrisonTxt.text = myContent.TotalTroopsContained.ToString();
    }

	public void EntryClicked() {
		GameInterface.instance.EditZone(myContent, false);
		GameInterface.instance.editZonePanel.onDoneEditing += GameInterface.instance.zonesPanel.RefillList;
	}
}
