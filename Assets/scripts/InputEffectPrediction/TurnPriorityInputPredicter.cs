using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPriorityInputPredicter : InputEffectPredicter {

	public override void WriteProjection(string inputText) {
		if (predictedEffectText) {
			int tpValue = int.Parse(inputText);

			if (GameController.GuardGameDataExist()) {
				List<Faction> createdFacs = GameController.instance.curData.factions;
				
				int ourMinPos = 1, ourMaxPosDelta = 0;
				for(int i = 0; i < createdFacs.Count; i++) {
					if (createdFacs[i] == GameInterface.instance.editFactionPanel.dataBeingEdited) continue;
					if (createdFacs[i].turnPriority < tpValue) {
						ourMinPos++;
					}else if(createdFacs[i].turnPriority == tpValue) {
						ourMaxPosDelta++;
					}
				}

				if(ourMaxPosDelta > 0) {
					predictedEffectText.text = "between " + ourMinPos.ToString() + " and " + (ourMinPos + ourMaxPosDelta).ToString() + " in turn order";
				}
				else {
					predictedEffectText.text = ourMinPos.ToString() + " in turn order";
				}
			}
			else {
				predictedEffectText.text = "?";
			}
		}
		
	}
}
