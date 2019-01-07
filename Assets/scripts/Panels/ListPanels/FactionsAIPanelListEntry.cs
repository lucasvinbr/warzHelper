using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsAIPanelListEntry : ListPanelEntry<Faction> {

    public Text nameTxt;

	public Toggle isAIToggle;

    public Image backgroundImg;

	public override void SetContent(Faction targetFaction)
    {
        myContent = targetFaction;
        nameTxt.text = targetFaction.name;
        backgroundImg.color = targetFaction.color;
		isAIToggle.isOn = !targetFaction.isPlayer;
    }

}
