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
        ownerTxt.text = "owner: " + myContent.ownerFaction;
        garrisonTxt.text = "garrison: " + myContent.troopsGarrisoned.ToString();
    }
}
