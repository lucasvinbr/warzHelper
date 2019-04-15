using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> battleZones = new List<Zone>();

	public BattlePanel battlePanel;

	public override void OnPhaseStart() {
		//find battles, register them and open a resolution menu for each one
		infoTxt.text = "Resolution of any battles started in the Command Phase";
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;

		Zone zoneCmderIsIn = null;
		Faction ownerFac = null;
		foreach(Commander cmder in factionCmders) {
			if (cmder.TotalAutocalcPower <= 0) continue;

			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if(!battleZones.Contains(zoneCmderIsIn) &&
				zoneCmderIsIn.ownerFaction != playerFac.ID) {
				ownerFac = GameController.GetFactionByID(zoneCmderIsIn.ownerFaction);
				if (ownerFac != null && 
					ownerFac.GetStandingWith(playerFac) != GameFactionRelations.FactionStanding.ally &&
					GameController.GetCombinedTroopsInZoneFromFactionAndAllies
					(zoneCmderIsIn, ownerFac).Count > 0) {
					battleZones.Add(zoneCmderIsIn);
				}
			}
		}
		
		if(battleZones.Count == 0) {
			infoTxt.text = "No battles to resolve!";
			OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
		}else {
			StartCoroutine(GoToNextBattle());
		}
		
	}

	public void OpenBattleResolutionPanelForZone(Zone targetZone) {
		battlePanel.OpenWithFilledInfos(GameModeHandler.instance.curPlayingFaction,
			GameController.GetFactionByID(targetZone.ownerFaction), targetZone,
			ShouldBattleBeAutocalcd(targetZone));
	}

	/// <summary>
	/// returns true if all involved factions are AI-controlled 
	/// and the "always autocalc ai battles" option is active
	/// </summary>
	/// <returns></returns>
	public bool ShouldBattleBeAutocalcd(Zone warZone) {
		if((GameController.instance.curData as GameInfo).alwaysAutocalcAiBattles) {
			if (GameModeHandler.instance.curPlayingFaction.isPlayer) {
				return false;
			}else {
				Faction checkedFaction = GameController.GetFactionByID(warZone.ownerFaction);
				if (checkedFaction.isPlayer) return false;
				else {
					foreach(Commander cmd in GameController.GetCommandersInZone(warZone)) {
						checkedFaction = GameController.GetFactionByID(cmd.ownerFaction);
						if (checkedFaction.isPlayer) return false;
					}

					return true;
				}
			}
		}

		return false;
	}

	public void OnBattleResolved(Zone battleZone) {
		battleZones.RemoveAt(0);
		if (battleZones.Count == 0) {
			infoTxt.text = "No more battles to resolve!";
			OnPhaseEnd();
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
