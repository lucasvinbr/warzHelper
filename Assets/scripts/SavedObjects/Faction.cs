using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Faction
{

	/// <summary>
	/// the faction's unique ID
	/// </summary>
	public int ID;

	/// <summary>
	/// the faction's exclusive name
	/// </summary>
    public string name;

	/// <summary>
	/// any extra text info the player might want to provide to the faction
	/// </summary>
	public string extraInfo;

	/// <summary>
	/// is this faction controlled by a player? 
	/// This affects options like automatically giving control to the AI to other factions and auto-resolving their battles
	/// </summary>
    public bool isPlayer = false;

	/// <summary>
	/// the faction's color.
	/// Commanders and zones owned by them will be tinted with this color
	/// </summary>
    public Color color;

	/// <summary>
	/// the path to an image file that represents this faction, shown in the battle screen
	/// </summary>
	public string iconPath;

	/// <summary>
	/// the smaller this value is, the bigger the chances this faction's turn will come before the others'.
	/// The order between factions with the same priority is defined when the game starts
	/// </summary>
    public int turnPriority = 0;

	/// <summary>
	/// a multiplier applied on the value defined at the Rules. The bigger this multiplier is, the better for this faction
	/// </summary>
	public float multCommanderPointAwardOnTurnStart = 1;
	/// <summary>
	/// a multiplier applied on the value defined at the Rules. The bigger this multiplier is, the better for this faction
	/// </summary>
	public float multZonePointAwardOnTurnStart = 1;
	/// <summary>
	/// a multiplier applied on the value defined at the Rules. The bigger this multiplier is, the better for this faction
	/// </summary>
	public float multMaxUnitsUnderOneCommander = 1;
	/// <summary>
	/// a multiplier applied on the value defined at the Rules. The bigger this multiplier is, the better for this faction
	/// </summary>
	public float multMaxUnitsInOneGarrison = 1;

	/// <summary>
	/// a multiplier... the bigger the better
	/// </summary>
	public float multBonusCmdersPerZone = 1;

	/// <summary>
	/// a number added to the value defined at the Rules. 
	/// The bigger this number is, the more commanders this faction will be able to field in comparison with the default
	/// </summary>
	public int extraMaxCommanders = 0;


	/// <summary>
	/// if set to a number that's greater than or equal to 0,
	/// this will restrict the auto training that occurs in full garrisons:
	/// troops won't upgrade past this specified index of the TroopTree list
	/// (an invalid value will clamp to the closest valid one)
	/// </summary>
	public int maxGarrisonedTroopTier = 0;

	/// <summary>
	/// the troops used by this faction, starting by the base troop and ending with the top-tier one... or not, if you want troops to get worse with time
	/// </summary>
	public List<int> troopLine;

	public Faction(string name) {
		this.ID = GameController.GetUnusedFactionID();
		this.name = name;
		troopLine = new List<int>();
		color = Color.white;
		while (GameController.GetFactionByName (this.name) != null) {
			this.name = name + " copy";
		}
		GameController.instance.curData.factions.Add(this);
	}

	public Faction() {}

	[XmlIgnore]
	public List<Zone> OwnedZones
	{
		get
		{
			List<Zone> returnedList = new List<Zone>();
			foreach (Zone z in GameController.instance.curData.zones) {
				if (z.ownerFaction == ID) {
					returnedList.Add(z);
				}
			}
			return returnedList;
		}
	}

	[XmlIgnore]
	public List<Commander> OwnedCommanders
	{
		get
		{
			List<Commander> returnedList = new List<Commander>();
			foreach (Commander c in GameController.instance.curData.deployedCommanders) {
				if (c.ownerFaction == ID) {
					returnedList.Add(c);
				}
			}
			return returnedList;
		}
	}

	/// <summary>
	/// calls GetTotalMaxCmders using OwnedZones, so if you've already got the owned zones before,
	/// it's better to call GetTotalMaxCmders directly
	/// </summary>
	[XmlIgnore]
	public int MaxCmders
	{
		get
		{
			return GetTotalMaxCmders(OwnedZones);
		}
	}

	/// <summary>
	/// considers the base max defined in the rules and this faction's extra max... 
	/// aand the bonus per zone from the provided list
	/// </summary>
	public int GetTotalMaxCmders(List<Zone> ourZones) {
		Rules r = GameController.instance.curData.rules;
		return extraMaxCommanders + r.baseMaxCommandersPerFaction +
			Mathf.RoundToInt(r.baseBonusCommandersPerZone * multBonusCmdersPerZone * ourZones.Count);
	}

	public float GetRelationWith(Faction targetFac) {
		if (targetFac == null) return GameFactionRelations.MIN_RELATIONS;

		return GameController.CurGameData.
			factionRelations.GetRelationBetweenFactions(ID, targetFac.ID);
	}

	public GameFactionRelations.FactionStanding GetStandingWith(Faction targetFac) {
		if (targetFac == null) return GameFactionRelations.FactionStanding.enemy;

		return GameController.CurGameData.
			factionRelations.GetStandingBetweenFactions(ID, targetFac.ID);
	}

	public GameFactionRelations.FactionStanding GetStandingWith(int targetFacID) {
		if (targetFacID < 0) return GameFactionRelations.FactionStanding.enemy;

		return GameController.CurGameData.
			factionRelations.GetStandingBetweenFactions(ID, targetFacID);
	}

	public float SetRelationWith(Faction targetFac, float newValue) {
		return GameController.CurGameData.
			factionRelations.SetRelationBetweenFactions(ID, targetFac.ID, newValue);
	}

	/// <summary>
	/// adds relations with the target fac, 
	/// optionally never becoming allies unless an alliance is proposed
	/// </summary>
	/// <param name="targetFac"></param>
	/// <param name="addition"></param>
	/// <returns></returns>
	public float AddRelationWith(Faction targetFac, float addition, bool preventAutoAlly = false) {
		return GameController.CurGameData.
			factionRelations.AddRelationBetweenFactions
				(ID, targetFac.ID, addition, preventAutoAlly);
	}

	/// <summary>
	/// adds relations with the target fac, 
	/// optionally never becoming allies unless an alliance is proposed
	/// </summary>
	/// <param name="targetFacID"></param>
	/// <param name="addition"></param>
	/// <returns></returns>
	public float AddRelationWith(int targetFacID, float addition, bool preventAutoAlly = false) {
		return GameController.CurGameData.
			factionRelations.AddRelationBetweenFactions
				(ID, targetFacID, addition, preventAutoAlly);
	}

	/// <summary>
	/// adds relations with the target facs, 
	/// optionally never becoming allies unless an alliance is proposed
	/// </summary>
	/// <param name="targetFacsIDs"></param>
	/// <param name="addition"></param>
	/// <returns></returns>
	public void AddRelationWith(List<int> targetFacsIDs, float addition, bool preventAutoAlly = false) {
		GameController.CurGameData.
			factionRelations.AddRelationBetweenFactions
				(ID, targetFacsIDs, addition, preventAutoAlly);
	}

	public static int SortByTurnPriority(Faction x, Faction y) {
		int comparison = x.turnPriority.CompareTo(y.turnPriority);
		//if it's equal, randomize it
		if (comparison == 0) comparison = Random.Range(0, 2) == 1 ? -1 : 1;

		return comparison;
	}
}