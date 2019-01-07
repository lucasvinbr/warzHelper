using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AiPlayer {

	/// <summary>
	/// how big is the influence of troops that aren't right on the spot, but can get to it quickly, in the AI's
	/// decisions?
	/// </summary>
	public const float minNearbyTroopsInfluenceFactor = 0.3f, maxNearbyTroopsInfluenceFactor = 0.4f;
	/// <summary>
	/// how big should the "move to zone" score be before we actually decide to move?
	/// </summary>
	public const float minMoveScoreThreshold = 0.25f, maxMoveScoreThreshold = 0.5f;

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
				GetZoneDangerScore(z, curFac) * 2);
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
		float recruitChance, topMoveScore, scoreCheckScore;
		bool hasActed = false;
		for (int i = commandableCmders.Count - 1; i >= 0; i--) {
			zoneCmderIsIn = GameController.GetZoneByID(commandableCmders[i].zoneIAmIn);

			recruitChance = (1.0f - ((float) commandableCmders[i].TotalTroopsContained /
				commandableCmders[i].MaxTroopsCommanded)) * zoneCmderIsIn.multRecruitmentPoints;

			moveDestZone = zoneCmderIsIn;
			//if no zone beats this score, we stay
			topMoveScore = GetZoneDangerScore(zoneCmderIsIn, ourFac);

			foreach(int nearbyZoneID in zoneCmderIsIn.linkedZones) {
				scoreCheckZone = GameController.GetZoneByID(nearbyZoneID);
				scoreCheckScore = GetZoneDangerScore(scoreCheckZone, ourFac);
				if (scoreCheckZone.ownerFaction != ourFac.ID) {
					//less danger, better if it's not our zone
					scoreCheckScore = 1 - scoreCheckScore;
				}

				if(scoreCheckScore > topMoveScore) {
					topMoveScore = scoreCheckScore;
					moveDestZone = scoreCheckZone;
				}
			}

			hasActed = false;

			Debug.Log("rec chance: " + recruitChance + " topMove: " + topMoveScore);

			if(recruitChance > topMoveScore) {
				hasActed = commandableCmders[i].RecruitTroops();
			}

			if (!hasActed && topMoveScore > GetRandomMoveScoreThreshold()) {
				if(moveDestZone.ID != zoneCmderIsIn.ID) {
					phaseScript.MoveCommander(commandableCmders[i].MeIn3d, moveDestZone.MyZoneSpot, false);
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
						hasActed = true;
					}
					//if we can't recruit nor train and staying is more interesting than moving...
					//do nothing
				}
			}



			commandableCmders.RemoveAt(i);
		}
	}

	/// <summary>
	/// gets forces in and around the zone and compares them.
	/// The closer to 1, the more enemies are nearby
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="ourFac"></param>
	/// <returns></returns>
	public static float GetZoneDangerScore(Zone targetZone, Faction ourFac) {
		Zone nearbyZone = null;
		float potentialAlliedPower = 0, potentialEnemyPower = 0;

		potentialAlliedPower = GameController.GetArmyAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneFromFaction(targetZone, ourFac));

		potentialEnemyPower = GameController.GetArmyAutocalcPowerFromTroopList
		(GameController.GetCombinedTroopsInZoneNotFromFaction(targetZone, ourFac));
			

		foreach (int nearbyZoneID in targetZone.linkedZones) {
			nearbyZone = GameController.GetZoneByID(nearbyZoneID);
			if(nearbyZone.ownerFaction == ourFac.ID) {
				potentialAlliedPower += GameController.GetArmyAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneFromFaction(nearbyZone, ourFac, true)) *
					GetRandomTroopInfluenceFactor();
			}else {
				potentialEnemyPower += GameController.GetArmyAutocalcPowerFromTroopList
					(GameController.GetCombinedTroopsInZoneNotFromFaction(nearbyZone, ourFac, true)) *
					GetRandomTroopInfluenceFactor();
			}
		}
		Debug.Log("pot enemy: " + potentialEnemyPower + " pot ally: " + potentialAlliedPower);

		return Mathf.Max(0, (potentialEnemyPower - potentialAlliedPower) /
			Mathf.Max(1, potentialEnemyPower));
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomTroopInfluenceFactor() {
		return Random.Range(minNearbyTroopsInfluenceFactor, maxNearbyTroopsInfluenceFactor);
	}

	/// <summary>
	/// gets a factor between the min and max
	/// </summary>
	/// <returns></returns>
	public static float GetRandomMoveScoreThreshold() {
		return Random.Range(minMoveScoreThreshold, maxMoveScoreThreshold);
	}
}
