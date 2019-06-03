using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Contains a list of non-repeating faction pairs and relation values.
/// lower values should mean hostility while higher ones, friendship
/// </summary>
[System.Serializable]
public class GameFactionRelations
{
	[System.Serializable]
	public class FactionRelation {
		public int[] relatedFacs = new int[2];
		public float relationValue = 0.0f;

		public FactionRelation() { }

		public FactionRelation(Faction fac1, Faction fac2, float relationValue = 0.0f) {
			relatedFacs[0] = fac1.ID;
			relatedFacs[1] = fac2.ID;
			this.relationValue = relationValue;
		}
	}

	/// <summary>
	/// this prevents relations to change according to ingame actions, like taking zones.
	/// it also stops AI from attempting to make alliances etc
	/// </summary>
	public bool lockRelations = false;

	/// <summary>
	/// this makes the game end if all remaining factions are allied to each other
	/// </summary>
	public bool alliedVictory = true;


	public List<FactionRelation> relations = new List<FactionRelation>();

	/// <summary>
	/// factions will be enemies if their relation value goes below this
	/// </summary>
	public const float CONSIDER_ENEMY_THRESHOLD = 0.0f;

	/// <summary>
	/// factions will be allies if their relation value goes above this
	/// </summary>
	public const float CONSIDER_ALLY_THRESHOLD = 1.0f;

	public const float MAX_RELATIONS = 2.0f, MIN_RELATIONS = -1.0f;

	public enum FactionStanding {
		enemy,
		neutral,
		ally
	}

	public GameFactionRelations() { }

	public void SetDefaultRelationsBetweenAllFactions() {
		List<Faction> factions = GameController.instance.curData.factions;
		for(int i = 0; i < factions.Count - 1; i++) {
			for(int j = i + 1; j < factions.Count; j++) {
				relations.Add(new FactionRelation(factions[i], factions[j]));
			}
		}
	}

	public float GetRelationBetweenFactions(Faction fac1, Faction fac2) {
		foreach(FactionRelation rel in relations) {
			if(rel.relatedFacs[0] == fac1.ID || rel.relatedFacs[1] == fac1.ID) {
				if(rel.relatedFacs[0] == fac2.ID || rel.relatedFacs[1] == fac2.ID) {
					return rel.relationValue;
				}
			}
		}

		return 0;
	}

	public float GetRelationBetweenFactions(int facID1, int facID2) {
		if(facID1 == facID2) {
			Debug.LogWarning("(FactionRelations) Attempted to get relation between faction and itself (probably wasn't meant to happen)");
		}
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					return rel.relationValue;
				}
			}
		}

		return 0;
	}

	public FactionStanding GetStandingBetweenFactions(int facID1, int facID2) {
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					return RelationValueToStanding(rel.relationValue);
				}
			}
		}

		return FactionStanding.neutral;
	}

	public float AddRelationBetweenFactions(int facID1, int facID2, float addition,
		bool preventBecomingAllies = false, bool notifyIfFactionStandingChanged = false,
		bool announceRelationChange = false) {
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					float relationsBeforeChange = rel.relationValue;
					rel.relationValue = 
						Mathf.Clamp(rel.relationValue + addition, MIN_RELATIONS, preventBecomingAllies ? CONSIDER_ALLY_THRESHOLD : MAX_RELATIONS);

					if (announceRelationChange) {
						LoggerAnnounceRelationChange(facID1, facID2, rel.relationValue, 
							relationsBeforeChange);
					}

					if(notifyIfFactionStandingChanged && 
						RelationValueToStanding(relationsBeforeChange) != 
						RelationValueToStanding(rel.relationValue)) {
						LoggerAnnounceStandingChange(facID1, facID2, RelationValueToStanding(rel.relationValue));
					}

					return rel.relationValue;
				}
			}
		}

		return 0;
	}

	/// <summary>
	/// adds relation between fac1 and all the targetFacs
	/// </summary>
	/// <param name="facID1"></param>
	/// <param name="targetFacs"></param>
	/// <param name="addition"></param>
	/// <param name="preventBecomingAllies"></param>
	/// <param name="notifyIfFactionStandingChanged"></param>
	/// <param name="announceRelationChange"></param>
	/// <returns></returns>
	public void AddRelationBetweenFactions(int facID1, List<int> targetFacs, float addition,
		bool preventBecomingAllies = false, bool notifyIfFactionStandingChanged = false,
		bool announceRelationChange = false) {

		foreach (int targetFac in targetFacs) {
			AddRelationBetweenFactions(facID1, targetFac, addition, preventBecomingAllies,
				notifyIfFactionStandingChanged, announceRelationChange);
		}

	}


	public float SetRelationBetweenFactions(int facID1, int facID2, float newValue) {
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					rel.relationValue =	Mathf.Clamp(newValue, MIN_RELATIONS, MAX_RELATIONS);
					return rel.relationValue;
				}
			}
		}

		return 0;
	}

	public void RemoveAllRelationEntriesWithFaction(int facID) {
		for(int i = relations.Count - 1; i >= 0; i--) {
			if(relations[i].relatedFacs[0] == facID || relations[i].relatedFacs[1] == facID) {
				relations.RemoveAt(i);
			}
		}
	}


	public void LoggerAnnounceRelationChange(int facID1, int facID2, float newValue, float oldValue) {
		if (newValue == oldValue) return; //yeah, no reporting if nothing changed
		string announcement = string.Format("{0}'s relation with {1} has {2} to {3}",
			GameController.GetFactionByID(facID1).name, GameController.GetFactionByID(facID2).name,
			newValue > oldValue ? "increased" : "deteriorated",
			newValue.ToString("0.00", CultureInfo.InvariantCulture));

		LoggerBox.instance.WriteText(announcement, 
			newValue > oldValue ? GameInterface.instance.positiveUIColor :
				GameInterface.instance.negativeUIColor);
	}


	public void LoggerAnnounceStandingChange(int facID1, int facID2, FactionStanding newStanding) {
		string announcement = string.Format("{0} and {1} have become ", 
			GameController.GetFactionByID(facID1).name, GameController.GetFactionByID(facID2).name);
		switch (newStanding) {
			case FactionStanding.ally:
				announcement += "allies!";
				break;
			case FactionStanding.neutral:
				announcement += "neutral towards each other!";
				break;
			case FactionStanding.enemy:
				announcement += "enemies!";
				break;
		}

		LoggerBox.instance.WriteText(announcement);
	}


	public static FactionStanding RelationValueToStanding(float relationValue) {
		if (relationValue < CONSIDER_ENEMY_THRESHOLD) {
			return FactionStanding.enemy;
		}
		else if (relationValue < CONSIDER_ALLY_THRESHOLD) {
			return FactionStanding.neutral;
		}
		else return FactionStanding.ally;
	}

	/// <summary>
	/// in case of a future localization feature...
	/// </summary>
	/// <param name="standing"></param>
	/// <returns></returns>
	public static string StandingToNiceName(FactionStanding standing) {
		switch (standing) {
			case FactionStanding.ally:
				return "Ally";
			case FactionStanding.enemy:
				return "Enemy";
			default:
				return "Neutral";
		}
	}

	/// <summary>
	/// shorthand for StandingToNiceName(RelationValueToStanding(relValue))
	/// </summary>
	/// <param name="relValue"></param>
	/// <returns></returns>
	public static string RelationValueToNiceName(float relValue) {
		return StandingToNiceName(RelationValueToStanding(relValue));
	}


	public static List<Faction> GetFactionsWithTargetStandingWithFac(Faction fac, FactionStanding targetStanding) {
		List<Faction> facList = new List<Faction>(GameController.instance.curData.factions);
		facList.Remove(fac);
		for (int i = facList.Count - 1; i >= 0; i--) {
			if (fac.GetStandingWith(facList[i]) != targetStanding) {
				facList.RemoveAt(i);
			}
		}

		return facList;
	}

}
