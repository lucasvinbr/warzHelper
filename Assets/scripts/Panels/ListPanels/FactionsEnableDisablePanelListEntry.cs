using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsEnableDisablePanelListEntry : ListPanelEntry<Faction> {

    public Text nameTxt;

	public Toggle isEnabledToggle;

    public Image backgroundImg;

	public override void SetContent(Faction targetFaction)
    {
        myContent = targetFaction;
        nameTxt.text = targetFaction.name;
        backgroundImg.color = targetFaction.color;
		isEnabledToggle.isOn = GameController.CurGameData.factions.Contains(targetFaction);
    }

}
