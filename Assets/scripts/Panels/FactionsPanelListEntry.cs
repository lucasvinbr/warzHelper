using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsPanelListEntry : MonoBehaviour {

    public Text nameTxt, turnPriorityTxt;

    public Image backgroundImg;

    public Faction myFaction;

	public void SetContent(Faction targetFaction)
    {
        myFaction = targetFaction;
        nameTxt.text = targetFaction.name;
        turnPriorityTxt.text = "Turn Priority: " + targetFaction.turnPriority.ToString();
        backgroundImg.color = targetFaction.color;
    }

    public void OpenEditFactionPanel()
    {
        GameInterface.instance.EditFaction(myFaction);
    }
}
