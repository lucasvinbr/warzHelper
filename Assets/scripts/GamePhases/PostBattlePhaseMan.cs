using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostBattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> conflictZones = new List<Zone>();

	private List<Zone> zonesToCleanUp = new List<Zone>();

	private List<Commander> commandersBeingDeleted = new List<Commander>();

	public const float CMDER_DESTROY_TIME = 0.45f;

	public override void OnPhaseStart() {
		base.OnPhaseStart();
		//only zones with our commanders should have something happening 
		conflictZones.Clear();
		zonesToCleanUp.Clear();
		commandersBeingDeleted.Clear();
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		GameInfo gData = GameController.CurGameData;
		List<Commander> verifiedCmders = playerFac.OwnedCommanders;
		List<Zone> verifiedZones = playerFac.OwnedZones;

		if(gData.unifyBattlePhase && playerFac.ID == gData.factions[gData.factions.Count - 1].ID) {
			//if it's the "big post-battle", check all cmders instead of just the playing faction's
			verifiedCmders = gData.deployedCommanders;
		}

		infoTxt.text = "The effects of any battles(commanders disappearing, zones being taken) happen now";
		Zone zoneCmderIsIn = null;
		Faction curZoneOwnerFac = null;
		foreach (Commander cmder in verifiedCmders) {
			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if (!conflictZones.Contains(zoneCmderIsIn)) {
				if (zoneCmderIsIn.CanBeTakenBy(cmder.ownerFaction)) {
					conflictZones.Add(zoneCmderIsIn);
					curZoneOwnerFac = GameController.GetFactionByID(zoneCmderIsIn.ownerFaction);
					DiplomacyManager.GlobalReactToAttack(zoneCmderIsIn);
				}
			}
		}

		//also add owned zones that should become neutral due to being completely abandoned
		foreach(Zone z in verifiedZones) {
			if (!conflictZones.Contains(z)) {
				if(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(z, z.ownerFaction).Count == 0) {
					conflictZones.Add(z);
				}
			}
		}

		if(conflictZones.Count > 0) {
			StartCoroutine(GoToNextConflict());
		}else {
			infoTxt.text = "End of Turn!";
			OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
		}
	}

	public void SolveConflictsAt(Zone confZone) {

		List<Commander> cmdersInZone = GameController.GetCommandersInZone(confZone);
		
		Faction cmderFac = null, ownerFac = GameController.GetFactionByID(confZone.ownerFaction);

		bool zoneWasTaken = false, 
			zoneStillHasDefenders = GameController.GetTotalTroopAmountFromTroopList(
				GameController.GetCombinedTroopsInZoneFromFactionAndAllies
				(confZone, ownerFac)) > 0;

		//"kill" all commanders with no troops
		foreach (Commander c in cmdersInZone) {
			if(c.TotalTroopsContained == 0) {
				StartCoroutine(KillCommanderRoutine(c));
				//mark for tidying after all "dead" cmders have been removed
				if (!zonesToCleanUp.Contains(confZone)) {
					zonesToCleanUp.Add(confZone);
				}
			}else {
				//allies won't take the zone for themselves even if the zone ends up with 0 garrison
				//...but if an ally of theirs is an enemy to the owner, they won't take a side
				cmderFac = GameController.GetFactionByID(c.ownerFaction);
				if (cmderFac.GetStandingWith(ownerFac) != GameFactionRelations.FactionStanding.ally &&
					!zoneWasTaken) {
					//just take the zone if it's neutral, no more checks needed
					if(ownerFac == null) {
						confZone.ownerFaction = c.ownerFaction;
						//clear the points to avoid "insta-max-garrison"
						//when taking a zone that was piling points up
						confZone.pointsToSpend = 0;
						confZone.MyZoneSpot.RefreshDataDisplay();
						zoneWasTaken = true;
					}else {
						//the check made for zoneStillHasDefenders is not enough in the
						//"ally is also ally of my enemy" case
						if (GameController.GetTotalTroopAmountFromTroopList(
							GameController.GetCombinedTroopsInZoneFromFactionAndAllies
							(confZone, ownerFac.ID, cmderFac.ID)) == 0) {

							confZone.ownerFaction = c.ownerFaction;
							confZone.pointsToSpend = 0;
							confZone.MyZoneSpot.RefreshDataDisplay();
							zoneWasTaken = true;
						}
					}
					
					
				}
			}
		}

		if (!zoneWasTaken) {
			if(!zoneStillHasDefenders) {
				//the zone's been abandoned then
				confZone.ownerFaction = -1;
				confZone.MyZoneSpot.RefreshDataDisplay();
			}
		}


		conflictZones.RemoveAt(0);
		if (conflictZones.Count == 0) {
			OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
		}
		else {
			StartCoroutine(GoToNextConflict());
		}
	}

	/// <summary>
	/// jumps to the conflict zone and make any changes necessary
	/// </summary>
	/// <returns></returns>
	public IEnumerator GoToNextConflict() {
		if(!GameModeHandler.instance.currentTurnIsFast)
			CameraPanner.instance.JumpToSpot(conflictZones[0].MyZoneSpot.transform.position);
		yield return WaitWhileNoOverlays(0.1f);
		SolveConflictsAt(conflictZones[0]);
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		//wait for all commanders to actually "die"
		while (commandersBeingDeleted.Count > 0) yield return null;

		//then tidy up "dirty" zones
		foreach(Zone z in zonesToCleanUp) {
			World.TidyZone(z);
		}

		zonesToCleanUp.Clear();

		GameInfo gData = GameController.CurGameData;

		//report relation changes...
		gData.factionRelations.AnnounceAllRelationChanges();

		//move caravans if it's the "last faction"'s turn end
		if (GameModeHandler.instance.curPlayingFaction.ID == gData.factions[gData.factions.Count - 1].ID) {
			GameModeHandler.instance.MercCaravansPseudoTurn();
		}


		if (noWait) {
			yield return null;
		}else {
			yield return WaitWhileNoOverlays(0.25f); //some extra wait, since it's the turn's end
		}
		
		yield return base.ProceedToNextPhaseRoutine(noWait);
	}

	IEnumerator KillCommanderRoutine(Commander cmder) {
		commandersBeingDeleted.Add(cmder);
		Transform commander3dTrans = cmder.MeIn3d.transform;
		float elapsedTime = 0;

		while (elapsedTime < CMDER_DESTROY_TIME) {
			commander3dTrans.Rotate(Vector3.up * 200 * Time.deltaTime);
			commander3dTrans.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / CMDER_DESTROY_TIME);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		commandersBeingDeleted.Remove(cmder);
		GameController.RemoveCommander(cmder);
	}

	public override void OnPhaseEnded() {
		base.OnPhaseEnded();
		infoTxt.text = string.Empty;
	}
}
