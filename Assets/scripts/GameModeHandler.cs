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

	private void Awake() {
		instance = this;
	}

	public override void Cleanup() {
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.CleanCmders();
		Cmder3dMover.instance.StopAllTweens();
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
		World.ToggleWorldDisplay(true);
		World.SetupAllZonesFromData();
		World.LinkAllZonesFromData();
		World.SetupAllCommandersFromData();
		World.instance.garrDescOnHoverScript.enabled = true;
		World.SetupBoardDetails();
		GameController.MakeFactionTurnPrioritiesUnique();
		//give initial points to zones if this is a new game
		GameInfo data = GameController.instance.curData as GameInfo;

		if(data.factionRelations == null ||
			data.factionRelations.relations.Count == 0) {
			data.factionRelations = new GameFactionRelations();
			data.factionRelations.SetDefaultRelationsBetweenAllFactions();
		}

		if (data.elapsedTurns == 0) {
			AddInitialPointsToZones();
			GameInterface.instance.gameOpsPanel.gameObject.SetActive(true);
			GameInterface.instance.gameOpsPanel.GoToAIPanel();
		}

		StartCoroutine(StartTurnAfterPanelsClose(data.curTurnPhase));
		
	}

	IEnumerator StartTurnAfterPanelsClose(TurnPhase startingPhase) {
		while(GameInterface.openedPanelsOverlayLevel > 0) {
			yield return null;
		}

		StartNewTurn(startingPhase);
	}



	public void AddInitialPointsToZones() {
		foreach(Zone z in GameController.instance.curData.zones) {
			z.pointsToSpend = z.pointsGivenAtGameStart;
			z.SpendPoints(false, true);
		}
	}

	public void StartNewTurn(TurnPhase startingPhase = TurnPhase.newCmder) {
		//find out which faction turn it is now
		GameInfo data = GameController.instance.curData as GameInfo;
		curPlayingFaction = GameController.GetNextFactionInTurnOrder(data.lastTurnPriority);
		
		if (GameController.IsFactionStillInGame(curPlayingFaction)) {
			currentTurnIsFast = (!curPlayingFaction.isPlayer && data.fastAiTurns);
			if(!currentTurnIsFast)
				bigAnnouncer.DoAnnouncement(curPlayingFaction.name + "\nTurn", curPlayingFaction.color);
			whoseTurnTxt.text = curPlayingFaction.name;
			whoseTurnTxt.color = curPlayingFaction.color;
			curPhase = startingPhase;
			data.curTurnPhase = curPhase;
			StartRespectivePhaseMan();
			turnPhasesContentsBox.SetActive(curPlayingFaction.isPlayer);
		}
		else{
			data.lastTurnPriority = curPlayingFaction.turnPriority;
			StartNewTurn();
			//TODO victory check, no factions capable of doing anything check
		}
		
	}

	public void StartRespectivePhaseMan() {
		phasesTabGroup.ToggleTabIndex((int)curPhase);
		orderedPhaseManagers[(int)curPhase].OnPhaseStart();
	}

	/// <summary>
	/// Interrupts all phases, properly stopping their routines and anything else they had running
	/// </summary>
	public void StopAllPhaseMans() {
		foreach(GamePhaseManager phaseMan in orderedPhaseManagers) {
			phaseMan.InterruptPhase();
		}
	}

	public void GoToNextTurnPhase() {
		if(curPhase != TurnPhase.postBattle) {
			curPhase++;
			GameInfo data = GameController.instance.curData as GameInfo;
			data.curTurnPhase = curPhase;
			StartRespectivePhaseMan();
		}else {
			TurnFinished();
		}
	}


	public void TurnFinished() {
		GameInfo data = GameController.instance.curData as GameInfo;
		data.lastTurnPriority = curPlayingFaction.turnPriority;
		data.elapsedTurns++;
		//TODO victory check
		StartNewTurn();
	}
}
