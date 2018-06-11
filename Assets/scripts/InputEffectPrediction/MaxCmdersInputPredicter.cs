using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class MaxCmdersInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			if (GameController.instance.curData != null) {
				predictedEffectText.text = Mathf.Max(int.Parse(inputText) + 
					GameController.instance.curData.rules.baseMaxCommandersPerFaction, 0).ToString(CultureInfo.InvariantCulture);
			}
			else {
				predictedEffectText.text = "?";
			}
		}
	}
}
