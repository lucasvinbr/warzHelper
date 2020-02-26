using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;
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
		public float relationValue = INITIAL_REL;

		public FactionRelation() { }

		public FactionRelation(Faction fac1, Faction fac2, float relationValue = INITIAL_REL) {
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

	public enum RelationChangeReportLevel {
		none,
		onlyPlayerFaction,
		allFactions
	}

	public RelationChangeReportLevel relationChangeReportLevel = RelationChangeReportLevel.onlyPlayerFaction;

	public RelationChangeReportLevel standingChangeReportLevel = RelationChangeReportLevel.allFactions;

	[XmlIgnore]
	/// <summary>
	/// whenever a faction relation crosses one of the relation change limits,
	/// it's added to this dict, so that, in the end of the turn, we can
	/// report this change only once (or not report it at all if it changed and then was restored
	/// to what it was when the turn began).
	/// we store the relation itself and the relation value before the first change in the turn
	/// </summary>
	public Dictionary<FactionRelation, float> reportedRelationChanges = new Dictionary<FactionRelation, float>();

	/// <summary>
	/// factions will be enemies if their relation value goes below this
	/// </summary>
	public const float CONSIDER_ENEMY_THRESHOLD = 0.0f;

	/// <summary>
	/// the relation between all facs when the game starts
	/// </summary>
	public const float INITIAL_REL = 0.35f;

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

	/// <summary>
	/// returns true if the number of entries in the relation list is correct according to the number of factions in the game
	/// </summary>
	/// <returns></returns>
	public bool CheckRelationListIntegrity() {
		int facsCount = GameController.instance.curData.factions.Count;

		return relations.Count == ((facsCount - 1) * facsCount / 2);
	}

	/// <summary>
	/// goes through the factions list checking if relations between every pair of facs exist, adding one if it doesn't
	/// </summary>
	public void AddAnyMissingFacEntries() {
		List<Faction> factions = GameController.instance.curData.factions;

		bool relationExists = false;

		for (int i = 0; i < factions.Count - 1; i++) {
			for (int j = i + 1; j < factions.Count; j++) {
				GetRelationBetweenFactions(factions[i], factions[j], out relationExists);

				if (!relationExists) {
					relations.Add(new FactionRelation(factions[i], factions[j]));
				}
			}
		}
	}

	public float GetRelationBetweenFactions(Faction fac1, Faction fac2, out bool relationEntryExists) {
		foreach(FactionRelation rel in relations) {
			if(rel.relatedFacs[0] == fac1.ID || rel.relatedFacs[1] == fac1.ID) {
				if(rel.relatedFacs[0] == fac2.ID || rel.relatedFacs[1] == fac2.ID) {
					relationEntryExists = true;
					return rel.relationValue;
				}
			}
		}

		relationEntryExists = false;
		return INITIAL_REL;
	}

	public float GetRelationBetweenFactions(int facID1, int facID2) {
		if(facID1 == facID2) {
			Debug.LogWarning("(FactionRelations) Attempted to get relation between faction and itself (probably wasn't meant to happen)");
			return 1;
		}
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					return rel.relationValue;
				}
			}
		}

		return INITIAL_REL;
	}

	public FactionStanding GetStandingBetweenFactions(int facID1, int facID2) {
		if (facID1 == facID2) return FactionStanding.ally;

		return RelationValueToStanding(GetRelationBetweenFactions(facID1, facID2));
	}

	/// <summary>
	/// returns the new relation value between the two facs
	/// </summary>
	/// <param name="facID1"></param>
	/// <param name="facID2"></param>
	/// <param name="addition"></param>
	/// <param name="preventBecomingAllies"></param>
	/// <returns></returns>
	public float AddRelationBetweenFactions(int facID1, int facID2, float addition,
		bool preventBecomingAllies = false) {
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					float relationsBeforeChange = rel.relationValue;
					rel.relationValue = 
						Mathf.Clamp(rel.relationValue + addition, MIN_RELATIONS, preventBecomingAllies ? CONSIDER_ALLY_THRESHOLD : MAX_RELATIONS);

					//register this relation for later announcement of changes
					if (!reportedRelationChanges.ContainsKey(rel)) {
						reportedRelationChanges.Add(rel, relationsBeforeChange);
					}

					//if a new alliance was made here, everyone should react!
					if(RelationValueToStanding(rel.relationValue) == FactionStanding.ally &&
						RelationValueToStanding(relationsBeforeChange) != FactionStanding.ally) {
						DiplomacyManager.GlobalReactToAlliance(rel.relatedFacs[0], rel.relatedFacs[1]);
					}

					return rel.relationValue;
				}
			}
		}

		return INITIAL_REL;
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
		bool preventBecomingAllies = false) {

		foreach (int targetFac in targetFacs) {
			AddRelationBetweenFactions(facID1, targetFac, addition, preventBecomingAllies);
		}

	}

	/// <summary>
	/// returns the new relation value between the facs, or INITIAL_REL if something's wrong.
	/// this does not trigger Faction Reactions to alliances caused by this change
	/// </summary>
	/// <param name="facID1"></param>
	/// <param name="facID2"></param>
	/// <param name="newValue"></param>
	/// <returns></returns>
	public float SetRelationBetweenFactions(int facID1, int facID2, float newValue) {
		foreach (FactionRelation rel in relations) {
			if (rel.relatedFacs[0] == facID1 || rel.relatedFacs[1] == facID1) {
				if (rel.relatedFacs[0] == facID2 || rel.relatedFacs[1] == facID2) {
					rel.relationValue =	Mathf.Clamp(newValue, MIN_RELATIONS, MAX_RELATIONS);
					return rel.relationValue;
				}
			}
		}

		return INITIAL_REL;
	}

	public void RemoveAllRelationEntriesWithFaction(int facID) {
		for(int i = relations.Count - 1; i >= 0; i--) {
			if(relations[i].relatedFacs[0] == facID || relations[i].relatedFacs[1] == facID) {
				relations.RemoveAt(i);
			}
		}
	}


	public void AnnounceAllRelationChanges() {
		foreach(KeyValuePair<FactionRelation, float> kvp in reportedRelationChanges) {
			if(kvp.Key.relationValue != kvp.Value) {
				if(relationChangeReportLevel != RelationChangeReportLevel.none) {
					if(relationChangeReportLevel == RelationChangeReportLevel.allFactions ||
						(GameController.GetFactionByID(kvp.Key.relatedFacs[0]).isPlayer ||
						GameController.GetFactionByID(kvp.Key.relatedFacs[1]).isPlayer)) {
						LoggerAnnounceRelationChange
							(kvp.Key.relatedFacs[0], kvp.Key.relatedFacs[1], kvp.Key.relationValue, kvp.Value);
					}
				}

				if(standingChangeReportLevel != RelationChangeReportLevel.none) {
					if(RelationValueToStanding(kvp.Value) != RelationValueToStanding(kvp.Key.relationValue)) {
						if (standingChangeReportLevel == RelationChangeReportLevel.allFactions ||
						(GameController.GetFactionByID(kvp.Key.relatedFacs[0]).isPlayer ||
						GameController.GetFactionByID(kvp.Key.relatedFacs[1]).isPlayer)) {
							LoggerAnnounceStandingChange
								(kvp.Key.relatedFacs[0], kvp.Key.relatedFacs[1],
								RelationValueToStanding(kvp.Key.relationValue));
						}
					}
				}
			}
		}

		reportedRelationChanges.Clear();
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
