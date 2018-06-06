using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone {
	/// <summary>
	/// the zone's unique name
	/// </summary>
    public string name;

	/// <summary>
	/// any text the player might want to add about the zone
	/// </summary>
	public string extraInfo;

	/// <summary>
	/// name of the faction that currently owns this zone
	/// </summary>
    public string ownerFaction;

	/// <summary>
	/// file path of this zone's picture, shown in the zone description screen and the battle screen
	/// </summary>
	public string pictureFilePath = "";

	/// <summary>
	/// the other zones this one is linked to.
	/// commanders can only move between zones that are linked to each other
	/// </summary>
	public List<string> linkedZones;

	/// <summary>
	/// this value multiplies any points invested in training. 
	/// 0 makes all points do nothing, making training impossible in this zone
	/// </summary>
	public float multTrainingPoints = 1;

	/// <summary>
	/// this value multiplies any points invested in recruitment. 
	/// 0 makes all points do nothing, making recruitment impossible in this zone
	/// </summary>
	public float multRecruitmentPoints = 1;

	/// <summary>
	/// this value multiplies the number of troops that can be garrisoned in this zone.
	/// 0 makes this zone a no man's land, belonging to a faction only for as long as a commander is kept in it
	/// </summary>
	public float multMaxUnitsInGarrison = 1;

	/// <summary>
	/// the zone's position in the world. Does not take the board into account
	/// </summary>
	public Vector2 coords;

	public List<TroopNumberPair> troopsGarrisoned;

	public Zone() {}

    public Zone(string name)
    {
        this.name = name;
        while(GameController.GetZoneByName(name) != null)
        {
            this.name = name + " copy";
        }
        
    }
}
