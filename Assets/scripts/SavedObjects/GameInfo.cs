using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : TemplateInfo {

    public int lastTurnPriority;

    public int elapsedTurns = 0;

    public GameInfo(string name) : base(name)
    {
		this.isATemplate = false;
		lastTurnPriority = -1;
    }

	public GameInfo(string name, TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		rules = baseData.rules;
		this.isATemplate = false;
		lastTurnPriority = -1;
	}

	public void ImportDataFromTemplate(TemplateInfo baseData) {
		factions = baseData.factions;
		zones = baseData.zones;
		troopTypes = baseData.troopTypes;
		deployedCommanders = baseData.deployedCommanders;
		rules = baseData.rules;
		this.isATemplate = false;
		lastTurnPriority = -1;
	}

    //empty constructor to enable xml serialization
    public GameInfo() {}
}
