using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SliderAndLabel : MonoBehaviour {

    public Slider theSlider;
    public Text theLabel;

	// Use this for initialization
	void Start () {
        theSlider.onValueChanged.AddListener((float newValue) =>
        {
            theLabel.text = theSlider.value.ToString();
        });
	}
	

    public void SetValue(float newValue)
    {
        theSlider.value = newValue;
    }
}
