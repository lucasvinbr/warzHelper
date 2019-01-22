using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class BattleResolutionManualPanel : GenericOverlayPanel {

	public BattleRemainingFactionTroopsPanel attackerPanel, defenderPanel;

	public BattlePanel bPanel;

	public void SetupAndOpen(List<TroopNumberPair> attackerForces, List<TroopNumberPair> defenderForces) {
		attackerPanel.representedTroops = attackerForces;
		defenderPanel.representedTroops = defenderForces;

		gameObject.SetActive(true);
	}

	public override void OnEnable() {
		base.OnEnable();
		attackerPanel.boxHeaderTxt.color = bPanel.attackerSide.factionNameTxt.color;

		defenderPanel.boxHeaderTxt.color = bPanel.defenderSide.factionNameTxt.color;
	}

	public void OnConfirm() {
		bPanel.attackerSide.SetPostBattleArmyData_RemainingArmy(attackerPanel.BakeIntoArmy());
		bPanel.defenderSide.SetPostBattleArmyData_RemainingArmy(defenderPanel.BakeIntoArmy());
	}

}