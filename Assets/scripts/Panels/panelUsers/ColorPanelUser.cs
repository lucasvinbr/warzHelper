using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPanelUser : PanelUser<ColorInputPanel> {

	public Image colorImg;

	public DirtableOverlayPanel parentDirtablePanel;

	protected override void Reset() {
		base.Reset();
		colorImg = GetComponent<Image>();
		parentDirtablePanel = GetComponentInParent<DirtableOverlayPanel>();
	}

	public void OpenEditColorMenu() {
		thePanel.SetPanelInfo("Set Color", colorImg.color, "Confirm", () => {
			colorImg.color = thePanel.colorImg.color;
			if(parentDirtablePanel) parentDirtablePanel.isDirty = true;
		});
		thePanel.Open();
	}
}
