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

	/// <summary>
	/// the last value entered when exporting battle info to JSON and setting the "split troops amount"
	/// </summary>
	public int lastEnteredExportTroopSplitAmt = 5;

	/// <summary>
	/// the last value for the variable name entered when exporting battle info to JSON with a custom "added variable"
	/// </summary>
	public string lastEnteredExportAddedVariable = "";

	/// <summary>
	/// the last value for the attackers' variable value entered when exporting battle info to JSON with a custom "added variable"
	/// </summary>
	public string lastEnteredExportAttackerVariable = "";

	/// <summary>
	/// the last value for the defenders' variable value entered when exporting battle info to JSON with a custom "added variable"
	/// </summary>
	public string lastEnteredExportDefenderVariable = "";

	public GameFactionRelations factionRelations;

	public UnifiedOrdersRegistry unifiedOrdersRegistry;

    public GameInfo(string name) : base(name)
    {
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		unifiedOrdersRegistry = new UnifiedOrdersRegistry();
		lastTurnPriority = -1;
    }

	public GameInfo(string name, TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		mercCaravans = baseData.mercCaravans;
		rules = baseData.rules;
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		unifiedOrdersRegistry = new UnifiedOrdersRegistry();
		lastTurnPriority = -1;
	}

	public void ImportDataFromTemplate(TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		mercCaravans = baseData.mercCaravans;
		rules = baseData.rules;
		this.isATemplate = false;
		factionRelations = new GameFactionRelations();
		unifiedOrdersRegistry = new UnifiedOrdersRegistry();
		lastTurnPriority = -1;
	}

    //empty constructor to enable xml serialization
    public GameInfo() {}
}
