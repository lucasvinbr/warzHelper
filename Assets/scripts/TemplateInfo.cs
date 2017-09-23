using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[XmlInclude(typeof(GameInfo))]
public class TemplateInfo {

    public string gameName;

    public bool isATemplate = false;

    public List<Faction> factions;

    public List<Zone> zones;

    public TemplateInfo(string name)
    {
        this.gameName = name;
        factions = new List<Faction>();
        zones = new List<Zone>();
    }

    //empty constructor to enable xml serialization
    public TemplateInfo()
    {

    }
}
