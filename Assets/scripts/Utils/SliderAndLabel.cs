using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SliderAndLabel : MonoBehaviour {

    public Slider theSlider;
    public Text theLabel;
	public bool addRefreshLabelListener = true;

	// Use this for initialization
	void Start () {
		if (addRefreshLabelListener) {
			theSlider.onValueChanged.AddListener((float newValue) =>
			{
				RefreshLabelText();
			});
		}
        
	}
	

    public void SetValue(float newValue)
    {
        theSlider.value = newValue;
    }

	public void RefreshLabelText() {
		theLabel.text = theSlider.value.ToString("0.00");
	}
}
