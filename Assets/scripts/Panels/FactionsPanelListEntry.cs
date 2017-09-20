using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsPanelListEntry : ListPanelEntry<Faction> {

    public Text nameTxt, turnPriorityTxt;

    public Image backgroundImg;

	public override void SetContent(Faction targetFaction)
    {
        myContent = targetFaction;
        nameTxt.text = targetFaction.name;
        turnPriorityTxt.text = "Turn Priority: " + targetFaction.turnPriority.ToString();
        backgroundImg.color = targetFaction.color;
    }

    public void OpenEditFactionPanel()
    {
        GameInterface.instance.EditFaction(myContent);
    }
}
