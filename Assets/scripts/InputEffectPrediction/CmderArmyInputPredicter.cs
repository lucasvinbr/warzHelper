using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class CmderArmyInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			if (GameController.instance.curData != null) {
				predictedEffectText.text = Mathf.RoundToInt
					(float.Parse(inputText, CultureInfo.InvariantCulture) * 
					GameController.instance.curData.rules.baseMaxUnitsUnderOneCommander).ToString(CultureInfo.InvariantCulture);
			}
			else {
				predictedEffectText.text = "?";
			}
		}
	}
}
