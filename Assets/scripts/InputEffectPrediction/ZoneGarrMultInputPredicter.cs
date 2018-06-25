using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class ZoneGarrMultInputPredicter : InputEffectPredicter {

	public string curOwnerFaction;

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			float multValue = float.Parse(inputText, CultureInfo.InvariantCulture);

			int resultingPoints = GameController.GetResultingMaxGarrForZone(curOwnerFaction, multValue);
			if (GameController.GuardGameDataExist()) {
				predictedEffectText.text = resultingPoints.ToString() + " units";
			}
			else {
				predictedEffectText.text = "?";
			}
		}
		
	}
}
