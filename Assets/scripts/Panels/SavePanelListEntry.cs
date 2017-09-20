using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SavePanelListEntry : ListPanelEntry<GameInfo> {

    public Text gameNameTxt;

    public override void SetContent(GameInfo theContent)
    {
        base.SetContent(theContent);
        gameNameTxt.text = theContent.gameName;
    }
}
