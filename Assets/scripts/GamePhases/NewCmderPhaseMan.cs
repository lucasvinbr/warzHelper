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
		skipBtn.interactable = true;
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		if(factionCmders.Count < playerFac.MaxCmders) {
			List<Zone> availableZones = playerFac.OwnedZones;
			if(availableZones.Count > 0) {
				bool zoneIsOccupied = false;
				for(int i = availableZones.Count - 1; i >= 0; i--) {
					zoneIsOccupied = false;
					foreach(Commander cmd in factionCmders) {
						if(cmd.zoneIAmIn == availableZones[i].ID) {
							zoneIsOccupied = true;
							break;
						}
					}
					if (zoneIsOccupied) {
						availableZones.RemoveAt(i);
					}
				}

				//after removing occupied zones, if there are any left, proceed
				if(availableZones.Count > 0) {
					CameraPanner.instance.TweenToSpot(availableZones[0].MyZoneSpot.transform.position);
					infoTxt.text = "Select an unoccupied zone you own to place a new commander in it";
					World.BeginNewCmderPlacement
						(DonePlacing, GameController.ZonesToZoneSpots(availableZones));
				}
				else {
					infoTxt.text = "All owned zones are already occupied!";
					OnPhaseEnd();
				}
			}else {
				infoTxt.text = "You have no owned zones to place a commander in!";
				OnPhaseEnd();
			}
		}else {
			infoTxt.text = "The faction's commander limit has been reached!";
			OnPhaseEnd();
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

	public override IEnumerator ProceedToNextPhaseRoutine() {
		skipBtn.interactable = false;
		yield return base.ProceedToNextPhaseRoutine();
	}


}
