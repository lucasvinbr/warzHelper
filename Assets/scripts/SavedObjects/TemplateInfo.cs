using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[XmlInclude(typeof(GameInfo))]
public class TemplateInfo {

    public string gameName;

    public bool isATemplate = false;

	public int lastIDGiven = -1;

	public Rules rules;

	public Board boardInfo;

    public List<Faction> factions;

    public List<Zone> zones;

	public List<TroopType> troopTypes;

	public List<Commander> deployedCommanders;

    public TemplateInfo(string name)
    {
        this.gameName = name;
        factions = new List<Faction>();
        zones = new List<Zone>();
		troopTypes = new List<TroopType>();
		deployedCommanders = new List<Commander>();
		rules = new Rules();
		boardInfo = new Board();
		this.isATemplate = true;
    }

    //empty constructor to enable xml serialization
    public TemplateInfo() {}
}
