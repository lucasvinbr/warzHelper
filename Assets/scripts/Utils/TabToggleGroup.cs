using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabToggleGroup : MonoBehaviour {

	public Toggle[] theToggleTabHeaders;
	public ToggleGroup theGroup;
	public Color pickedTabHeaderColor = Color.white, unselectedTabHeaderColor = Color.black;

	void Start() {
		for(int i = 0; i < theToggleTabHeaders.Length; i++) {
			Toggle theT = theToggleTabHeaders[i];
			theGroup.RegisterToggle(theT);
			theT.onValueChanged.AddListener((bool isNowOn) => {
				//tab headers usually have image components attached to them in order to have backgrounds
				Image tabBG = theT.GetComponent<Image>();
				if (tabBG) {
					tabBG.color = isNowOn ? pickedTabHeaderColor : unselectedTabHeaderColor;
				}
				if (theT.graphic) {
					CanvasGroup tabContentsGroup = theT.graphic.GetComponent<CanvasGroup>();
					if (tabContentsGroup) {
						tabContentsGroup.interactable = isNowOn;
						tabContentsGroup.blocksRaycasts = isNowOn;
					}
				}
			});
			theT.onValueChanged.Invoke(theT.isOn); //run it, just to make sure only one tab's contents are accessible
		}
	}
}
