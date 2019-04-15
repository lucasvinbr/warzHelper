﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiPlayer {

	/// <summary>
	/// how big is the influence of troops that aren't right on the spot, but can get to it quickly, in the AI's
	/// decisions?
	/// </summary>
	public const float MIN_NEARBY_TROOPS_INFLUENCE = 0.2f, MAX_NEARBY_TROOPS_INFLUENCE = 0.4f;
	/// <summary>
	/// how big should the "move to zone" score be before we actually decide to move?
	/// </summary>
	public const float MIN_MOVE_SCORE_THRESHOLD = 0.1f, MAX_MOVE_SCORE_THRESHOLD = 0.35f;

	/// <summary>
	/// how much should the percentage of upgradeable-in-one-turn troops be
	/// before we start considering training as a better option than recruiting/moving?
	/// </summary>
	public const float MIN_TRAIN_SCORE_THRESHOLD = 0.25f, MAX_TRAIN_SCORE_THRESHOLD = 0.35f;

	/// <summary>
	/// how much should a situation where there is no room for new commanders, 
	/// but more commanders can still be placed,
	/// make a commander more likely to leave a zone/not go to the only empty zone left?
	/// the resulting value gets closer to this the further we are from the max cmder count
	/// </summary>
	public const float MAX_MAKE_ROOM_SCORE_BONUS = 0.9f;

	/// <summary>
	/// how much should the presence of a friendly commander in a potential move target zone
	/// make it more likely for more cmders to go to that zone as well?
	/// </summary>
	public const float FRIENDLY_CMDER_PRESENCE_MULT = 1.15f;

	/// <summary>
	/// if the ratio of spawned cmders / max cmders is below this value,
	/// the AI should try to make room for new cmders
	/// </summary>
	public const float SHOULD_GET_MORE_CMDERS_THRESHOLD = 0.75f;


	/// <summary>
	/// how much should a zone's danger score be taken into account
	/// when choosing where to spawn a new cmder?
	/// </summary>
	public const float NEW_CMDER_ZONE_DANGER_INFLUENCE = 0.75f;

	public static void AiNewCmderPhase(Faction curFac, List<Zone> availableZones) {
		if(availableZones.Count == 1) {
			//not much thinking needed for the AI in this case
			World.CreateNewCmderAtZone(availableZones[0], GameModeHandler.instance.curPlayingFaction);
			return;
		}

		Dictionary<Zone, float> zoneFavorScores = new Dictionary<Zone, float>();


		//zones with higher recruit factors are better for new cmders
		//also give more score to zones which might be in danger
		foreach (Zone z in availableZones) {
			zoneFavorScores.Add(z, z.multRecruitmentPoints +
				GetZoneDangerScore(z, curFac) * NEW_CMDER_ZONE_DANGER_INFLUENCE);
		}

		Zone topZone = null;
		float topScore = 0;

		foreach(KeyValuePair<Zone, float> kvp in zoneFavorScores) {
			if(kvp.Value > topScore) {
				topScore = kvp.Value;
				topZone = kvp.Key;
			}
		}

		if(!GameModeHandler.instance.currentTurnIsFast)
			CameraPanner.instance.JumpToSpot(topZone.MyZoneSpot.transform.position);
		World.CreateNewCmderAtZone(topZone, GameModeHandler.instance.curPlayingFaction);

	}

	public static void AiCommandPhase(Faction ourFac, List<Commander> commandableCmders,
		CommandPhaseMan phaseScript) {
		Zone zoneCmderIsIn = null, moveDestZone = null, scoreCheckZone = null;

		List<Commander> ourCmders = ourFac.OwnedCommanders;

		List<Zone> emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);

		//how close to our max cmders?
		float factionCmderAmountRatio = ourCmders.Count / (float) ourFac.MaxCmders;

		float recruitChance, topMoveScore, scoreCheckScore, trainChance;
		bool hasActed = false;

		for (int i = commandableCmders.Count - 1; i >= 0; i--) {
			zoneCmderIsIn = GameController.GetZoneByID(commandableCmders[i].zoneIAmIn);

			//recruit chance might be more than 100%, but it's ok as it will do something else
			//if it can't recruit
			recruitChance = (1.3f - ((float) commandableCmders[i].TotalTroopsContained /
				commandableCmders[i].MaxTroopsCommanded)) * (zoneCmderIsIn.multRecruitmentPoints);

			trainChance = 
				GetPercentOfTroopsUpgradedIfTrained(commandableCmders[i], ourFac, zoneCmderIsIn);

			moveDestZone = zoneCmderIsIn;
			//if no zone beats this score, we stay
			topMoveScore = GetZoneMoveScore(zoneCmderIsIn, ourFac);

			if(factionCmderAmountRatio < SHOULD_GET_MORE_CMDERS_THRESHOLD &&
				emptyNewCmderZones.Count == 0) {
				//if our territories are full of cmders, make it more likely to move around
				topMoveScore *= factionCmderAmountRatio;
				recruitChance *= factionCmderAmountRatio;
				trainChance *= factionCmderAmountRatio;
			}

			foreach(int nearbyZoneID in zoneCmderIsIn.linkedZones) {
				scoreCheckZone = GameController.GetZoneByID(nearbyZoneID);

				scoreCheckScore = GetZoneMoveScore(scoreCheckZone, ourFac);

				//if we're low on commanders,
				//we should try to be more selective in our attacks...
				//make room for new cmders by stacking existing cmders in a friendly zone and stuff
				if(factionCmderAmountRatio < SHOULD_GET_MORE_CMDERS_THRESHOLD) {
					if (scoreCheckZone.ownerFaction != ourFac.ID) {
						scoreCheckScore *= factionCmderAmountRatio;
					}else {
						if (emptyNewCmderZones.Count == 1 &&
							emptyNewCmderZones[0] == scoreCheckZone) {
							//better not move to this spot, it's the only place for a new cmder
							scoreCheckScore *= factionCmderAmountRatio;
						}
						else if(emptyNewCmderZones.Count == 0) {
							//make it more likely to move to this spot then
							scoreCheckScore += Mathf.Lerp(0.0f, MAX_MAKE_ROOM_SCORE_BONUS, (1 - factionCmderAmountRatio));
						}
					}
				} 

				if (scoreCheckScore > topMoveScore) {
					topMoveScore = scoreCheckScore;
					moveDestZone = scoreCheckZone;
				}
			}

			hasActed = false;

			//Debug.Log("rec chance: " + recruitChance + " topMove: " + topMoveScore);
			if(trainChance > GetRandomTrainScoreThreshold() && trainChance > recruitChance &&
				trainChance > topMoveScore) {
				commandableCmders[i].TrainTroops(out hasActed);
			}

			if(!hasActed && recruitChance > topMoveScore) {
				hasActed = commandableCmders[i].RecruitTroops();
			}

			if (!hasActed && topMoveScore > GetRandomMoveScoreThreshold()) {
				if(moveDestZone.ID != zoneCmderIsIn.ID) {
					phaseScript.MoveCommander(commandableCmders[i].MeIn3d, moveDestZone.MyZoneSpot, false);
					//refresh the empty zone list if we moved;
					//that way, our other cmders may not "feel" the same need to move as this one did
					emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);
					hasActed = true;
				}
			}

			if(!hasActed) {
				//if we decided to do nothing else, train troops
				commandableCmders[i].TrainTroops(out hasActed);

				//try recruiting and moving again if we can't train!
				if (!hasActed) {
					hasActed = commandableCmders[i].RecruitTroops();
				}

				if (!hasActed) {
					if (moveDestZone.ID != zoneCmderIsIn.ID) {
						phaseScript.MoveCommander(commandableCmders[i].MeIn3d, moveDestZone.MyZoneSpot, false);
						emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);
						hasActed = true;
					}
					//if we can't recruit nor train and staying is more interesting than moving...
					//do nothing
				}
			}



			commandableCmders.RemoveAt(i);
		}
	}

	#region getters
	/// <summary>
	/// gets forces in and around the zone and compares them.
	/// The closer to 1, the more enemies are nearby compared to allied forces
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static float GetZoneDangerScore(Zone targetZone, Faction ourFac) {
		Zone nearbyZone = null;
		float potentialAlliedPower = 0, potentialEnemyPower = 0;

		potentialAlliedPower = GameController.GetArmyAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(targetZone, ourFac));

		potentialEnemyPower = GameController.GetArmyAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(targetZone, ourFac));
			

		foreach (int nearbyZoneID in targetZone.linkedZones) {
			nearbyZone = GameController.GetZoneByID(nearbyZoneID);
			if(nearbyZone.ownerFaction == ourFac.ID) {
				potentialAlliedPower += GameController.GetArmyAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(nearbyZone, ourFac, true)) *
					GetRandomTroopInfluenceFactor();
			}else {
				potentialEnemyPower += GameController.GetArmyAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(nearbyZone, ourFac, true)) *
					GetRandomTroopInfluenceFactor();
			}
		}
		//Debug.Log("pot enemy: " + potentialEnemyPower + " pot ally: " + potentialAlliedPower);

		return Mathf.Max(0, (potentialEnemyPower - potentialAlliedPower) /
			Mathf.Max(1, potentialEnemyPower));
	}

	/// <summary>
	/// uses the danger score and some extra info to get a "should I move there" score.
	/// This favors zones with friendly cmders to help the AI make big armies
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static float GetZoneMoveScore(Zone targetZone, Faction ourFac) {
		float finalScore = GetZoneDangerScore(targetZone, ourFac);

		//if the zone is hostile, the less danger the better
		if(targetZone.CanBeTakenBy(ourFac)) {
			finalScore = 1 - finalScore;
		}

		//we should add score if friendly commanders are already there,
		//in order to make bigger armies
		//(allied cmders shouldn't be considered here because they can't attack at the same time)
		foreach(Commander cmd in GameController.GetCommandersOfFactionInZone(targetZone, ourFac)) {
			finalScore *= FRIENDLY_CMDER_PRESENCE_MULT;
		}

		return finalScore;
	}

	/// <summary>
	/// returns the percentage compared to the cmder's MAX amount of troops that would be upgraded if
	/// the cmder trained instead of moving or recruiting
	/// </summary>
	/// <returns></returns>
	public static float GetPercentOfTroopsUpgradedIfTrained(Commander cmder, Faction cmderFac, Zone zoneCmderIsIn) {
		if (cmder.pointsToSpend > 0 && zoneCmderIsIn.multTrainingPoints > 0) {
			int totalTrainableTroops = 0;
			int trainableTroops = 0;
			int troopTrainingCostHere = 0;
			int troopIndexInGarrison = -1;
			int fakePointsToSpend = cmder.pointsToSpend;
			TroopType curTTBeingTrained = null, curTTUpgradeTo = null;
			for (int i = 0; i < cmderFac.troopLine.Count - 1; i++) { //the last one can't upgrade, so...
				troopIndexInGarrison = cmder.IndexOfTroopInContainer(cmderFac.troopLine[i]);
				if (troopIndexInGarrison >= 0) {
					curTTBeingTrained = GameController.GetTroopTypeByID(cmderFac.troopLine[i]);
					curTTUpgradeTo = GameController.GetTroopTypeByID(cmderFac.troopLine[i + 1]);
					troopTrainingCostHere = Mathf.RoundToInt(curTTUpgradeTo.pointCost / zoneCmderIsIn.multTrainingPoints);
					if (troopTrainingCostHere == 0) {
						trainableTroops = cmder.troopsContained[troopIndexInGarrison].troopAmount;
					}
					else {
						trainableTroops = Mathf.Min(fakePointsToSpend / troopTrainingCostHere,
							cmder.troopsContained[troopIndexInGarrison].troopAmount);
					}
					if (trainableTroops > 0) {
						totalTrainableTroops += trainableTroops;
						fakePointsToSpend -= trainableTroops * troopTrainingCostHere;
					}
				}
			}

			return (float) totalTrainableTroops / cmder.MaxTroopsCommanded;
		}

		return 0.0f;
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomTroopInfluenceFactor() {
		return Random.Range(MIN_NEARBY_TROOPS_INFLUENCE, MAX_NEARBY_TROOPS_INFLUENCE);
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomMoveScoreThreshold() {
		return Random.Range(MIN_MOVE_SCORE_THRESHOLD, MAX_MOVE_SCORE_THRESHOLD);
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomTrainScoreThreshold() {
		return Random.Range(MIN_TRAIN_SCORE_THRESHOLD, MAX_TRAIN_SCORE_THRESHOLD);
	}


	#endregion
}
