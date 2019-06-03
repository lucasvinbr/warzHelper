using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPhaseMan : GamePhaseManager {

	public Button getIdleCmderBtn;

	public CanvasGroup selectedCmderBtnsGroup;

	List<Commander> commandableCommanders = new List<Commander>();

	public WorldCommandPhase worldCommandScript;

	public CmdPhaseCurCmderInfoBox curCmderInfoBox;

	public Toggle commandPhaseTab;

	private ColorBlock defaultTabColors;

	private bool isMultiOrdering = false;


	[Header("Colors when Multi-ordering is active")]
	public ColorBlock multiOrderingTabColors;

	private void Start() {
		defaultTabColors = commandPhaseTab.colors;
	}

	public override void OnPhaseStart() {
		base.OnPhaseStart();
		//check if we've got any cmder to actually command
		commandableCommanders.Clear();
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
			if (playerFac.isPlayer) {
				//sort commanders by zone so that the player's focus jumps around less often
				commandableCommanders.Sort(Commander.SortByZoneIAmIn);
				worldCommandScript.allowedCmders3d = GameController.CmdersToCmder3ds(commandableCommanders);
				worldCommandScript.enabled = true;
				curCmderInfoBox.gameObject.SetActive(true);
				SelectCmder(commandableCommanders[0]);
			}else {
				AiPlayer.AiCommandPhase(playerFac, commandableCommanders, this);
				OnPhaseEnding(GameModeHandler.instance.currentTurnIsFast);
			}
		}else {
			bool fastEnd = (GameModeHandler.instance.currentTurnIsFast);
			if(!fastEnd)
				SmallTextAnnouncer.instance.DoAnnouncement("No active commanders found!", Color.white);
			OnPhaseEnding(fastEnd);
		}
		
		
	}

	private void Update() {
		if (GameModeHandler.instance.curPlayingFaction != null && 
			GameModeHandler.instance.curPlayingFaction.isPlayer) {
			if (Input.GetButtonDown("Do Multi-order")) {
				ToggleMultiOrdering(true);
			}

			if (Input.GetButtonUp("Do Multi-order")) {
				ToggleMultiOrdering(false);
			}
		}
	}

	public void ToggleMultiOrdering(bool enable) {
		isMultiOrdering = enable;
		commandPhaseTab.colors = enable ? multiOrderingTabColors : defaultTabColors;

		if (worldCommandScript.curSelectedCmder) {
			SetCmderInfoBoxContent(worldCommandScript.curSelectedCmder.data);
		}
	}

	/// <summary>
	/// sets the content of the info box 
	/// taking multi-ordering into consideration:
	/// uses either the info on all cmders in the zone
	/// or with the target cmder's info only
	/// </summary>
	/// <param name="cmder"></param>
	private void SetCmderInfoBoxContent(Commander cmder) {
		if (isMultiOrdering) {
			Zone curZone = GameController.GetZoneByID(cmder.zoneIAmIn);
			Faction curFac = GameModeHandler.instance.curPlayingFaction;
			List<TroopNumberPair> cmderArmies = null;
			List<Commander> cmdersInZone = GameController.GetCommandersOfFactionInZone(curZone, curFac, out cmderArmies);
			curCmderInfoBox.SetContent(cmderArmies, cmdersInZone.Count, cmder);
		}else {
			curCmderInfoBox.SetContent(cmder);
		}
	}

	public void SelectCmder(Commander cmder) {
		CameraPanner.instance.TweenToSpot(GameController.GetZoneByID(cmder.zoneIAmIn).
			MyZoneSpot.transform.position);
		worldCommandScript.SelectCmder(cmder.MeIn3d);
		SetCmderInfoBoxContent(cmder);
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
	/// goes to the next idle cmder in the commandable commanders list,
	/// optionally tweening the cam to the only idle one left.
	/// must be called before removing the active commander from the list
	/// </summary>
	public void GoToNextIdleCmder(Commander curActiveCmder, bool moveCamToCmderIfNoNext = false) {
		if(commandableCommanders.Count <= 1) {
			if (moveCamToCmderIfNoNext) {
				CameraPanner.instance.TweenToSpot(worldCommandScript.curSelectedCmder.transform.position);
			}
			return;
		}
		int curCmderIndex = commandableCommanders.IndexOf(curActiveCmder);
		int desiredCmderIndex = curCmderIndex + 1;
		if (desiredCmderIndex >= commandableCommanders.Count) {
			desiredCmderIndex = 0;
		}

		SelectCmder(commandableCommanders[desiredCmderIndex]);
	}

	/// <summary>
	/// used by the UI btn
	/// </summary>
	public void GoToNextIdleCmder() {
		if (worldCommandScript.curSelectedCmder) {
			GoToNextIdleCmder(worldCommandScript.curSelectedCmder.data, true);
		}
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
			OnPhaseEnding();
		}
	}

	public void CmderBatchHasActed(List<Commander> actedCmders) {
		if(actedCmders.Count > 0) {
			int nextCmderIndex = commandableCommanders.IndexOf(actedCmders[0]);

			foreach(Commander cmd in actedCmders) {
				commandableCommanders.Remove(cmd);
			}

			worldCommandScript.allowedCmders3d = GameController.CmdersToCmder3ds(commandableCommanders);

			if (commandableCommanders.Count == 0) {
				OnPhaseEnding();
			}else {
				if (nextCmderIndex >= commandableCommanders.Count) nextCmderIndex = 0;
				SelectCmder(commandableCommanders[nextCmderIndex]);
			}
		}
		
	}


	public void RecruitBtnPressed() {
		if (isMultiOrdering) {
			List<Commander> actedCmders = new List<Commander>();

			foreach (Commander cmd in GameController.GetCommandersOfFactionInZone
				(GameController.GetZoneByID(worldCommandScript.curSelectedCmder.data.zoneIAmIn),
				GameModeHandler.instance.curPlayingFaction, commandableCommanders)) {
				if (cmd.RecruitTroops()) {
					actedCmders.Add(cmd);
				}
			}

			CmderBatchHasActed(actedCmders);
		}else {
			Commander curCmder = worldCommandScript.curSelectedCmder.data;
			if (curCmder.RecruitTroops()) {
				CmderHasActed(curCmder);
			}
			else {
				SmallTextAnnouncer.instance.DoAnnouncement("Couldn't recruit any troops!", Color.white);
			}
		}
		

	}

	public void TrainingBtnPressed() {
		if (isMultiOrdering) {
			List<Commander> actedCmders = new List<Commander>();

			foreach (Commander cmd in GameController.GetCommandersOfFactionInZone
				(GameController.GetZoneByID(worldCommandScript.curSelectedCmder.data.zoneIAmIn),
				GameModeHandler.instance.curPlayingFaction, commandableCommanders)) {
				if (cmd.TrainTroops()) {
					actedCmders.Add(cmd);
				}
			}

			CmderBatchHasActed(actedCmders);
		}
		else {
			Commander curCmder = worldCommandScript.curSelectedCmder.data;
			if (curCmder.TrainTroops()) {
				CmderHasActed(curCmder);
			}
			else {
				SmallTextAnnouncer.instance.DoAnnouncement("Couldn't train any troops!", Color.white);
			}
		}
		

	}

	public void MoveCommander(Cmder3d movingCmder3d, ZoneSpot destinationSpot, bool runHasActed = true) {
		Zone ourOldZone = GameController.GetZoneByID(movingCmder3d.data.zoneIAmIn);

		if (isMultiOrdering && runHasActed) {
			List<Commander> actedCmders = new List<Commander>();

			foreach (Commander cmd in GameController.GetCommandersOfFactionInZone
				(ourOldZone,
				GameModeHandler.instance.curPlayingFaction, commandableCommanders)) {
				cmd.zoneIAmIn = destinationSpot.data.ID;
				cmd.pointsToSpend = 0;
				TransformTweener.instance.StartTween(cmd.MeIn3d.transform, destinationSpot, true);
				actedCmders.Add(cmd);
			}

			World.TidyZone(ourOldZone);

			CmderBatchHasActed(actedCmders);
		}
		else {
			movingCmder3d.data.zoneIAmIn = destinationSpot.data.ID;
			movingCmder3d.data.pointsToSpend = 0;
			//reset other cmders' positions after departing
			World.TidyZone(ourOldZone);
			TransformTweener.instance.StartTween(movingCmder3d.transform, destinationSpot, true);
			if (runHasActed) CmderHasActed(movingCmder3d.data);
		}
		
		
	}

	public void SkipBtnPressed() {
		CmderHasActed(worldCommandScript.curSelectedCmder.data);
	}

	public override void OnPhaseEnding(bool noWait = false) {
		worldCommandScript.enabled = false;
		selectedCmderBtnsGroup.interactable = false;
		curCmderInfoBox.gameObject.SetActive(false);
		base.OnPhaseEnding(noWait);
	}

	public override void OnPhaseEnded() {
		base.OnPhaseEnded();
		ToggleMultiOrdering(false);
	}

	public override void InterruptPhase() {
		base.InterruptPhase();
		worldCommandScript.enabled = false;
		selectedCmderBtnsGroup.interactable = false;
		curCmderInfoBox.gameObject.SetActive(false);
	}

}
