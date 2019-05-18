using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameInfo : TemplateInfo {

    public int lastTurnPriority;

    public int elapsedTurns = 0;

	public GameModeHandler.TurnPhase curTurnPhase = GameModeHandler.TurnPhase.newCmder;

	public bool fastAiTurns = true, alwaysAutocalcAiBattles = false;

	/// <summary>
	/// this makes the battle phase only happen after all factions took turns,
	/// making it possible for allied factions to attack at the same time
	/// </summary>
	public bool unifyBattlePhase = false;

	public GameFactionRelations factionRelations;

    public GameInfo(string name) : base(name)
    {
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		lastTurnPriority = -1;
    }

	public GameInfo(string name, TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		rules = baseData.rules;
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		lastTurnPriority = -1;
	}

	public void ImportDataFromTemplate(TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		rules = baseData.rules;
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		lastTurnPriority = -1;
	}

    //empty constructor to enable xml serialization
    public GameInfo() {}
}
