using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointAwardPhaseMan : GamePhaseManager {

	public Text infoTxt;

	public override void OnPhaseStart() {
		//add points to all zones and cmders of the playing faction
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		List<Zone> factionZones = playerFac.OwnedZones;
		infoTxt.text = "Awarding points to each zone and commander; zones will auto-use them";
		foreach(Commander cmd in factionCmders) {
			cmd.GetPointAwardPoints();
		}

		foreach (Zone z in factionZones) {
			z.GetPointAwardPoints();
			z.SpendPoints(true); //zones that actually do something with their points will emit some effects
		}

		OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
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
