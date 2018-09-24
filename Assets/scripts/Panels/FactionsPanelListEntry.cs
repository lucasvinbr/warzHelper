using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsPanelListEntry : ListPanelEntry<Faction> {

    public Text nameTxt, turnPriorityTxt, zonesOwnedTxt;

    public Image backgroundImg;

	public override void SetContent(Faction targetFaction)
    {
        myContent = targetFaction;
        nameTxt.text = targetFaction.name;
        turnPriorityTxt.text = targetFaction.turnPriority.ToString();
        backgroundImg.color = targetFaction.color;
		zonesOwnedTxt.text = "TODO"; //TODO get number of zones owned by this faction
    }

    public void OpenEditFactionPanel()
    {
		GameInterface.instance.EditFaction(myContent, false);
    }

}
