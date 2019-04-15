using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostBattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> conflictZones = new List<Zone>();

	private List<Commander> commandersBeingDeleted = new List<Commander>();

	public const float CMDER_DESTROY_TIME = 0.75f;

	public override void OnPhaseStart() {
		//only zones with our commanders should have something happening 
		conflictZones.Clear();
		commandersBeingDeleted.Clear();
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		infoTxt.text = "The effects of any battles(commanders disappearing, zones being taken), happen now";
		Zone zoneCmderIsIn = null;
		Faction curZoneOwnerFac = null;
		foreach (Commander cmder in factionCmders) {
			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if (!conflictZones.Contains(zoneCmderIsIn)) {
				if (zoneCmderIsIn.CanBeTakenBy(playerFac)) {
					conflictZones.Add(zoneCmderIsIn);
					curZoneOwnerFac = GameController.GetFactionByID(zoneCmderIsIn.ownerFaction);
					DiplomacyManager.GlobalReactToAttack(playerFac, curZoneOwnerFac);
				}
			}
		}

		//also add owned zones that should become neutral due to being completely abandoned
		foreach(Zone z in playerFac.OwnedZones) {
			if (!conflictZones.Contains(z)) {
				if(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(z, playerFac).Count == 0) {
					conflictZones.Add(z);
				}
			}
		}

		if(conflictZones.Count > 0) {
			StartCoroutine(GoToNextConflict());
		}else {
			infoTxt.text = "End of Turn!";
			OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
		}
	}

	public void SolveConflictsAt(Zone confZone) {

		List<Commander> cmdersInZone = GameController.GetCommandersInZone(confZone);
		bool zoneWasTaken = false;
		Faction cmderFac = null, ownerFac = GameController.GetFactionByID(confZone.ownerFaction);
		//"kill" all commanders with no troops
		foreach(Commander c in cmdersInZone) {
			if(c.TotalTroopsContained == 0) {
				StartCoroutine(KillCommanderRoutine(c));
			}else {
				//allies won't take the zone for themselves even if the zone ends up with 0 garrison
				cmderFac = GameController.GetFactionByID(c.ownerFaction);
				if (cmderFac.GetStandingWith(ownerFac) != GameFactionRelations.FactionStanding.ally &&
					!zoneWasTaken) {

					confZone.ownerFaction = c.ownerFaction;
					//clear the points to avoid "insta-max-garrison"
					//when taking a zone that was piling points up
					confZone.pointsToSpend = 0; 
					confZone.MyZoneSpot.RefreshDataDisplay();
					zoneWasTaken = true;
				}
			}
		}

		if (!zoneWasTaken) {
			if(GameController.GetArmyAmountFromTroopList(
				GameController.GetCombinedTroopsInZoneFromFactionAndAllies
				(confZone, ownerFac)) == 0) {
				//the zone's been abandoned then
				confZone.ownerFaction = -1;
				confZone.MyZoneSpot.RefreshDataDisplay();
			}
		}


		conflictZones.RemoveAt(0);
		if (conflictZones.Count == 0) {
			OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
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
		yield return WaitWhileNoOverlays(0.35f);
		SolveConflictsAt(conflictZones[0]);
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		//wait for all commanders to actually "die"
		while (commandersBeingDeleted.Count > 0) yield return null;
		if (noWait) {
			yield return null;
		}else {
			yield return WaitWhileNoOverlays(0.4f); //some extra wait, since it's the turn's end
		}
		
		yield return base.ProceedToNextPhaseRoutine(noWait);
	}

	IEnumerator KillCommanderRoutine(Commander cmder) {
		commandersBeingDeleted.Add(cmder);
		Transform commander3dTrans = cmder.MeIn3d.transform;
		float elapsedTime = 0;

		while(elapsedTime < CMDER_DESTROY_TIME) {
			commander3dTrans.Rotate(Vector3.up * 200 * Time.deltaTime);
			commander3dTrans.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedTime / CMDER_DESTROY_TIME);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		commandersBeingDeleted.Remove(cmder);
		GameController.RemoveCommander(cmder);
	}
}
