using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rules {
	/// <summary>
	/// the maximum multiplier applied to the sum of all troops' autoResolvePower during autocalc battles.
	/// it might help a losing faction win some battles, but may make troop training lose relevance if too high
	/// </summary>
	public float autoResolveBattleDieSides = 6f;

	/// <summary>
	/// in an autocalc battle, samples are taken from both the involved sides and their power is compared.
	/// this defines the base sample size used.
	/// The side with more troops gets a proportionally bigger sample
	/// </summary>
	public int autoResolveBattleSampleSize = 10;

	/// <summary>
	/// if one of the sides involved in an autocalc battle has an army size above this,
	/// it won't make a difference when calculating army sample size proportions.
	/// Useful for preventing huge steamrollers that don't take any losses
	/// </summary>
	public int autoResolveBattleMaxArmyForProportion = 400;

	/// <summary>
	/// in an autocalc battle, the auto-resolve power of the involved troops is multiplied by a random
	/// value between 1 and autoResolveBattleDieSides. The side with the greatest resulting value wins,
	/// and then the winner's resulting value is deducted from the loser's power, so that
	/// some troops are lost.
	/// This value defines if, and how much, the winner should also take damages according to the loser's
	/// power result (a value greater than 1 will make the winner take extra damage compared to the loser!)
	/// </summary>
	public float autoResolveWinnerDamageMultiplier = 0.75f;

	/// <summary>
	/// when a battle is won, the victor gets bonus points according to the point cost of 
	/// the defeated troops. This value multiplies the received points 
	/// (use 0 to disable post-battle point awards)
	/// </summary>
	public float battleWinnerPointAwardFactor = 0.4f;

	/// <summary>
	/// the chance of a mercenary caravan staying in a zone for one turn instead of moving to another one
	/// </summary>
	public float caravanStayChance = 0.25f;

	/// <summary>
	/// the points each commander receives at the beginning of their faction's turn
	/// </summary>
	public int baseCommanderPointAwardOnTurnStart = 50;
	/// <summary>
	/// the points each zone receives at the beginning of their owner faction's turn.
	/// zones automatically use these points to recruit and upgrade their garrison
	/// </summary>
	public int baseZonePointAwardOnTurnStart = 50;
	/// <summary>
	/// the maximum number of troops one commander can mobilize. He won't be able to recruit if this value is reached
	/// </summary>
	public int baseMaxUnitsUnderOneCommander = 60;
	/// <summary>
	/// the maximum number of troops that can garrison in one zone. 
	/// Zones will automatically spend all points in training after this limit is reached,
	/// and will spend in recruiting before it is reached
	/// </summary>
	public int baseMaxUnitsInOneGarrison = 90;
	/// <summary>
	/// the maximum number of commanders one faction can have at the same time, 
	/// before bonus commanders per zone is applied. 
	/// Having a high value here may favor a "turtle" strategy and rare but decisive attacks
	/// </summary>
	public int baseMaxCommandersPerFaction = 5;

	/// <summary>
	/// this value multiplied by the amount of zones owned by a faction 
	/// equals the amount of bonus commanders the faction gets.
	/// a high value can make a faction "snowball" with lots of cmders compared to others
	/// </summary>
	public float baseBonusCommandersPerZone = 0.35f;


	public const string NO_FACTION_NAME = "No Faction";

	/// <summary>
	/// the board's x and y dimensions. Affects camera boundaries.
	/// doesn't affect zone placement, but may make some zones inacessible to the player!
	/// </summary>
	public Vector2 boardDimensions = new Vector2(100, 100);

	/// <summary>
	/// the path to a "ground" texture used in this board. Can be a map or something depicting a landscape, for example
	/// </summary>
	public string boardTexturePath = "";

	//empty constructor to enable xml serialization
	public Rules() {}
}
