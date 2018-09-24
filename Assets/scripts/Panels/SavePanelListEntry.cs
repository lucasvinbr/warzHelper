using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePanelListEntry : ListPanelEntry<TemplateInfo> {

    public Text gameNameTxt;

    public Text gameExtraInfoTxt;

    public Image backgroundImg;

    void Start()
    {
        backgroundImg.color = GameInterface.instance.deselectedUIElementColor;
    }

    public override void SetContent(TemplateInfo theContent)
    {
        base.SetContent(theContent);
        gameNameTxt.text = theContent.gameName;
        //if this is a template, show number of factions or something like that
        //if it's a game, show elapsed turns
        if(theContent.GetType() == typeof(GameInfo))
        {
            gameExtraInfoTxt.text = "Elapsed Turns: " + (theContent as GameInfo).elapsedTurns.ToString();
        }
        else
        {
            gameExtraInfoTxt.text = "Num. Factions: " + theContent.factions.Count.ToString();
        }
    }

    public void OnClicked()
    {
        //tell the save panel we've been selected
        GameInterface.instance.saveListPanel.SelectEntry(this);
    }

    public void ToggleBackgroundHighlightColor(bool lit)
    {
        if (lit)
        {
            backgroundImg.color = GameInterface.instance.selectedUIElementColor;
        }
        else
        {
            backgroundImg.color = GameInterface.instance.deselectedUIElementColor;
        }
    }
}
