using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointAwardPhaseMan : GamePhaseManager {

	public Text infoTxt;

	public override void OnPhaseStart() {
		base.OnPhaseStart();
		//add points to all zones and cmders of the playing faction
		//if in unified mode, this phase is skipped for all factions except the first one in turn order, in which everyone gets points
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		GameInfo curGameData = GameController.CurGameData;

		if (curGameData.unifyBattlePhase) {
			if (playerFac.ID != curGameData.factions[0].ID) {
				infoTxt.text = "Unified Mode: Phase only runs in first faction's turn";
				OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
				return;
			}
		}

		List<Commander> awardedCmders = curGameData.unifyBattlePhase ? curGameData.deployedCommanders : playerFac.OwnedCommanders;
		List<Zone> awardedZones = curGameData.unifyBattlePhase ? curGameData.zones : playerFac.OwnedZones;
		infoTxt.text = "Awarding points to each zone and commander; zones will auto-use them";
		foreach(Commander cmd in awardedCmders) {
			cmd.GetPointAwardPoints();
		}

		foreach (Zone z in awardedZones) {
			z.GetPointAwardPoints();
			z.SpendPoints(true); //zones that actually do something with their points will emit some effects
		}

		OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		if (noWait) {
			yield return null;
		}
		else {
			yield return WaitWhileNoOverlays(0.8f); //some extra wait, since we've got effects
		}
		
		yield return base.ProceedToNextPhaseRoutine(noWait);
	}

}
