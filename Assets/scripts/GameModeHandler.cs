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
		World.ToggleWorldDisplay(false);
		GameController.instance.facMatsHandler.PurgeFactionColorsDict();
	}

	public override void InitializeUI() {
		GameController.instance.facMatsHandler.ReBakeFactionColorsDict();
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.ToggleWorldDisplay(true);
		World.SetupAllZonesFromData();
		World.LinkAllZonesFromData();
		World.SetGroundSizeAccordingToRules();
		GameController.MakeFactionTurnPrioritiesUnique();
		//give initial points to zones if this is a new game
		GameInfo data = GameController.instance.curData as GameInfo;
		if (data.elapsedTurns == 0) {
			AddInitialPointsToZones();
		}

		StartNewTurn();
		
	}

	public void AddInitialPointsToZones() {
		foreach(Zone z in GameController.instance.curData.zones) {
			z.pointsToSpend = z.pointsGivenAtGameStart;
			z.SpendPoints(false);
		}
	}

	public void StartNewTurn() {
		//find out which faction turn it is now
		GameInfo data = GameController.instance.curData as GameInfo;
		curPlayingFaction = GameController.GetNextFactionInTurnOrder(data.lastTurnPriority);
		bigAnnouncer.DoBigAnnouncement(curPlayingFaction.name + "\nTurn", curPlayingFaction.color);
		whoseTurnTxt.text = curPlayingFaction.name;
		whoseTurnTxt.color = curPlayingFaction.color;
		curPhase = TurnPhase.newCmder;
		StartRespectivePhaseMan();
	}

	public void StartRespectivePhaseMan() {
		phasesTabGroup.ToggleTabIndex((int)curPhase);
		orderedPhaseManagers[(int)curPhase].OnPhaseStart();
	}

	public void GoToNextTurnPhase() {
		if(curPhase != TurnPhase.postBattle) {
			curPhase++;
			StartRespectivePhaseMan();
		}else {
			TurnFinished();
		}
	}


	public void TurnFinished() {
		GameInfo data = GameController.instance.curData as GameInfo;
		data.lastTurnPriority = curPlayingFaction.turnPriority;
		data.elapsedTurns++;
		StartNewTurn();
	}
}
