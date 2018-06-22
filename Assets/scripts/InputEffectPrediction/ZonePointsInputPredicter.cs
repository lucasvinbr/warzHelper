using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class ZoneMaxGarrInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			if (GameController.GuardGameDataExist()) {
				predictedEffectText.text = Mathf.RoundToInt
					(float.Parse(inputText, CultureInfo.InvariantCulture) * 
					GameController.instance.curData.rules.baseMaxUnitsInOneGarrison).ToString(CultureInfo.InvariantCulture);
			}
			else {
				predictedEffectText.text = "?";
			}
		}
	}
}
