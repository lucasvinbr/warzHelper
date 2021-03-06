﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[XmlInclude(typeof(GameInfo))]
[System.Serializable]
public class TemplateInfo {

    public string gameName;

    public bool isATemplate = false;

	public Rules rules;

    public List<Faction> factions;

    public List<Zone> zones;

	public List<TroopType> troopTypes;

	public List<Commander> deployedCommanders;

	public List<MercCaravan> mercCaravans;

    public TemplateInfo(string name)
    {
        this.gameName = name;
        factions = new List<Faction>();
        zones = new List<Zone>();
		troopTypes = new List<TroopType>();
		deployedCommanders = new List<Commander>();
		mercCaravans = new List<MercCaravan>();
		rules = new Rules();
		this.isATemplate = true;
    }

    //empty constructor to enable xml serialization
    public TemplateInfo() {}
}
