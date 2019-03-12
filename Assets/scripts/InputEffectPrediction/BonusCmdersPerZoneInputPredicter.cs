using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class BonusCmdersPerZoneInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			if (GameController.GuardGameDataExist()) {
				predictedEffectText.text = string.Concat((float.Parse(inputText, CultureInfo.InvariantCulture) * 
					GameController.instance.curData.rules.baseBonusCommandersPerZone).ToString(CultureInfo.InvariantCulture), 
					" bonus cmders / zone");
			}
			else {
				predictedEffectText.text = "?";
			}
		}
	}
}
