using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ColorInputPanel : InputPanel {

    public SliderAndLabel r, g, b;

    public Image colorImg;

    public Color theColor = Color.white;

    public void SetPanelInfo(string infoLabelContent, Color initialColor, string actionButtonText, UnityEvent onActionButtonPress)
    {
        SetPanelInfo(infoLabelContent, actionButtonText, onActionButtonPress);
        SetTheColor(initialColor);
    }

    public void SetTheColor(Color desiredColor, bool refreshRGBSliders = true)
    {
        theColor = desiredColor;
        colorImg.color = theColor;

        if (refreshRGBSliders)
        {
            RefreshRGB();
        }
        
    }

    public void SetR(float newR)
    {
        theColor.r = newR;
        colorImg.color = theColor;
    }

    public void SetG(float newG)
    {
        theColor.g = newG;
        colorImg.color = theColor;
    }

    public void SetB(float newB)
    {
        theColor.b = newB;
        colorImg.color = theColor;
    }

    void RefreshRGB()
    {
        r.SetValue(theColor.r);
        g.SetValue(theColor.g);
        b.SetValue(theColor.b);
    }
}
