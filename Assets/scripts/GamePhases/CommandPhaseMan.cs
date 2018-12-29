using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPhaseMan : GamePhaseManager {

	public Button getIdleCmderBtn;

	public CanvasGroup selectedCmderBtnsGroup;

	List<Commander> commandableCommanders = new List<Commander>();

	public WorldCommandPhase worldCommandScript;

	public override void OnPhaseStart() {
		//check if we've got any cmder to actually command
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		foreach(Commander cmder in factionCmders) {
			if(cmder.troopsContained.Count == 0 && cmder.pointsToSpend > 0) {
				cmder.RecruitTroops(); //if we don't recruit, we would just disappear at the end of the turn
			}else {
				commandableCommanders.Add(cmder);
			}
		}
		if(commandableCommanders.Count > 0) {
			worldCommandScript.enabled = true;
			worldCommandScript.allowedCmders3d = GameController.CmdersToCmder3ds(commandableCommanders);
			SelectCmder(commandableCommanders[0]);
		}else {
			SmallTextAnnouncer.instance.DoAnnouncement("No active commanders found!", Color.white);
			OnPhaseEnd();
		}
		
		
	}

	public void SelectCmder(Commander cmder) {
		CameraPanner.instance.JumpToSpot(cmder.MeIn3d.transform.position);
		worldCommandScript.SelectCmder(cmder.MeIn3d);
	}

	/// <summary>
	/// update the allowed zones for the world script and the UI for this script
	/// </summary>
	/// <param name="cmder"></param>
	public void OnCmderSelectedInWorld(Commander cmder) {
		selectedCmderBtnsGroup.interactable = true;
		Zone zoneCmderIsIn = GameController.GetZoneByID(cmder.zoneIAmIn);
		List<ZoneSpot> nearbySpots = new List<ZoneSpot>();

		foreach (int zoneID in zoneCmderIsIn.linkedZones) {
			nearbySpots.Add(GameController.GetZoneByID(zoneID).MyZoneSpot);
		}

		worldCommandScript.CmderSelectedSetAllowedZones(nearbySpots);
	}

	/// <summary>
	/// goes to the next idle cmder in the commandable commanders list.
	/// must be called before removing the active commander from the list
	/// </summary>
	public void GoToNextIdleCmder(Commander curActiveCmder) {
		if(commandableCommanders.Count <= 1) {
			return;
		}
		int curCmderIndex = commandableCommanders.IndexOf(curActiveCmder);
		int desiredCmderIndex = curCmderIndex + 1;
		if (desiredCmderIndex >= commandableCommanders.Count) {
			desiredCmderIndex = 0;
		}

		SelectCmder(commandableCommanders[desiredCmderIndex]);
	}

	public void GoToNextIdleCmder() {
		GoToNextIdleCmder(worldCommandScript.curSelectedCmder.data);
	}

	/// <summary>
	/// selects the next idle cmder and removes the provided cmder from the list
	/// </summary>
	/// <param name="cmder"></param>
	public void CmderHasActed(Commander cmder) {
		GoToNextIdleCmder(cmder);
		commandableCommanders.Remove(cmder);
		worldCommandScript.allowedCmders3d = GameController.CmdersToCmder3ds(commandableCommanders);
		if(commandableCommanders.Count == 0) {
			OnPhaseEnd();
		}
	}


	public void RecruitBtnPressed() {
		Commander curCmder = worldCommandScript.curSelectedCmder.data;
		if (curCmder.RecruitTroops()) {
			CmderHasActed(curCmder);
		}else {
			SmallTextAnnouncer.instance.DoAnnouncement("Couldn't recruit any troops!", Color.white);
		}

	}

	public void TrainingBtnPressed() {
		Commander curCmder = worldCommandScript.curSelectedCmder.data;
		if (curCmder.TrainTroops()) {
			CmderHasActed(curCmder);
		}
		else {
			SmallTextAnnouncer.instance.DoAnnouncement("Couldn't train any troops!", Color.white);
		}

	}

	public void MoveCommander(Cmder3d movingCmder3d, ZoneSpot destinationZone) {
		movingCmder3d.data.zoneIAmIn = destinationZone.data.ID;
		StartCoroutine(TweenCommanderToSpot(movingCmder3d, destinationZone.GetGoodSpotForCommander()));
		CmderHasActed(movingCmder3d.data);
	}

	public void SkipBtnPressed() {
		CmderHasActed(worldCommandScript.curSelectedCmder.data);
	}

	public override void OnPhaseEnd() {
		worldCommandScript.enabled = false;
		selectedCmderBtnsGroup.interactable = false;
		base.OnPhaseEnd();
	}


	IEnumerator TweenCommanderToSpot(Cmder3d movingCmder3d, Vector3 destSpot) {
		float elapsedTime = 0;
		Vector3 originalCmderPos = movingCmder3d.transform.position;

		while (elapsedTime < Rules.CMDER3D_ANIM_MOVE_DURATION) {
			movingCmder3d.transform.position =
				Vector3.Slerp(originalCmderPos, destSpot, 
				elapsedTime / Rules.CMDER3D_ANIM_MOVE_DURATION);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

	}

}
