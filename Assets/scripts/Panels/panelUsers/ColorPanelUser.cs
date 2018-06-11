using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPanelUser : PanelUser<ColorInputPanel> {

	public Image colorImg;

	protected override void Reset() {
		base.Reset();
		colorImg = GetComponent<Image>();
	}

	public void OpenEditColorMenu() {
		thePanel.SetPanelInfo("Set Faction Color", colorImg.color, "Confirm", () => {
			colorImg.color = thePanel.colorImg.color;
		});
		thePanel.Open();
	}
}
