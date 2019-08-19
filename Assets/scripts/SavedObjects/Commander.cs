using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class Commander : TroopContainer {
	public int ID;

	public int zoneIAmIn;


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
			if (_meIn3d == null) {
				_meIn3d = World.GetCmder3dForCommander(this);
				if (_meIn3d == null) {
					Debug.LogWarning("no 3d for cmder! cmderID: " + ID);
				}
			}
			return _meIn3d;
		}
	}

	public Commander() { }

	public Commander(int ownerFactionID, int zoneStartingLocation) {
		this.ID = GameController.GetUnusedCmderID();
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
	/// trains right away or registers the order for "action time" if we're playing in unified mode
	/// </summary>
	/// <returns></returns>
	public bool OrderTrainTroops() {
		if (GameController.CurGameData.unifyBattlePhase) {
			if (GetPercentOfTroopsUpgradedIfTrained() > 0.0f) {
				GameController.CurGameData.unifiedOrdersRegistry.RegisterOrder
					(RegisteredCmderOrder.OrderType.train, ID, addVisualFeedbackNow: GameController.GetFactionByID(ownerFaction).isPlayer);
				return true;
			}
			else {
				return false;
			}
		}
		else {
			return TrainTroops();
		}
	}

	/// <summary>
	/// true if at least 1 troop was upgraded
	/// </summary>
	/// <returns></returns>
	public override bool TrainTroops() {
		if (pointsToSpend <= 0) return false;

		Zone curZone = GameController.GetZoneByID(zoneIAmIn);

		if (curZone.multTrainingPoints > 0) {
			Faction ownerFac = GameController.GetFactionByID(ownerFaction);
			bool hasTrained = false;
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

			return hasTrained;
		}
		else {
			return false;
		}

	}

	/// <summary>
	/// recruits right away or registers the order for "action time" if we're playing in unified mode
	/// </summary>
	/// <returns></returns>
	public bool OrderRecruitTroops() {
		if (GameController.CurGameData.unifyBattlePhase) {
			if (GetPercentOfNewTroopsIfRecruited() > 0.0f) {
				GameController.CurGameData.unifiedOrdersRegistry.RegisterOrder
					(RegisteredCmderOrder.OrderType.recruit, ID, addVisualFeedbackNow: GameController.GetFactionByID(ownerFaction).isPlayer);
				return true;
			}
			else {
				return false;
			}
		}
		else {
			return RecruitTroops();
		}
	}

	/// <summary>
	/// true if at least 1 troop was successfully recruited
	/// </summary>
	/// <returns></returns>
	public override bool RecruitTroops() {
		Zone curZone = GameController.GetZoneByID(zoneIAmIn);
		Faction ownerFac = GameController.GetFactionByID(ownerFaction);
		if (TotalTroopsContained < MaxTroopsCommanded && curZone.multRecruitmentPoints > 0) {
			//recruitment!

			TroopType recruitableTroopType = GetTroopTypeRecruitedWhereIAm(ownerFac);
			if (recruitableTroopType == null) return false;

			int troopRecruitmentCostHere =
				Mathf.RoundToInt(recruitableTroopType.pointCost / curZone.multRecruitmentPoints);
			int recruitableTroopsAmount = 0;
			//this troop can be so cheap and the zone so good that the troop ends up with cost 0 after rounding
			if (troopRecruitmentCostHere == 0) {
				recruitableTroopsAmount = MaxTroopsCommanded - TotalTroopsContained;
			}
			else {
				recruitableTroopsAmount = Mathf.Min(pointsToSpend / troopRecruitmentCostHere,
					MaxTroopsCommanded - TotalTroopsContained);
			}
			AddTroop(recruitableTroopType.ID, recruitableTroopsAmount);
			pointsToSpend -= troopRecruitmentCostHere * recruitableTroopsAmount;
			WorldFXManager.instance.EmitParticle(WorldFXManager.instance.recruitParticle, MeIn3d.transform.position,
				GameController.GetFactionByID(ownerFaction).color);
			troopsContained.Sort(TroopNumberPair.CompareTroopNumberPairsByAutocalcPower);
			return true;

		}

		return false;
	}

	/// <summary>
	/// moves right now or registers it for the proper "unified action" turn
	/// </summary>
	/// <param name="zoneID"></param>
	/// <returns></returns>
	public bool OrderMoveToZone(int zoneID) {
		Zone curZone = GameController.GetZoneByID(zoneIAmIn),
			targetZone = GameController.GetZoneByID(zoneID);

		if (targetZone == null) {
			return false;
		}

		if (GameController.CurGameData.unifyBattlePhase) {
			if (curZone != null && curZone.linkedZones.Contains(zoneID)) {
				GameController.CurGameData.unifiedOrdersRegistry.RegisterOrder
					(RegisteredCmderOrder.OrderType.move, ID, zoneID, GameController.GetFactionByID(ownerFaction).isPlayer);
				return true;
			}
			else return false;

		}
		else {
			return MoveToZone(zoneID);
		}
	}

	/// <summary>
	/// spends all points available to move to the 
	/// </summary>
	/// <param name="zoneID"></param>
	/// <returns></returns>
	public bool MoveToZone(int zoneID) {
		Zone zoneWeWereIn = GameController.GetZoneByID(zoneIAmIn),
			targetZone = GameController.GetZoneByID(zoneID);

		//don't move if the destination doesn't exist or if it's no longer linked to where we are
		if (targetZone == null || !zoneWeWereIn.linkedZones.Contains(zoneID)) return false;

		zoneIAmIn = zoneID;
		pointsToSpend = 0;
		//reset other cmders' positions after departing
		World.TidyZone(zoneWeWereIn);
		TransformTweener.instance.StartTween(MeIn3d.transform, targetZone.MyZoneSpot, true);

		return true;
	}

	/// <summary>
	/// returns the percentage compared to the cmder's MAX amount of troops that would be upgraded if
	/// the cmder trained instead of moving or recruiting
	/// </summary>
	/// <returns></returns>
	public float GetPercentOfTroopsUpgradedIfTrained() {
		if (pointsToSpend <= 0) return 0.0f;

		Zone curZone = GameController.GetZoneByID(zoneIAmIn);

		if (curZone.multTrainingPoints > 0) {
			Faction cmderFac = GameController.GetFactionByID(ownerFaction);
			int totalTrainableTroops = 0;
			int trainableTroops = 0;
			int troopTrainingCostHere = 0;
			int troopIndexInGarrison = -1;
			int fakePointsToSpend = pointsToSpend;
			TroopType curTTBeingTrained = null, curTTUpgradeTo = null;
			for (int i = 0; i < cmderFac.troopLine.Count - 1; i++) { //the last one can't upgrade, so...
				troopIndexInGarrison = IndexOfTroopInContainer(cmderFac.troopLine[i]);
				if (troopIndexInGarrison >= 0) {
					curTTBeingTrained = GameController.GetTroopTypeByID(cmderFac.troopLine[i]);
					curTTUpgradeTo = GameController.GetTroopTypeByID(cmderFac.troopLine[i + 1]);
					troopTrainingCostHere = Mathf.RoundToInt(curTTUpgradeTo.pointCost / curZone.multTrainingPoints);
					if (troopTrainingCostHere == 0) {
						trainableTroops = troopsContained[troopIndexInGarrison].troopAmount;
					}
					else {
						trainableTroops = Mathf.Min(fakePointsToSpend / troopTrainingCostHere,
							troopsContained[troopIndexInGarrison].troopAmount);
					}
					if (trainableTroops > 0) {
						totalTrainableTroops += trainableTroops;
						fakePointsToSpend -= trainableTroops * troopTrainingCostHere;
					}
				}
			}

			//Debug.Log("GetPercentOfTroopsUpgradedIfTrained = " + ((float)totalTrainableTroops / MaxTroopsCommanded));
			return (float)totalTrainableTroops / MaxTroopsCommanded;
		}

		return 0.0f;
	}

	/// <summary>
	/// returns the percentage compared to the cmder's MAX amount of troops that would be added if
	/// the cmder recruited instead of moving or training
	/// </summary>
	/// <returns></returns>
	public float GetPercentOfNewTroopsIfRecruited() {
		if (pointsToSpend <= 0) return 0.0f;

		Zone curZone = GameController.GetZoneByID(zoneIAmIn);

		if (curZone.multRecruitmentPoints > 0) {
			Faction cmderFac = GameController.GetFactionByID(ownerFaction);
			int fakePointsToSpend = pointsToSpend;

			TroopType recruitableTroopType = GetTroopTypeRecruitedWhereIAm(cmderFac);
			if (recruitableTroopType == null) return 0.0f;

			int troopRecruitmentCostHere =
				Mathf.RoundToInt(recruitableTroopType.pointCost / curZone.multRecruitmentPoints);
			int recruitableTroopsAmount = 0;
			//this troop can be so cheap and the zone so good that the troop ends up with cost 0 after rounding
			if (troopRecruitmentCostHere == 0) {
				recruitableTroopsAmount = MaxTroopsCommanded - TotalTroopsContained;
			}
			else {
				recruitableTroopsAmount = Mathf.Min(pointsToSpend / troopRecruitmentCostHere,
					MaxTroopsCommanded - TotalTroopsContained);
			}

			//Debug.Log("GetPercentOfNewTroopsIfRecruited = " + ((float)recruitableTroopsAmount / MaxTroopsCommanded));
			return (float)recruitableTroopsAmount / MaxTroopsCommanded;
		}

		return 0.0f;
	}

	/// <summary>
	/// returns either our faction's base troop type 
	/// or the local merc caravan's type...
	/// or null, if there is no caravan and our faction has no troop types at all
	/// </summary>
	/// <returns></returns>
	public TroopType GetTroopTypeRecruitedWhereIAm(Faction ownerFac = null) {
		if (ownerFac == null) ownerFac = GameController.GetFactionByID(ownerFaction);

		TroopType recruitableTroopType = null;

		MercCaravan localCaravan = GameController.GetMercCaravanInZone(zoneIAmIn);

		if (localCaravan != null) {
			recruitableTroopType = GameController.GetTroopTypeByID(localCaravan.containedTroopType);
		}
		else if (ownerFac.troopLine.Count > 0) {
			recruitableTroopType = GameController.GetTroopTypeByID(ownerFac.troopLine[0]); //maybe this is some kind of special faction that only relies on mercs?
		}

		return recruitableTroopType;
	}

	public static int SortByZoneIAmIn(Commander x, Commander y) {
		return x.zoneIAmIn.CompareTo(y.zoneIAmIn);
	}

}

