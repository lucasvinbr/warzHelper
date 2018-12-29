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
		bPanel.attackerSide.SetPostBattleArmyData(attackerBox.sliderGroup.theSlider.value);
		bPanel.defenderSide.SetPostBattleArmyData(defenderBox.sliderGroup.theSlider.value);
	}

}