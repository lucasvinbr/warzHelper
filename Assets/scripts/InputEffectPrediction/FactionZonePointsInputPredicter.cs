﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Globalization;

public class FactionZonePointsInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			if (GameController.GuardGameDataExist()) {
				predictedEffectText.text = Mathf.RoundToInt
					(float.Parse(inputText, CultureInfo.InvariantCulture) * 
					GameController.instance.curData.rules.baseZonePointAwardOnTurnStart).ToString(CultureInfo.InvariantCulture);
			}
			else {
				predictedEffectText.text = "?";
			}
		}
	}
}
