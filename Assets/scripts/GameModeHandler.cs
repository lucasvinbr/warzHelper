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

		if(data.factionRelations == null ||
			data.factionRelations.relations.Count == 0) {
			data.factionRelations = new GameFactionRelations();
			data.factionRelations.SetDefaultRelationsBetweenAllFactions();
		}

		//give initial points to zones if this is a new game
		if (data.elapsedTurns == 0) {
			AddInitialPointsToZones();
			GameInterface.instance.gameOpsPanel.gameObject.SetActive(true);
		}

		StartCoroutine(StartTurnAfterPanelsClose(data.curTurnPhase));
		
	}

	/// <summary>
	/// waits until no panels are open (looking at the overlayLevel from GameInterface)
	/// and then starts a new turn
	/// </summary>
	/// <param name="startingPhase"></param>
	/// <returns></returns>
	IEnumerator StartTurnAfterPanelsClose(TurnPhase startingPhase) {
		while(GameInterface.openedPanelsOverlayLevel > 0) {
			yield return null;
		}

		StartNewTurn(startingPhase);
	}


	/// <summary>
	/// if zones have a "points given on game start" value,
	/// that value is given (and spent) now
	/// </summary>
	public void AddInitialPointsToZones() {
		foreach(Zone z in GameController.instance.curData.zones) {
			z.pointsToSpend = z.pointsGivenAtGameStart;
			z.SpendPoints(false, true);
		}
	}

	public void StartNewTurn(TurnPhase startingPhase = TurnPhase.newCmder) {
		//find out which faction turn it is now
		GameInfo data = GameController.CurGameData;
		curPlayingFaction = GameController.GetNextFactionInTurnOrder(data.lastTurnPriority);
		
		if (GameController.ShouldFactionGetATurn(curPlayingFaction)) {
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
			//kill this faction then!
			KillFaction(curPlayingFaction);
			StartNewTurn();
			//TODO "game should end" checks
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
		GameInfo curData = GameController.CurGameData;
		if (curPhase != TurnPhase.postBattle) {
			
			curPhase++;
			if (curPhase == TurnPhase.battle) {
				//skip this battle and post-battle phases if we're in "unified" mode
				//and it's not the last faction's turn
				if (curData.unifyBattlePhase) {
					if (curPlayingFaction.ID != curData.factions[curData.factions.Count - 1].ID) {
						curPhase = TurnPhase.postBattle;
						GoToNextTurnPhase();
						return;
					}
				}
			}
			curData.curTurnPhase = curPhase;
			StartRespectivePhaseMan();
		}else {
			//move caravans if it's the "last faction"'s turn end
			if (curPlayingFaction.ID == curData.factions[curData.factions.Count - 1].ID) {
				MercCaravansPseudoTurn();
			}

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
			if(!GameController.ShouldFactionGetATurn(curPlayingFaction, false)) {
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

		foreach(MercCaravan mc in GameController.CurGameData.mercCaravans) {
			mc.CaravanThinkMove();
		}
	}

	/// <summary>
	/// does the remove faction procedure and shows a message
	/// </summary>
	public void KillFaction(Faction killedFac) {
		ModalPanel.Instance().MessageBox(null, "Faction Eliminated", killedFac.name + " is no more!", null,
			null, null, null, false, ModalPanel.ModalMessageType.Ok); //TODO show the faction's logo if set
		GameController.RemoveFaction(killedFac);
	}
}
