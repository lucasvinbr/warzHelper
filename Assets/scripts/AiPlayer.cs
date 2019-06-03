using System.Collections;
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
	public const float MIN_TRAIN_SCORE_THRESHOLD = 0.22f, MAX_TRAIN_SCORE_THRESHOLD = 0.39f;

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
	public const float NEW_CMDER_ZONE_DANGER_INFLUENCE = 0.65f;

	public static void AiNewCmderPhase(Faction curFac, List<Zone> availableZones) {
		if (availableZones.Count == 1) {
			//not much thinking needed for the AI in this case
			World.CreateNewCmderAtZone(availableZones[0], GameModeHandler.instance.curPlayingFaction);
			return;
		}

		Zone topZone = null;
		float topScore = 0;
		float candidateScore = 0;

		//zones with higher recruit factors are better for new cmders
		//also give more score to zones which might be in danger
		foreach (Zone z in availableZones) {
			candidateScore = z.multRecruitmentPoints + GetZoneDangerScore(z, curFac) * NEW_CMDER_ZONE_DANGER_INFLUENCE;
			if (candidateScore >= topScore) {
				topScore = candidateScore;
				topZone = z;
			}
		}

		if (!GameModeHandler.instance.currentTurnIsFast)
			CameraPanner.instance.JumpToSpot(topZone.MyZoneSpot.transform.position);
		World.CreateNewCmderAtZone(topZone, GameModeHandler.instance.curPlayingFaction);

	}

	public static void AiCommandPhase(Faction ourFac, List<Commander> commandableCmders,
		CommandPhaseMan phaseScript) {
		Zone zoneCmderIsIn = null, moveDestZone = null, scoreCheckZone = null,
			fallbackMoveDestZone = null;

		List<Commander> ourCmders = ourFac.OwnedCommanders;

		List<Zone> emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);

		//how close to our max cmders?
		float factionCmderAmountRatio = ourCmders.Count / (float)ourFac.MaxCmders;

		float recruitChance, topMoveScore, scoreCheckScore, trainChance;
		bool hasActed = false;

		for (int i = commandableCmders.Count - 1; i >= 0; i--) {
			zoneCmderIsIn = GameController.GetZoneByID(commandableCmders[i].zoneIAmIn);

			//recruit chance might be more than 100%, but it's ok as it will do something else
			//if it can't recruit
			recruitChance = commandableCmders[i].GetPercentOfNewTroopsIfRecruited() * 1.35f;

			trainChance = commandableCmders[i].GetPercentOfTroopsUpgradedIfTrained();

			moveDestZone = zoneCmderIsIn;
			//if no zone beats this score, we stay
			topMoveScore = GetZoneDangerScore(zoneCmderIsIn, ourFac);

			if (factionCmderAmountRatio < SHOULD_GET_MORE_CMDERS_THRESHOLD &&
				emptyNewCmderZones.Count == 0) {
				//if our territories are full of cmders, make it more likely to move around
				topMoveScore *= factionCmderAmountRatio;
				recruitChance *= factionCmderAmountRatio;
				trainChance *= factionCmderAmountRatio;
			}

			foreach (int nearbyZoneID in zoneCmderIsIn.linkedZones) {
				scoreCheckZone = GameController.GetZoneByID(nearbyZoneID);

				scoreCheckScore = GetZoneMoveScore(scoreCheckZone, ourFac);

				//if we're low on commanders,
				//we should try to be more selective in our attacks...
				//make room for new cmders by stacking existing cmders in a friendly zone and stuff
				if (factionCmderAmountRatio < SHOULD_GET_MORE_CMDERS_THRESHOLD) {
					if (scoreCheckZone.ownerFaction != ourFac.ID) {
						scoreCheckScore *= factionCmderAmountRatio;
					}
					else {
						if (emptyNewCmderZones.Count == 1 &&
							emptyNewCmderZones[0] == scoreCheckZone) {
							//better not move to this spot, it's the only place for a new cmder
							scoreCheckScore *= factionCmderAmountRatio;
						}
						else if (emptyNewCmderZones.Count == 0) {
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

			if (topMoveScore <= 0.0f) {
				//we're "safe-landlocked"!
				//we should find a path to danger then
				if (fallbackMoveDestZone == null) {
					//get a good destination zone for the faction
					fallbackMoveDestZone = FindInterestingZone(ourFac);
					Debug.Log("Faction " + ourFac.name + " got " + fallbackMoveDestZone.name +
						" as fallback moveDest zone");
				}

				moveDestZone = GetNextZoneInPathToZone(fallbackMoveDestZone, zoneCmderIsIn);
				topMoveScore = MAX_MOVE_SCORE_THRESHOLD;
				Debug.Log("when trying to go from " + zoneCmderIsIn.name + " to " +
					fallbackMoveDestZone.name + ", cmder got " + moveDestZone.name + " as path");

			}

			hasActed = false;

			//Debug.Log("rec chance: " + recruitChance + " topMove: " + topMoveScore);
			if (trainChance > GetRandomTrainScoreThreshold() && trainChance > recruitChance &&
				trainChance > topMoveScore) {
				hasActed = commandableCmders[i].TrainTroops();
			}

			if (!hasActed && recruitChance > topMoveScore) {
				hasActed = commandableCmders[i].RecruitTroops();
			}

			if (!hasActed && topMoveScore > GetRandomMoveScoreThreshold()) {
				if (moveDestZone.ID != zoneCmderIsIn.ID) {
					phaseScript.MoveCommander(commandableCmders[i].MeIn3d, moveDestZone.MyZoneSpot, false);
					//refresh the empty zone list if we moved;
					//that way, our other cmders may not "feel" the same need to move as this one did
					emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);
					hasActed = true;
				}
			}

			if (!hasActed) {
				//if we decided to do nothing else, train troops
				hasActed = commandableCmders[i].TrainTroops();

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

		potentialAlliedPower = GameController.GetTotalAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(targetZone, ourFac));

		potentialEnemyPower = GameController.GetTotalAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(targetZone, ourFac));


		foreach (int nearbyZoneID in targetZone.linkedZones) {
			nearbyZone = GameController.GetZoneByID(nearbyZoneID);
			if (nearbyZone.ownerFaction == ourFac.ID) {
				//nearby allies have less meaning than nearby enemies
				//because they may not arrive in time to help...
				//or choose to not help at all
				potentialAlliedPower += GameController.GetTotalAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(nearbyZone, ourFac, true)) *
					GetRandomNearbyTroopInfluenceFactor() / 1.5f;
			}
			else {
				potentialEnemyPower += GameController.GetTotalAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(nearbyZone, ourFac, true)) *
					GetRandomNearbyTroopInfluenceFactor();
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
		if (targetZone.CanBeTakenBy(ourFac)) {
			finalScore = 1 - finalScore;
		}

		//we should add score if friendly commanders are already there,
		//in order to make bigger armies
		//(allied cmders shouldn't be considered here because they can't attack at the same time)
		foreach (Commander cmd in GameController.GetCommandersOfFactionInZone(targetZone, ourFac)) {
			finalScore *= FRIENDLY_CMDER_PRESENCE_MULT;
		}

		return finalScore;
	}

	/// <summary>
	/// returns one of our zones that has the largest danger level.
	/// looks for, in order:
	/// -dangerous owned zone
	/// -dangerous allied zone
	/// -neutral zone
	/// -any dangerous zone
	/// -any zone
	/// </summary>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static Zone FindInterestingZone(Faction ourFac) {
		List<Zone> browsedList = ourFac.OwnedZones;
		float foundScore = 0.0f;
		Zone foundZone = GetMostInterestingZoneInList(browsedList, ourFac, out foundScore);

		if (foundScore > 0.0f) {
			return foundZone;
		}

		browsedList = GameController.GetZonesOwnedByAlliesOfFac(ourFac);
		foundZone = GetMostEndangeredZoneInList(browsedList, out foundScore);

		if (foundScore > 0.0f) {
			return foundZone;
		}

		browsedList = GameController.GetNeutralZones();
		if (browsedList.Count > 0) {
			return browsedList[Random.Range(0, browsedList.Count)];
		}

		browsedList = GameController.instance.curData.zones;
		foundZone = GetMostInterestingZoneInList(browsedList, ourFac, out foundScore);

		if (foundScore > 0.0f) {
			return foundZone;
		}

		//if we get to this point... maybe the game should have ended already
		return browsedList[Random.Range(0, browsedList.Count)];
	}

	public static Zone GetMostInterestingZoneInList(List<Zone> zList, Faction ourFac,
		out float bestScore) {
		float topScore = 0.0f, candidateScore = 0.0f;
		Zone topZone = null;
		foreach (Zone z in zList) {
			candidateScore = GetZoneMoveScore(z, ourFac);
			if (topScore < candidateScore) {
				topScore = candidateScore;
				topZone = z;
			}
		}

		bestScore = topScore;

		return topZone;
	}

	/// <summary>
	/// gets the zone with the biggest danger score according to the zone's owner faction
	/// </summary>
	/// <param name="zList"></param>
	/// <param name="bestScore"></param>
	/// <returns></returns>
	public static Zone GetMostEndangeredZoneInList(List<Zone> zList, out float bestScore) {
		float topScore = 0.0f, candidateScore = 0.0f;
		Zone topZone = null;
		foreach (Zone z in zList) {
			candidateScore = GetZoneDangerScore(z, GameController.GetFactionByID(z.ownerFaction));
			if (topScore < candidateScore) {
				topScore = candidateScore;
				topZone = z;
			}
		}

		bestScore = topScore;

		return topZone;
	}

	public static Zone GetNextZoneInPathToZone(Zone targetZone, Zone startZone) {
		if (targetZone.ID == startZone.ID) return startZone;

		List<KeyValuePair<int, float>> frontier = new List<KeyValuePair<int, float>>();
		Dictionary<int, int> cameFrom = new Dictionary<int, int>();

		List<int> foundPath = new List<int>();

		Zone curScannedZone = startZone;
		Zone costCalcZone = null;

		frontier.Add(new KeyValuePair<int, float>(startZone.ID, 0));

		KeyValuePair<int, float> newEntry;
		int frontierInsertIndex = 0;

		while (frontier[0].Key != targetZone.ID && frontier.Count > 0) {
			curScannedZone = GameController.GetZoneByID(frontier[0].Key);
			frontier.RemoveAt(0);
			foreach (int zID in curScannedZone.linkedZones) {
				if (!cameFrom.ContainsKey(zID)) {
					costCalcZone = GameController.GetZoneByID(zID);
					cameFrom.Add(zID, curScannedZone.ID);
					newEntry = new KeyValuePair<int, float>
						(zID, Vector2.Distance(costCalcZone.coords, targetZone.coords));

					//add entry with priority according to the distance to the dest
					for(frontierInsertIndex = 0; frontierInsertIndex < frontier.Count; frontierInsertIndex++) {
						if(newEntry.Value <= frontier[frontierInsertIndex].Value) {
							break;
						}
					}

					frontier.Insert(frontierInsertIndex, newEntry);
				}
				
			}
		}

		if(frontier.Count > 0) {
			//rebuild path...
			frontierInsertIndex = targetZone.ID;
			foundPath.Insert(0, frontierInsertIndex);

			while(frontierInsertIndex != startZone.ID) {
				foundPath.Insert(0, cameFrom[frontierInsertIndex]);
				frontierInsertIndex = cameFrom[frontierInsertIndex];
			}
		}else {
			//we failed to find a path?!
			//just stand still then
			Debug.LogWarning("[AIPlayer] Couldn't find a path to zone " + targetZone.name);
			return startZone;
		}


		Debug.Log("pathfinding... going to " + GameController.GetZoneByID(foundPath[1]).name);
		return GameController.GetZoneByID(foundPath[1]);
	}

	

	#region random factor getters
	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomNearbyTroopInfluenceFactor() {
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

	#endregion
}
