using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;

/// <summary>
/// script used by faction entries of the "allies" and "enemies" lists
/// of the faction selected as target for a diplomatic message
/// </summary>
public class FactionEntryWithNameAndTooltip : MonoBehaviour {

	public TMP_Text facNameTxt;
	public TooltipTrigger tooltip;

	public void SetNameAndColorToFac(Faction fac) {
		facNameTxt.text = fac.name;
		facNameTxt.color = fac.color;
	}
}
