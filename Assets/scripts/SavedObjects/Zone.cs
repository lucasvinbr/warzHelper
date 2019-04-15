using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Zone : TroopContainer {

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


	/// <summary>
	/// coords for placing the zone in the world, using the saved y coord as z
	/// </summary>
	public Vector3 CoordsForWorld
	{
		get
		{
			return Vector3.right * coords.x + Vector3.forward * coords.y;
		}
	}

	/// <summary>
	/// calculated taking in account the zone's faction and factors
	/// </summary>
	public int MaxTroopsInGarrison
	{
		get
		{
			return Mathf.RoundToInt(GameController.GetFactionByID(ownerFaction).multMaxUnitsInOneGarrison * 
				multMaxUnitsInGarrison * 
				GameController.instance.curData.rules.baseMaxUnitsInOneGarrison);
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
		troopsContained = new List<TroopNumberPair>();
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
		Faction ownerFac = GameController.GetFactionByID(ownerFaction);
		int basePoints = GameController.instance.curData.rules.baseZonePointAwardOnTurnStart;

		pointsToSpend += Mathf.RoundToInt(basePoints * ownerFac.multZonePointAwardOnTurnStart);
	}

	/// <summary>
	/// if we're not neutral, uses the pointsToSpend to add more troops to the garrison
	/// and upgrade them
	/// </summary>
	public void SpendPoints(bool useGraphicFX = false, bool ignoreGarrisonTierLimit = false) {
		if(ownerFaction >= 0 && pointsToSpend > 0) {
			bool hasRecruited = false, hasTrained = false;
			Faction ownerFac = GameController.GetFactionByID(ownerFaction);
			if(TotalTroopsContained < MaxTroopsInGarrison && multRecruitmentPoints > 0) {
				//recruitment!
				if(ownerFac.troopLine.Count > 0) {
					TroopType baseFactionTroop = GameController.GetTroopTypeByID(ownerFac.troopLine[0]);
					int troopRecruitmentCostHere = 
						Mathf.RoundToInt(baseFactionTroop.pointCost / multRecruitmentPoints);
					int recruitableTroopsAmount = 0;
					//this troop can be so cheap and the zone so good that the troop ends up with cost 0 after rounding
					if (troopRecruitmentCostHere == 0) {
						recruitableTroopsAmount = MaxTroopsInGarrison - TotalTroopsContained;
					}
					else {
						recruitableTroopsAmount = Mathf.Min(pointsToSpend / troopRecruitmentCostHere,
							MaxTroopsInGarrison - TotalTroopsContained);
					}
					AddTroop(baseFactionTroop.ID, recruitableTroopsAmount);
					pointsToSpend -= troopRecruitmentCostHere * recruitableTroopsAmount;
					hasRecruited = true;
				}
				
			}

			//if we still have points left... training time
			if(pointsToSpend > 0 && multTrainingPoints > 0) {
				int trainableTroops = 0;
				int troopTrainingCostHere = 0;
				int troopIndexInGarrison = -1;
				int tierLimit = ignoreGarrisonTierLimit ? ownerFac.troopLine.Count - 1 : ownerFac.maxGarrisonedTroopTier - 1;
				TroopType curTTBeingTrained = null, curTTUpgradeTo = null;
				for(int i = 0; i < tierLimit; i++) {
					
					troopIndexInGarrison = IndexOfTroopInContainer(ownerFac.troopLine[i]);
					if(troopIndexInGarrison >= 0) {
						curTTBeingTrained = GameController.GetTroopTypeByID(ownerFac.troopLine[i]);
						curTTUpgradeTo = GameController.GetTroopTypeByID(ownerFac.troopLine[i + 1]);
						troopTrainingCostHere = Mathf.RoundToInt(curTTUpgradeTo.pointCost / multRecruitmentPoints);
						if (troopTrainingCostHere == 0) {
							trainableTroops = troopsContained[troopIndexInGarrison].troopAmount;
						}
						else {
							trainableTroops = Mathf.Min(pointsToSpend / troopTrainingCostHere,
								troopsContained[troopIndexInGarrison].troopAmount);
						}
						if(trainableTroops > 0) {
							RemoveTroop(curTTBeingTrained.ID, trainableTroops);
							AddTroop(curTTUpgradeTo.ID, trainableTroops);
							pointsToSpend -= trainableTroops * troopTrainingCostHere;
							hasTrained = true;
						}
					}
				}
			}

			troopsContained.Sort(TroopNumberPair.CompareTroopNumberPairsByAutocalcPower);

			if (useGraphicFX) {
				if (hasRecruited) {
					WorldFXManager.instance.EmitParticle(WorldFXManager.instance.recruitParticle, MyZoneSpot.transform.position,
					GameController.GetFactionByID(ownerFaction).color);
				}

				if (hasTrained) {
					WorldFXManager.instance.EmitParticle(WorldFXManager.instance.bolsterParticle, MyZoneSpot.transform.position,
					GameController.GetFactionByID(ownerFaction).color);
				}
				
			}
		}
	}

	/// <summary>
	/// this version here is only used when receiving points for a victory
	/// </summary>
	public override void TrainTroops() {
		if (pointsToSpend > 0 && multTrainingPoints > 0) {
			bool hasTrained = false;
			int trainableTroops = 0;
			int troopTrainingCostHere = 0;
			int troopIndexInGarrison = -1;
			Faction ownerFac = GameController.GetFactionByID(ownerFaction);
			TroopType curTTBeingTrained = null, curTTUpgradeTo = null;
			for (int i = 0; i < ownerFac.maxGarrisonedTroopTier - 1; i++) {

				troopIndexInGarrison = IndexOfTroopInContainer(ownerFac.troopLine[i]);
				if (troopIndexInGarrison >= 0) {
					curTTBeingTrained = GameController.GetTroopTypeByID(ownerFac.troopLine[i]);
					curTTUpgradeTo = GameController.GetTroopTypeByID(ownerFac.troopLine[i + 1]);
					troopTrainingCostHere = Mathf.RoundToInt(curTTUpgradeTo.pointCost / multRecruitmentPoints);
					if (troopTrainingCostHere == 0) {
						trainableTroops = troopsContained[troopIndexInGarrison].troopAmount;
					}
					else {
						trainableTroops = Mathf.Min(pointsToSpend / troopTrainingCostHere,
							troopsContained[troopIndexInGarrison].troopAmount);
					}
					if (trainableTroops > 0) {
						RemoveTroop(curTTBeingTrained.ID, trainableTroops);
						AddTroop(curTTUpgradeTo.ID, trainableTroops);
						pointsToSpend -= trainableTroops * troopTrainingCostHere;
						hasTrained = true;
					}
				}
			}

			if (hasTrained) {
				WorldFXManager.instance.EmitParticle(WorldFXManager.instance.bolsterParticle, MyZoneSpot.transform.position,
				GameController.GetFactionByID(ownerFaction).color);
			}
		}
	}

	/// <summary>
	/// a zone can be taken by a faction if it's neutral,
	/// if it's NOT already controlled by said faction
	/// or if it's controlled by a faction that is NOT allied to the invader
	/// </summary>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public bool CanBeTakenBy(Faction targetFac) {
		return ((ownerFaction != targetFac.ID) && (ownerFaction < 0 || 
			GameController.GetFactionByID(ownerFaction).GetStandingWith(targetFac) !=
				GameFactionRelations.FactionStanding.ally));
	}

}
