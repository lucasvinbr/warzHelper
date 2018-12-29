using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Commander : TroopContainer {
	public int ID;

	public int zoneIAmIn;

	public int pointsToSpend = 0;

	public int MaxTroopsCommanded
	{
		get
		{
			return Mathf.RoundToInt(GameController.GetFactionByID(ownerFaction).multMaxUnitsUnderOneCommander *
				GameController.instance.curData.rules.baseMaxUnitsUnderOneCommander);
		}
	}

	private Cmder3d _meIn3d;

	public Cmder3d MeIn3d
	{
		get
		{
			if(_meIn3d == null) {
				_meIn3d = World.GetCmder3dForCommander(this);
			}
			return _meIn3d;
		}
	}

	public Commander() { }

	public Commander(int ownerFactionID, int zoneStartingLocation) {
		this.ID = GameController.GetUnusedZoneID();
		this.ownerFaction = ownerFactionID;
		this.zoneIAmIn = zoneStartingLocation;
		troopsContained = new List<TroopNumberPair>();
		GameController.instance.curData.deployedCommanders.Add(this);
	}

	/// <summary>
	/// receives points according to our owner faction
	/// </summary>
	public void GetPointAwardPoints() {
		Faction ownerFac = GameController.GetFactionByID(ownerFaction);
		int basePoints = GameController.instance.curData.rules.baseCommanderPointAwardOnTurnStart;

		pointsToSpend += Mathf.RoundToInt(basePoints * ownerFac.multCommanderPointAwardOnTurnStart);
	}

	/// <summary>
	/// true if at least 1 troop was upgraded
	/// </summary>
	/// <returns></returns>
	public bool TrainTroops() {
		Zone curZone = GameController.GetZoneByID(zoneIAmIn);
		Faction ownerFac = GameController.GetFactionByID(ownerFaction);
		bool hasTrained = false;
		if (pointsToSpend > 0 && curZone.multTrainingPoints > 0) {
			int trainableTroops = 0;
			int troopTrainingCostHere = 0;
			int troopIndexInGarrison = -1;
			TroopType curTTBeingTrained = null, curTTUpgradeTo = null;
			for (int i = 0; i < ownerFac.troopLine.Count - 1; i++) { //the last one can't upgrade, so...

				troopIndexInGarrison = IndexOfTroopInContainer(ownerFac.troopLine[i]);
				if (troopIndexInGarrison >= 0) {
					curTTBeingTrained = GameController.GetTroopTypeByID(ownerFac.troopLine[i]);
					curTTUpgradeTo = GameController.GetTroopTypeByID(ownerFac.troopLine[i + 1]);
					troopTrainingCostHere = Mathf.RoundToInt(curTTUpgradeTo.pointCost / curZone.multTrainingPoints);
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
				WorldFXManager.instance.EmitParticle(WorldFXManager.instance.bolsterParticle, MeIn3d.transform.position,
					GameController.GetFactionByID(ownerFaction).color);
				troopsContained.Sort(TroopNumberPair.CompareTroopNumberPairsByAutocalcPower);
			}
		}

		return hasTrained;
	}

	/// <summary>
	/// true if at least 1 troop was successfully recruited
	/// </summary>
	/// <returns></returns>
	public bool RecruitTroops() {
		Zone curZone = GameController.GetZoneByID(zoneIAmIn);
		Faction ownerFac = GameController.GetFactionByID(ownerFaction);
		if (TotalTroopsContained < MaxTroopsCommanded && curZone.multRecruitmentPoints > 0) {
			//recruitment!
			if (ownerFac.troopLine.Count > 0) {
				TroopType baseFactionTroop = GameController.GetTroopTypeByID(ownerFac.troopLine[0]);
				int troopRecruitmentCostHere =
					Mathf.RoundToInt(baseFactionTroop.pointCost / curZone.multRecruitmentPoints);
				int recruitableTroopsAmount = 0;
				//this troop can be so cheap and the zone so good that the troop ends up with cost 0 after rounding
				if (troopRecruitmentCostHere == 0) {
					recruitableTroopsAmount = MaxTroopsCommanded - TotalTroopsContained;
				}
				else {
					recruitableTroopsAmount = Mathf.Min(pointsToSpend / troopRecruitmentCostHere,
						MaxTroopsCommanded - TotalTroopsContained);
				}
				AddTroop(baseFactionTroop.ID, recruitableTroopsAmount);
				pointsToSpend -= troopRecruitmentCostHere * recruitableTroopsAmount;
				WorldFXManager.instance.EmitParticle(WorldFXManager.instance.recruitParticle, MeIn3d.transform.position,
					GameController.GetFactionByID(ownerFaction).color);
				troopsContained.Sort(TroopNumberPair.CompareTroopNumberPairsByAutocalcPower);
				return true;
				
			}

		}

		return false;
	}

}

