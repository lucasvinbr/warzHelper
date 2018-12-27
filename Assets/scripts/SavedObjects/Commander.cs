using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Commander {
	public int ID;

	public int ownerFaction;

	public int zoneIAmIn;

	public List<TroopNumberPair> troopsCommanded;

	public int pointsToSpend = 0;

	public Commander() { }

	public Commander(int ownerFactionID, int zoneStartingLocation) {
		this.ID = GameController.GetUnusedZoneID();
		this.ownerFaction = ownerFactionID;
		this.zoneIAmIn = zoneStartingLocation;
		troopsCommanded = new List<TroopNumberPair>();
		GameController.instance.curData.deployedCommanders.Add(this);
	}

	/// <summary>
	/// receives points according to the zone we are in and owner faction
	/// </summary>
	public void GetPointAwardPoints() {

	}
}

