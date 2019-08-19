using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class BattleResolutionPercentagePanel : GenericOverlayPanel {

	public BattleResolutionPercentageBox attackerBox, defenderBox;

	public BattlePanel bPanel;

	public override void OnEnable() {
		base.OnEnable();
		attackerBox.factionNameTxt.text = "Attackers: " + bPanel.attackerSide.factionNameTxt.text;
		attackerBox.factionNameTxt.color = bPanel.attackerSide.factionNameTxt.color;
		attackerBox.sliderGroup.theSlider.value = 1.0f;

		defenderBox.factionNameTxt.text = "Defenders: " + bPanel.defenderSide.factionNameTxt.text;
		defenderBox.factionNameTxt.color = bPanel.defenderSide.factionNameTxt.color;
		defenderBox.sliderGroup.theSlider.value = 1.0f;

		attackerBox.sliderGroup.RefreshLabelText();
		defenderBox.sliderGroup.RefreshLabelText();
	}

	public void OnConfirmPercentages() {
		bPanel.battlePhase.battleData.attackerSideInfo.SetPostBattleArmyData_RemainingPercent(attackerBox.sliderGroup.theSlider.value);
		bPanel.battlePhase.battleData.defenderSideInfo.SetPostBattleArmyData_RemainingPercent(defenderBox.sliderGroup.theSlider.value);

		if (!bPanel.battlePhase.battleData.BattleEndCheck()) {
			//we only need to update side info displays if the battle hasn't ended yet, 
			//because the delegate would take care of it otherwise
			bPanel.UpdateArmyDisplays();
		}
	}

}