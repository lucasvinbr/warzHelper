using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class BattleResolutionManualPanel : GenericOverlayPanel {

	public BattleRemainingFactionTroopsPanel attackerPanel, defenderPanel;

	public BattlePanel bPanel;

	public void SetupAndOpen(TroopList attackerForces, TroopList defenderForces) {
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
		bPanel.battlePhase.battleData.attackerSideInfo.SetPostBattleArmyData_RemainingArmy(attackerPanel.BakeIntoArmy());
		bPanel.battlePhase.battleData.defenderSideInfo.SetPostBattleArmyData_RemainingArmy(defenderPanel.BakeIntoArmy());
		if (!bPanel.battlePhase.battleData.BattleEndCheck()) {
			//we only need to update side info displays if the battle hasn't ended yet, 
			//because the delegate would take care of it otherwise
			bPanel.UpdateArmyDisplays();
		}
	}

}