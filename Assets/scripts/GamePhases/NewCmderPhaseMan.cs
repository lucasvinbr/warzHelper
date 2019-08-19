using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewCmderPhaseMan : GamePhaseManager {

	public Button skipBtn;
	public Text infoTxt;

	public override void OnPhaseStart() {
		base.OnPhaseStart();
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
					OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
				}
			}
			else {
				infoTxt.text = "All owned zones are already occupied!";
				OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
			}
		}
		else {
			infoTxt.text = "The faction's commander limit has been reached!";
			if (!GameModeHandler.instance.currentTurnIsFast)
				SmallTextAnnouncer.instance.DoAnnouncement
					("The faction's commander limit has been reached!", Color.white);
			OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
		}
	}

	public void DonePlacing() {
		infoTxt.text = "Done!";
		OnPhaseEnding();
	}

	public void SkipBtn() {
		infoTxt.text = "Skipped!";
		OnPhaseEnding();
	}

	public override void OnPhaseEnding(bool noWait = false) {
		World.instance.cmderPlacerScript.enabled = false;
		base.OnPhaseEnding(noWait);
	}

	public override IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		skipBtn.interactable = false;
		yield return base.ProceedToNextPhaseRoutine(noWait);
	}

	public override void InterruptPhase() {
		base.InterruptPhase();
		World.instance.cmderPlacerScript.enabled = false;
	}


	/// <summary>
	/// places the new commander right away or waits for the "unified action turn" to do it
	/// </summary>
	/// <param name="targetZoneID"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static bool OrderPlaceNewCmder(int targetZoneID, Faction targetFac) {
		Zone targetZone = GameController.GetZoneByID(targetZoneID);

		if (targetZone == null) {
			return false;
		}

		return OrderPlaceNewCmder(targetZone, targetFac);
	}

	/// <summary>
	/// places the new commander right away or waits for the "unified action turn" to do it
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static bool OrderPlaceNewCmder(Zone targetZone, Faction targetFac) {

		if (GameController.CurGameData.unifyBattlePhase) {
			if (targetZone.ownerFaction == targetFac.ID) {
				GameController.CurGameData.unifiedOrdersRegistry.RegisterOrder
					(RegisteredCmderOrder.OrderType.createCmder, targetFac.ID, targetZone.ID, targetFac.isPlayer);
				return true;
			}
			else return false;

		}
		else {
			World.CreateNewCmderAtZone(targetZone, targetFac);
			return true;
		}
	}
}
