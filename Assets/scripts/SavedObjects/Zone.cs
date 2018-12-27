using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Zone {

	/// <summary>
	/// the zone's unique ID
	/// </summary>
	public int ID;


	/// <summary>
	/// the zone's unique name
	/// </summary>
	public string name;

	/// <summary>
	/// any text the player might want to add about the zone
	/// </summary>
	public string extraInfo;

	/// <summary>
	/// ID of the faction that currently owns this zone. a negative number probably means this zone is neutral
	/// </summary>
	public int ownerFaction;

	/// <summary>
	/// file path of this zone's picture, shown in the zone description screen and the battle screen
	/// </summary>
	public string pictureFilePath = "";

	/// <summary>
	/// the other zones this one is linked to.
	/// commanders can only move between zones that are linked to each other
	/// </summary>
	public List<int> linkedZones;

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
	/// the amount of points given to this zone at the beginning of the game.
	/// these points are used both for recruiting and upgrading (first recruiting then upgrading)
	/// and are not affected by the zone's multipliers
	/// </summary>
	public int pointsGivenAtGameStart = 0;

	/// <summary>
	/// the zone's position in the world. Does not take the board into account, and the Y is actually Z! (use CoordsForWorld when placing the zone)
	/// </summary>
	public Vector2 coords;

	public int pointsToSpend = 0;

	/// <summary>
	/// coords for placing the zone in the world, using the saved y coord as z
	/// </summary>
	public Vector3 CoordsForWorld{
		get{
			return Vector3.right * coords.x + Vector3.forward * coords.y;
		}
	}

	public List<TroopNumberPair> troopsGarrisoned;

	public int TotalTroopsInGarrison
	{
		get
		{
			int total = 0;
			for(int i = 0; i < troopsGarrisoned.Count; i++) {
				total += troopsGarrisoned[i].troopAmount;
			}
			return total;
		}
	}

	private ZoneSpot _mySpot;

	/// <summary>
	/// basically a cached GetZoneSpotByZoneName
	/// </summary>
	public ZoneSpot MyZoneSpot
	{
		get
		{
			if(_mySpot == null) {
				_mySpot = World.GetZoneSpotByZoneName(name);
			}

			return _mySpot;
		}
	}

	public Zone() { }

	public Zone(string name) {
		this.ID = GameController.GetUnusedZoneID();
		this.name = name;
		this.ownerFaction = -1;
		troopsGarrisoned = new List<TroopNumberPair>();
		linkedZones = new List<int>();
		while (GameController.GetZoneByName(this.name) != null) {
			this.name = name + " copy";
		}
		GameController.instance.curData.zones.Add(this);
	}


	/// <summary>
	/// gets points according to our owner faction and our own factors
	/// </summary>
	public void GetPointAwardPoints() {

	}

	/// <summary>
	/// if we're not neutral, uses the pointsToSpend to add more troops to the garrison
	/// and upgrade them
	/// </summary>
	public void SpendPoints(bool useGraphicFX = false) {
		if(ownerFaction >= 0 && pointsToSpend > 0) {
			if (useGraphicFX) {

				WorldFXManager.instance.EmitParticle(WorldFXManager.instance.bolsterParticle, MyZoneSpot.transform.position,
					GameController.GetFactionByID(ownerFaction).color);
			}
		}
		//TODO spend points haha
	}
	
}
