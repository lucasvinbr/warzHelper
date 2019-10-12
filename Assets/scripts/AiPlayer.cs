using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiPlayer {

	/// <summary>
	/// how big is the influence of troops that aren't right on the spot, but can get to it quickly, in the AI's
	/// decisions?
	/// </summary>
	public const float MIN_NEARBY_TROOPS_INFLUENCE = 0.52f, MAX_NEARBY_TROOPS_INFLUENCE = 0.88f;

	/// <summary>
	/// how big should the "move to zone" score be before we actually decide to move?
	/// </summary>
	public const float MIN_MOVE_SCORE_THRESHOLD = 0.3f, MAX_MOVE_SCORE_THRESHOLD = 0.37f;

	/// <summary>
	/// multiplies the base recruitment chance to make recruiting more important than training
	/// </summary>
	public const float RECRUIT_CHANCE_ENCOURAGEMENT = 1.35f;

	/// <summary>
	/// how much should the percentage of upgradeable-in-one-turn troops be
	/// before we start considering training as a better option than recruiting/moving?
	/// </summary>
	public const float MIN_TRAIN_SCORE_THRESHOLD = 0.22f, MAX_TRAIN_SCORE_THRESHOLD = 0.35f;

	/// <summary>
	/// if the enemies have the allies' power times this value in a zone, the AI should be disencouraged to move or stay there
	/// </summary>
	public const float MIN_FRIENDLY_ZONE_TOO_DANGEROUS_THRESHOLD = 2.5f, MAX_FRIENDLY_ZONE_TOO_DANGEROUS_THRESHOLD = 3.5f;

	/// <summary>
	/// divisor applied to move scores to keep training and recruiting relevant
	/// </summary>
	public const float DANGER_SCORE_DIVISOR = 1.55f;

	/// <summary>
	/// the AI is "discouraged" to attack factions that are neutral to them by these multipliers:
	/// they multiply the move score of those neutral zones, reducing them if these values are below 1.
	/// the smaller the value, the more "discouragement" is applied
	/// </summary>
	public const float MIN_DISENCOURAGE_ATK_ON_NEUTRAL_FAC_MULT = 0.35f, MAX_DISENCOURAGE_ATK_ON_NEUTRAL_FAC_MULT = 0.6f;

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
	public const float FRIENDLY_CMDER_PRESENCE_MULT = 1.08f;

	/// <summary>
	/// if the ratio of spawned cmders / max cmders is below this value,
	/// the AI should try to make room for new cmders
	/// </summary>
	public const float SHOULD_GET_MORE_CMDERS_THRESHOLD = 0.75f;

	/// <summary>
	/// the AI will avoid traveling to further locations if at least one of the nearby zones reaches this score
	/// </summary>
	public const float SHOULD_GO_DEFENSIVE_THRESHOLD = 0.43f;

	/// <summary>
	/// how much should a zone's danger score be taken into account
	/// when choosing where to spawn a new cmder?
	/// </summary>
	public const float NEW_CMDER_ZONE_DANGER_INFLUENCE = 0.35f;

	public static void AiNewCmderPhase(Faction curFac, List<Zone> availableZones) {
		if (availableZones.Count == 1) {
			//not much thinking needed for the AI in this case
			NewCmderPhaseMan.OrderPlaceNewCmder(availableZones[0].ID, GameModeHandler.instance.curPlayingFaction);
			return;
		}

		Zone topZone = null;
		float topScore = 0;
		float candidateScore = 0;


		//zones with higher recruit factors are better for new cmders
		//also give more score to zones which might be in danger
		foreach (Zone z in availableZones) {
			candidateScore = GetZoneDangerScore(z, curFac);
			//too much danger is bad!
			if (candidateScore >= GetRandomFriendlyTooDangerousThreshold()) candidateScore = GetRandomMoveScoreThreshold();
			candidateScore = z.multRecruitmentPoints + candidateScore * NEW_CMDER_ZONE_DANGER_INFLUENCE;
			if (candidateScore >= topScore) {
				topScore = candidateScore;
				topZone = z;
			}
		}

		if (!GameModeHandler.instance.currentTurnIsFast)
			CameraPanner.instance.JumpToSpot(topZone.MyZoneSpot.transform.position);
		NewCmderPhaseMan.OrderPlaceNewCmder(topZone.ID, GameModeHandler.instance.curPlayingFaction);

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
		bool plansOnStayingInCurZone = false;

		for (int i = commandableCmders.Count - 1; i >= 0; i--) {
			zoneCmderIsIn = GameController.GetZoneByID(commandableCmders[i].zoneIAmIn);

			//recruit chance might be more than 100%, but it's ok as it will do something else
			//if it can't recruit
			recruitChance = Mathf.Max(commandableCmders[i].GetPercentOfNewTroopsIfRecruited(),
				Mathf.Min(commandableCmders[i].GetPercentOfNewTroopsIfRecruitedComparedToCurrent(), 1.0f)) * 
				RECRUIT_CHANCE_ENCOURAGEMENT;

			trainChance = commandableCmders[i].GetPercentOfTroopsUpgradedIfTrained();

			moveDestZone = zoneCmderIsIn;
			//if no zone beats this score, we stay
			topMoveScore = GetZoneDangerScore(zoneCmderIsIn, ourFac);

			//but too much danger is bad and should reduce our chances of staying
			if (topMoveScore >= GetRandomFriendlyTooDangerousThreshold()) topMoveScore = GetRandomMoveScoreThreshold();

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
							scoreCheckScore *= factionCmderAmountRatio * factionCmderAmountRatio;
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

			if (topMoveScore <= SHOULD_GO_DEFENSIVE_THRESHOLD) {
				//Debug.Log("safe-landlocked! move score is " + topMoveScore);
				//we're "safe-landlocked"!
				//we should find a path to danger then
				if (fallbackMoveDestZone == null) {
					//get a good destination zone for the faction
					fallbackMoveDestZone = FindInterestingZone(ourFac);
					//Debug.Log("Faction " + ourFac.name + " got " + fallbackMoveDestZone.name +
					//	" as fallback moveDest zone");
				}

				moveDestZone = GetNextZoneInPathToZone(fallbackMoveDestZone, zoneCmderIsIn);
				topMoveScore = MAX_MOVE_SCORE_THRESHOLD;
				//Debug.Log("when trying to go from " + zoneCmderIsIn.name + " to " +
				//	fallbackMoveDestZone.name + ", cmder got " + moveDestZone.name + " as path");

			}

			hasActed = false;
			plansOnStayingInCurZone = moveDestZone.ID == zoneCmderIsIn.ID;


			if (trainChance > recruitChance &&
				(plansOnStayingInCurZone || trainChance > topMoveScore)) {
				hasActed = commandableCmders[i].OrderTrainTroops();
			}

			if (!hasActed && (plansOnStayingInCurZone || recruitChance > topMoveScore)) {
				hasActed = commandableCmders[i].OrderRecruitTroops();
			}

			if (!hasActed && topMoveScore > GetRandomMoveScoreThreshold()) {
				if (!plansOnStayingInCurZone) {
					phaseScript.MoveCommander(commandableCmders[i], moveDestZone.MyZoneSpot, false);
					//refresh the empty zone list if we moved;
					//that way, our other cmders may not "feel" the same need to move as this one did
					emptyNewCmderZones = GameController.GetZonesForNewCmdersOfFaction(ourFac);
					hasActed = true;
				}
			}

			if (!hasActed) {
				//if we decided to do nothing else, train troops
				hasActed = commandableCmders[i].OrderTrainTroops();

				//try recruiting again if we can't train!
				if (!hasActed) {
					hasActed = commandableCmders[i].OrderRecruitTroops();
				}

				//do nothing then
				//Debug.Log("commander from " + ourFac.name + " at " + zoneCmderIsIn.name +
				//	" decided to do nothing. Move score: " + topMoveScore + ", rec score: " + recruitChance +
				//	", train score: " + trainChance);
			}



			commandableCmders.RemoveAt(i);
		}
	}

	#region getters
	/// <summary>
	/// gets forces in and around the zone and compares them.
	/// 1 = enemies and allies have equal power, closer to 0 = fewer enemies, bigger than 1 = allies at disadvantage.
	/// value meanings are flipped if getSafetyScoreInstead is true
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static float GetZoneDangerScore(Zone targetZone, Faction ourFac, bool getSafetyScoreInstead = false) {
		Zone nearbyZone = null;
		float potentialAlliedPower = 0, potentialEnemyPower = 0;

		potentialAlliedPower = GameController.GetTotalAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(targetZone, ourFac, useProjectedAllyPositions: true));

		potentialEnemyPower = GameController.GetTotalAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(targetZone, ourFac));


		foreach (int nearbyZoneID in targetZone.linkedZones) {
			nearbyZone = GameController.GetZoneByID(nearbyZoneID);
			if (nearbyZone.ownerFaction == ourFac.ID || 
				ourFac.GetStandingWith(nearbyZone.ownerFaction) == GameFactionRelations.FactionStanding.ally) {
				//nearby allies have less meaning than nearby enemies
				//because they may not arrive in time to help...
				//or choose to not help at all
				potentialAlliedPower += GameController.GetTotalAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneFromFactionAndAllies(nearbyZone, ourFac, true)) *
					GetRandomNearbyTroopInfluenceFactor();
			}
			else {
				potentialEnemyPower += GameController.GetTotalAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneNotAlliedToFaction(nearbyZone, ourFac, true)) *
					GetRandomNearbyTroopInfluenceFactor();
			}
		}
		//Debug.Log("pot enemy: " + potentialEnemyPower + " pot ally: " + potentialAlliedPower);

		return getSafetyScoreInstead ? potentialAlliedPower / Mathf.Max(1, potentialEnemyPower) : potentialEnemyPower / Mathf.Max(1, potentialAlliedPower);
	}

	/// <summary>
	/// uses the danger score and some extra info to get a "should I move there" score.
	/// This favors zones with friendly cmders to help the AI make big armies
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static float GetZoneMoveScore(Zone targetZone, Faction ourFac) {
		float finalScore;

		//if the zone is hostile, the less danger the better
		if (targetZone.CanBeTakenBy(ourFac)) {
			finalScore = GetZoneDangerScore(targetZone, ourFac, true);

			//discourage attacks vs. neutral factions to prevent us from getting a lot of enemies too fast
			if(targetZone.ownerFaction >= 0 && 
				ourFac.GetStandingWith(targetZone.ownerFaction) == GameFactionRelations.FactionStanding.neutral) {
				finalScore *= GetRandomAtkOnNeutralFactionDiscouragement();
			}
		}else {
			finalScore = GetZoneDangerScore(targetZone, ourFac);

			//if an allied zone is TOO dangerous, maybe we shouldn't go there
			if(finalScore >= GetRandomFriendlyTooDangerousThreshold()) {
				finalScore = GetRandomMoveScoreThreshold();
			}
		}

		//we should add score if friendly commanders are already there,
		//in order to make bigger armies
		foreach (Commander cmd in GameController.GetCommandersOfFactionAndAlliesInZone(targetZone, ourFac, true)) {
			finalScore *= FRIENDLY_CMDER_PRESENCE_MULT;
		}

		//we divide by the SCORE DIVISOR to avoid getting too high scores compared to recruit and train
		finalScore /= DANGER_SCORE_DIVISOR;

		return finalScore;
	}

	/// <summary>
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


		//Debug.Log("pathfinding... going to " + GameController.GetZoneByID(foundPath[1]).name);
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

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomAtkOnNeutralFactionDiscouragement() {
		return Random.Range(MIN_DISENCOURAGE_ATK_ON_NEUTRAL_FAC_MULT, MAX_DISENCOURAGE_ATK_ON_NEUTRAL_FAC_MULT);
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomFriendlyTooDangerousThreshold() {
		return Random.Range(MIN_FRIENDLY_ZONE_TOO_DANGEROUS_THRESHOLD, MAX_FRIENDLY_ZONE_TOO_DANGEROUS_THRESHOLD);
	}

	#endregion

	#endregion
}
