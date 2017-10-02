using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// a scene follower with a text
/// </summary>
public class GUIFollowerText : GUISceneFollower {

    public Text myText;

    public void SetText(string text)
    {
        myText.text = text;
    }
}
