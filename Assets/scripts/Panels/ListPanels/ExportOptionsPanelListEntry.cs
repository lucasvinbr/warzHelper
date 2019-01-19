using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExportOptionsPanelListEntry : ListPanelEntry<UnityAction> {

	public Text btnTxt;

	public Button theBtn;

	public override void SetContent(UnityAction theContent) {
		base.SetContent(theContent);
		theBtn.onClick.AddListener(theContent);
	}
}
