using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePanelListEntry : ListPanelEntry<TemplateInfo> {

    public Text gameNameTxt;

    public Text gameExtraInfoTxt;

    public override void SetContent(TemplateInfo theContent)
    {
        base.SetContent(theContent);
        gameNameTxt.text = theContent.gameName;
        //TODO extra info: if this is a template, show number of factions or something like that
        //if it's a game, show elapsed turns
    }

    public void OnClicked()
    {
        //TODO check what's going on and then decide what this button should do
    }
}
