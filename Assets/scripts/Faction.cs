﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
	/// <summary>
	/// the faction's exclusive name
	/// </summary>
    public string name;

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
	/// the smaller this value is, the bigger the chances this faction's turn will come before the others'
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
	/// a number added to the value defined at the Rules. 
	/// The bigger this number is, the more commanders this faction will be able to field in comparison with the default
	/// </summary>
	public int extraMaxCommanders = 0;

	/// <summary>
	/// if set to a number that's greater than or equal to 0,
	/// this will restrict the auto training that occurs in full garrisons:
	/// troops won't upgrade past this specified index of the TroopTree list
	/// (an invalid value disables this feature)
	/// </summary>
	public int maxGarrisonedTroopTier = -1;

	/// <summary>
	/// the troops used by this faction, starting by the base troop and ending with the top-tier one... or not, if you want troops to get worse with time
	/// </summary>
	public List<string> troopTree;

	/// <summary>
	/// when a new commander is created, it will have these troops
	/// </summary>
	public List<TroopNumberPair> newCommanderTroops;

}