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
		foreach(Commander cmder in factionCmders) {
			zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
			if(!battleZones.Contains(zoneCmderIsIn) &&
				zoneCmderIsIn.ownerFaction != playerFac.ID) {
				if(zoneCmderIsIn.ownerFaction >= 0 && 
					GameController.GetCombinedTroopsInZoneFromFaction
					(zoneCmderIsIn, GameController.GetFactionByID(zoneCmderIsIn.ownerFaction)).Count > 0) {
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
			GameController.GetFactionByID(targetZone.ownerFaction), targetZone);
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


}
