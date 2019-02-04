using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PostBattlePhaseMan : GamePhaseManager {

	public Text infoTxt;

	public List<Zone> conflictZones = new List<Zone>();

	private List<Commander> commandersBeingDeleted = new List<Commander>();

	public const float CMDER_DESTROY_TIME = 1.0f;

	public override void OnPhaseStart() {
		//only zones with our commanders should have something happening 
		conflictZones.Clear();
		commandersBeingDeleted.Clear();
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		infoTxt.text = "The effects of any battles(commanders disappearing, zones being taken), happen now";
		Zone zoneCmderIsIn = null;
		foreach (Commander cmder in factionCmders) {
			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if (!conflictZones.Contains(zoneCmderIsIn) &&
				zoneCmderIsIn.ownerFaction != playerFac.ID) {
				conflictZones.Add(zoneCmderIsIn);
			}
		}

		//also add owned zones that should become neutral due to being completely abandoned
		foreach(Zone z in playerFac.OwnedZones) {
			if (!conflictZones.Contains(z)) {
				if(GameController.GetCombinedTroopsInZoneFromFaction(z, playerFac).Count == 0) {
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
		//"kill" all commanders with no troops
		foreach(Commander c in cmdersInZone) {
			if(c.TotalTroopsContained == 0) {
				StartCoroutine(KillCommanderRoutine(c));
			}else {
				if(c.ownerFaction != confZone.ownerFaction && !zoneWasTaken) {
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
				GameController.GetCombinedTroopsInZoneFromFaction
				(confZone, GameController.GetFactionByID(confZone.ownerFaction))) == 0) {
				//the zone's been abandoned then
				confZone.ownerFaction = -1;
				confZone.MyZoneSpot.RefreshDataDisplay();
			}
		}


		conflictZones.RemoveAt(0);
		if (conflictZones.Count == 0) {
			OnPhaseEnd();
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
		CameraPanner.instance.TweenToSpot(conflictZones[0].MyZoneSpot.transform.position);
		yield return WaitWhileNoOverlays(0.35f);
		SolveConflictsAt(conflictZones[0]);
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		//wait for all commanders to actually "die"
		while (commandersBeingDeleted.Count > 0) yield return null;
		if (noWait) {
			yield return null;
		}else {
			yield return WaitWhileNoOverlays(0.8f); //some extra wait, since it's the turn's end
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
