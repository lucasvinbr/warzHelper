using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewCmderPhaseMan : GamePhaseManager {

	public Button skipBtn;
	public Text infoTxt;

	public override void OnPhaseStart() {
		//check if it's possible for the player faction to place a new cmder
		//(if the limit hasn't been reached and the faction has a zone without a commander in it)
		
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		
		if (factionCmders.Count < playerFac.MaxCmders) {
			List<Zone> availableZones = GameController.GetZonesForNewCmdersOfFaction(playerFac);
			if (availableZones.Count > 0) {
				if (playerFac.isPlayer) {
					CameraPanner.instance.TweenToSpot(availableZones[0].MyZoneSpot.transform.position);
					infoTxt.text = "Select an unoccupied zone you own to place a new commander in it";
					World.BeginNewCmderPlacement
						(DonePlacing, GameController.ZonesToZoneSpots(availableZones));
					skipBtn.interactable = true;
				}
				else {
					AiPlayer.AiNewCmderPhase(playerFac, availableZones);
					OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
				}
			}
			else {
				infoTxt.text = "All owned zones are already occupied!";
				OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
			}
		}
		else {
			infoTxt.text = "The faction's commander limit has been reached!";
			if (!GameModeHandler.instance.currentTurnIsFast)
				SmallTextAnnouncer.instance.DoAnnouncement
					("The faction's commander limit has been reached!", Color.white);
			OnPhaseEnd(GameModeHandler.instance.currentTurnIsFast);
		}
	}

	public void DonePlacing() {
		infoTxt.text = "Done!";
		OnPhaseEnd();
	}

	public void SkipBtn() {
		infoTxt.text = "Skipped!";
		OnPhaseEnd();
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		skipBtn.interactable = false;
		yield return base.ProceedToNextPhaseRoutine(noWait);
	}

	public override void InterruptPhase() {
		base.InterruptPhase();
		World.instance.cmderPlacerScript.enabled = false;
	}

}
