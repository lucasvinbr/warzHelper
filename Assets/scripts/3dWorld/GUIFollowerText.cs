using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// a scene follower with a text
/// </summary>
public class GUIFollowerText : GUISceneFollower {

    public TMP_Text myText;

    public void SetText(string text)
    {
        myText.text = text;
    }
}
