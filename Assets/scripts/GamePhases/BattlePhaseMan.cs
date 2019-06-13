using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> battleZones = new List<Zone>();

	public BattlePanel battlePanel;

	public override void OnPhaseStart() {
		base.OnPhaseStart();
		battleZones.Clear();
		//find battles, register them and open a resolution menu for each one
		infoTxt.text = "Resolution of any battles started in the Command Phase";
		GameInfo curGameData = GameController.CurGameData;
		List<Commander> potentialFighterCmders = curGameData.unifyBattlePhase ?
			curGameData.deployedCommanders : GameModeHandler.instance.curPlayingFaction.OwnedCommanders;

		Zone zoneCmderIsIn = null;
		Faction ownerFac = null;
		foreach(Commander cmder in potentialFighterCmders) {
			if (cmder.TotalAutocalcPower <= 0) continue;

			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if(!battleZones.Contains(zoneCmderIsIn) &&
				zoneCmderIsIn.ownerFaction != cmder.ownerFaction) {
				ownerFac = GameController.GetFactionByID(zoneCmderIsIn.ownerFaction);
				if (ownerFac != null && 
					ownerFac.GetStandingWith(cmder.ownerFaction) != GameFactionRelations.FactionStanding.ally &&
					GameController.GetCombinedTroopsInZoneFromFactionAndAllies
					(zoneCmderIsIn, ownerFac).Count > 0) {
					battleZones.Add(zoneCmderIsIn);
				}
			}
		}
		
		if(battleZones.Count == 0) {
			infoTxt.text = "No battles to resolve!";
			OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
		}else {
			StartCoroutine(GoToNextBattle());
		}
		
	}

	public void OpenBattleResolutionPanelForZone(Zone targetZone) {
		Faction attackerFaction = GameModeHandler.instance.curPlayingFaction,
			defenderFaction = GameController.GetFactionByID(targetZone.ownerFaction);
		if((GameController.CurGameData).unifyBattlePhase) {
			foreach(Commander cmder in GameController.GetCommandersInZone(targetZone)) {
				if(cmder.ownerFaction != defenderFaction.ID &&
					defenderFaction.GetStandingWith(cmder.ownerFaction) != GameFactionRelations.FactionStanding.ally) {
					attackerFaction = GameController.GetFactionByID(cmder.ownerFaction);
					break;
				}
			}
		}

		battlePanel.OpenWithFilledInfos(attackerFaction,
			defenderFaction, targetZone);
	}

	public void OnBattleResolved(Zone battleZone) {
		battleZones.RemoveAt(0);
		if (battleZones.Count == 0) {
			infoTxt.text = "No more battles to resolve!";
			OnPhaseEnding();
		}
		else {
			StartCoroutine(GoToNextBattle());
		}
	}

	/// <summary>
	/// jumps to the battle zone and, after a little while, opens the resolution panel for it
	/// </summary>
	/// <returns></returns>
	public IEnumerator GoToNextBattle() {
		CameraPanner.instance.TweenToSpot(battleZones[0].MyZoneSpot.transform.position);
		yield return WaitWhileNoOverlays(0.35f);
		OpenBattleResolutionPanelForZone(battleZones[0]);
	}

	public override void InterruptPhase() {
		base.InterruptPhase();
		battlePanel.FullInterrupt();
	}


}
