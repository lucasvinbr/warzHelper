using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePanelListEntry : ListPanelEntry<TemplateInfo> {

    public Text gameNameTxt;

    public Text gameExtraInfoTxt;

    public Image backgroundImg;

    public override void SetContent(TemplateInfo theContent)
    {
        base.SetContent(theContent);
        gameNameTxt.text = theContent.gameName;
        //if this is a template, show number of factions or something like that
        //if it's a game, show elapsed turns
        if(theContent.GetType() == typeof(GameInfo))
        {
            gameExtraInfoTxt.text = "elapsed turns: " + (theContent as GameInfo).elapsedTurns.ToString();
        }
        else
        {
            gameExtraInfoTxt.text = theContent.factions.Count.ToString() + " factions";
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
