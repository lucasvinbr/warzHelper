using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> battleZones = new List<Zone>();

	public BattlePanel battlePanel;

	public Battle battleData;

	public override void OnPhaseStart() {
		base.OnPhaseStart();
		battleZones.Clear();

		if (battleData == null) battleData = new Battle();

		infoTxt.text = "Resolution of any battles started in the Command Phase";
		GameInfo curGameData = GameController.CurGameData;

		if (curGameData.unifyBattlePhase) {
			if (GameModeHandler.instance.curPlayingFaction.ID != curGameData.factions[curGameData.factions.Count - 1].ID) {
				//this phase is skipped if we're in "unified" mode and it's not the last faction's turn
				OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
				return;
			}
			else {
				//this is the last faction's turn in the order: it's the "big action phase"!
				//all scheduled orders are run at the same time and then we check for battles
				curGameData.unifiedOrdersRegistry.RunAllOrders();
			}
		}

		//in unified mode, this phase should run only once, during the last faction's turn in turn order
		List<Commander> potentialFighterCmders = curGameData.unifyBattlePhase ?
			curGameData.deployedCommanders : GameModeHandler.instance.curPlayingFaction.OwnedCommanders;

		Zone zoneCmderIsIn;
		Faction ownerFac;
		foreach(Commander cmder in potentialFighterCmders) {
			if (cmder.troopsContained.TotalAutocalcPower <= 0) continue;

			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);

			if(!curGameData.zonesWithResolvedBattlesThisTurn.Contains(zoneCmderIsIn.ID) &&
				!battleZones.Contains(zoneCmderIsIn) &&
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


	/// <summary>
	/// moves the cam to the target zone and opens the battle resolution panel
	/// or does autocalc resolution... or both
	/// </summary>
	/// <param name="targetZone"></param>
	public IEnumerator ResolveBattle(Zone targetZone) {
		Faction	defenderFaction = GameController.GetFactionByID(targetZone.ownerFaction);
		List<Commander> atkerCmders = new List<Commander>();

		GameInfo curData = GameController.CurGameData;

		foreach(Commander cmder in GameController.GetCommandersInZone(targetZone)) {
			if(cmder.ownerFaction != defenderFaction.ID &&
				defenderFaction.GetStandingWith(cmder.ownerFaction) != GameFactionRelations.FactionStanding.ally) {
				atkerCmders.Add(cmder);
			}
		}

		battleData.FillInfo(atkerCmders, defenderFaction, targetZone);

		//if the "always autocalc AI battles" option is active,
		//we must check if the player's troops aren't participating
		//before forcing autocalc resolution
		if (curData.alwaysAutocalcAiBattles) {

			foreach (TroopContainer tc in battleData.attackerSideInfo.ourContainers) {
				if (GameController.GetFactionByID(tc.ownerFaction).isPlayer) {
					yield return FocusOnBattle(targetZone);
					battlePanel.OpenWithFilledInfo(battleData);
					yield break;
				}
			}

			foreach (TroopContainer tc in battleData.defenderSideInfo.ourContainers) {
				if (GameController.GetFactionByID(tc.ownerFaction).isPlayer) {
					yield return FocusOnBattle(targetZone);
					battlePanel.OpenWithFilledInfo(battleData);
					yield break;
				}
			}


			if (curData.showBattleResolutionPanelForAutocalcAiBattles) {
				yield return FocusOnBattle(targetZone);
				battlePanel.OpenWithFilledInfo(battleData);
			}else {
				//a little waiting to avoid freezing and allowing to pause between battles
				yield return WaitWhileNoOverlays(0.01f);
				battleData.AutocalcResolution();
				OnBattleResolved();
			}
			yield break;
		}else {
			yield return FocusOnBattle(targetZone);
			battlePanel.OpenWithFilledInfo(battleData);
		}

		
	}

	public void OnBattleResolved() {
		GameController.CurGameData.zonesWithResolvedBattlesThisTurn.Add(battleZones[0].ID);
		battleZones.RemoveAt(0);
		if (battleZones.Count == 0) {
			infoTxt.text = "No more battles to resolve!";
			OnPhaseEnding();
		}
		else {
			StartCoroutine(GoToNextBattle());
		}
	}

	public IEnumerator FocusOnBattle(Zone warZone) {
		CameraPanner.instance.TweenToSpot(warZone.MyZoneSpot.transform.position);
		yield return WaitWhileNoOverlays(0.35f);
	}

	
	public IEnumerator GoToNextBattle() {
		yield return ResolveBattle(battleZones[0]);
	}

	public override void InterruptPhase() {
		base.InterruptPhase();
		GameController.CurGameData.zonesWithResolvedBattlesThisTurn.Clear();
		battlePanel.FullInterrupt();
	}

	public override void OnPhaseEnded() {
		base.OnPhaseEnded();
		GameController.CurGameData.zonesWithResolvedBattlesThisTurn.Clear();
		infoTxt.text = string.Empty;
	}


}
