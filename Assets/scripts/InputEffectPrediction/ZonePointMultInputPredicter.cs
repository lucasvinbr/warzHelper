using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ZonePointMultInputPredicter : InputEffectPredicter {

	public string curOwnerFaction;

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			float multValue = float.Parse(inputText, CultureInfo.InvariantCulture);

			int resultingPoints = GameController.GetResultingPointsForZone(curOwnerFaction, multValue);
			if (GameController.GuardGameDataExist()) {
				predictedEffectText.text = "Result: " + resultingPoints.ToString() + " points";
			}
			else {
				predictedEffectText.text = "?";
			}
		}
		
	}
}
