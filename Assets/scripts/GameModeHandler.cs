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

	public Faction curPlayingFaction;

	public GamePhaseManager[] orderedPhaseManagers;

	public TabToggleGroup phasesTabGroup;

	public Text whoseTurnTxt;

	private void Awake() {
		instance = this;
	}

	public override void ClearUI() {
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.CleanCmders();
		World.ToggleWorldDisplay(false);
		World.instance.garrDescOnHoverScript.enabled = false;
		GameInterface.instance.DisableAndStoreAllOpenOverlayPanels();
		GameController.instance.facMatsHandler.PurgeFactionColorsDict();
		TexLoader.PurgeTexDict();
	}

	public override void InitializeUI() {
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
		if (data.elapsedTurns == 0) {
			AddInitialPointsToZones();
		}

		StartNewTurn(data.curTurnPhase);
		
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
		bigAnnouncer.DoAnnouncement(curPlayingFaction.name + "\nTurn", curPlayingFaction.color);
		whoseTurnTxt.text = curPlayingFaction.name;
		whoseTurnTxt.color = curPlayingFaction.color;
		curPhase = startingPhase;
		data.curTurnPhase = curPhase;
		StartRespectivePhaseMan();
	}

	public void StartRespectivePhaseMan() {
		phasesTabGroup.ToggleTabIndex((int)curPhase);
		orderedPhaseManagers[(int)curPhase].OnPhaseStart();
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
