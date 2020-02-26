using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeHandler : ModeUI {

	public enum TurnPhase {
		newCmder,
		pointAward,
		command,
		battle,
		postBattle
	}

	public TurnPhase curPhase;

	public BigTextAnnouncer bigAnnouncer;

	public static GameModeHandler instance;

	/// <summary>
	/// the faction playing right now (it's this faction's turn).
	/// It should be set to null if no one is playing (if we're not in game mode, for example)
	/// </summary>
	public Faction curPlayingFaction;

	public GamePhaseManager[] orderedPhaseManagers;

	public TabToggleGroup phasesTabGroup;

	public Text whoseTurnTxt;

	public GameObject turnPhasesContentsBox;

	/// <summary>
	/// if true, the turn should be less noticeable as a whole
	/// </summary>
	public bool currentTurnIsFast = false;

	/// <summary>
	/// true if a turn is going to be started once all panels are closed
	/// </summary>
	private bool turnStartScheduled = false;

	private void Awake() {
		instance = this;
	}

	public override void Cleanup() {
		WorldVisualFeedbacks.instance.PoolAllOrderFeedbacks();
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.CleanCmders();
		World.CleanMCaravans();
		TransformTweener.instance.StopAllTweens();
		World.ToggleWorldDisplay(false);
		World.instance.garrDescOnHoverScript.enabled = false;
		StopAllPhaseMans();
		GameInterface.instance.DisableAndStoreAllOpenOverlayPanels();
		GameController.instance.facMatsHandler.PurgeFactionColorsDict();
		TexLoader.PurgeTexDict();
		curPlayingFaction = null;
	}

	public override void Initialize() {
		GameController.instance.facMatsHandler.ReBakeFactionColorsDict();
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.CleanCmders();
		World.CleanMCaravans();
		World.ToggleWorldDisplay(true);
		World.SetupAllZonesFromData();
		World.LinkAllZonesFromData();
		World.SetupAllCommandersFromData();
		World.instance.garrDescOnHoverScript.enabled = true;
		World.SetupAllMercCaravansFromData();
		World.SetupBoardDetails();
		GameController.MakeFactionTurnPrioritiesUnique();

		GameInfo data = GameController.CurGameData;

		if (data.factionRelations == null) {
			data.factionRelations = new GameFactionRelations();
		}

		if (data.factionRelations.relations.Count == 0) {
			data.factionRelations.SetDefaultRelationsBetweenAllFactions();
		}

		if (data.unifiedOrdersRegistry == null) {
			data.unifiedOrdersRegistry = new UnifiedOrdersRegistry();
		}

		//give initial points to zones if this is a new game
		if (data.elapsedTurns == 0) {
			AddInitialPointsToZones();
			//open the gameOptions panel to allow the player to make additional arrangements before the game actually starts
			GameInterface.instance.gameOpsPanel.gameObject.SetActive(true);
		}

		//check if all existing factions have entries in the relations list
		if (!data.factionRelations.CheckRelationListIntegrity()) {
			data.factionRelations.AddAnyMissingFacEntries();
		}

		ScheduleTurnStartAfterPanelsClose(data.curTurnPhase);
		

	}

	/// <summary>
	/// waits until no panels are open (looking at the overlayLevel from GameInterface)
	/// and then starts a new turn
	/// </summary>
	/// <param name="startingPhase"></param>
	/// <returns></returns>
	private IEnumerator StartTurnAfterPanelsClose(TurnPhase startingPhase, bool noFactionKillingThisTurn = false) {
		while (GameInterface.openedPanelsOverlayLevel > 0) {
			yield return null;
		}

		StartNewTurn(startingPhase, noFactionKillingThisTurn);
		turnStartScheduled = false;
	}

	/// <summary>
	/// starts a turn once all panels are closed
	/// (if it's not when the game mode is initializing, make sure to StopAllPhaseMans before starting a new turn!)
	/// </summary>
	/// <param name="startingPhase"></param>
	/// <param name="noFactionKillingThisTurn"></param>
	public void ScheduleTurnStartAfterPanelsClose(TurnPhase startingPhase, bool noFactionKillingThisTurn = false)
	{
		if (!turnStartScheduled)
		{
			StartCoroutine(StartTurnAfterPanelsClose(startingPhase, noFactionKillingThisTurn));
			turnStartScheduled = true;
		}
	}


	/// <summary>
	/// if zones have a "points given on game start" value,
	/// that value is given (and spent) now
	/// </summary>
	public void AddInitialPointsToZones() {
		foreach (Zone z in GameController.instance.curData.zones) {
			z.pointsToSpend = z.pointsGivenAtGameStart;
			z.SpendPoints(false, true);
		}
	}

	private void StartNewTurn(TurnPhase startingPhase = TurnPhase.newCmder, bool noFactionKillingThisTurn = false) {
		//find out which faction turn it is now
		GameInfo data = GameController.CurGameData;
		curPlayingFaction = GameController.GetNextFactionInTurnOrder(data.lastTurnPriority);

		if (GameController.ShouldFactionGetATurn(curPlayingFaction)) {
			currentTurnIsFast = (!curPlayingFaction.isPlayer && data.fastAiTurns);
			if (!currentTurnIsFast)
				bigAnnouncer.DoAnnouncement(curPlayingFaction.name + "\nTurn", curPlayingFaction.color);
			whoseTurnTxt.text = curPlayingFaction.name;
			whoseTurnTxt.color = curPlayingFaction.color;
			curPhase = startingPhase;
			data.curTurnPhase = curPhase;
			StartRespectivePhaseMan();
			turnPhasesContentsBox.SetActive(curPlayingFaction.isPlayer);
			data.unifiedOrdersRegistry.RefreshOrderFeedbacksVisibility();
		}
		else {
			data.lastTurnPriority = curPlayingFaction.turnPriority;
			if (!noFactionKillingThisTurn)
			{
				//kill this faction then!
				KillFaction(curPlayingFaction);
			}
			
			StartNewTurn(TurnPhase.newCmder, noFactionKillingThisTurn);
			//TODO "game should end" checks
		}

	}

	public void StartRespectivePhaseMan() {
		//set all tabs to Off to circumvent issues that may be caused by phases being skipped
		phasesTabGroup.SetAllTabsOff();
		phasesTabGroup.ToggleTabIndex((int)curPhase);
		orderedPhaseManagers[(int)curPhase].OnPhaseStart();
	}

	/// <summary>
	/// Interrupts all phases, properly stopping their routines and anything else they had running
	/// </summary>
	public void StopAllPhaseMans() {
		foreach (GamePhaseManager phaseMan in orderedPhaseManagers) {
			phaseMan.InterruptPhase();
		}
	}

	public void GoToNextTurnPhase() {
		GameInfo curData = GameController.CurGameData;
		if (curPhase != TurnPhase.postBattle) {
			curPhase++;
			curData.curTurnPhase = curPhase;
			StartRespectivePhaseMan();
		}
		else {
			TurnFinished();
		}
	}


	public void TurnFinished() {
		
		GameInfo data = GameController.CurGameData;

		data.lastTurnPriority = curPlayingFaction.turnPriority;
		data.elapsedTurns++;
		//TODO victory check
		//check for a special case:
		//in "unified mode" last faction in turn order isn't killed right away
		if (data.unifyBattlePhase && curPlayingFaction.ID == data.factions[data.factions.Count - 1].ID) {
			if (!GameController.ShouldFactionGetATurn(curPlayingFaction, false)) {
				KillFaction(curPlayingFaction);
			}
		}
		StartNewTurn();
	}

	/// <summary>
	/// makes all caravans think about moving
	/// </summary>
	public void MercCaravansPseudoTurn() {
		whoseTurnTxt.text = "MERC CARAVANS";
		whoseTurnTxt.color = Color.white;

		foreach (MercCaravan mc in GameController.CurGameData.mercCaravans) {
			mc.CaravanThinkMove();
		}
	}

	/// <summary>
	/// does the remove faction procedure and shows a message
	/// </summary>
	public void KillFaction(Faction killedFac) {
		ModalPanel.Instance().MessageBox(null, "Faction Eliminated", killedFac.name + " is no more!", null,
			null, null, null, false, ModalPanel.ModalMessageType.Ok); //TODO show the faction's logo if set
		GameController.DefeatFaction(killedFac);
	}
}
