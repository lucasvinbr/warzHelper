using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules {
	/// <summary>
	/// each commander can only make this many actions each turn
	/// </summary>
    public int maxCommanderMovesPerTurn = 1;
	/// <summary>
	/// the maximum multiplier applied to the sum of all troops' autoResolvePower during autocalc battles
	/// it might help a losing faction win some battles, but may make troop training lose relevance if too high
	/// </summary>
	public int autoResolveBattleDieSides = 6;

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
	/// the maximum number of commanders one faction can have at the same time. 
	/// Having a high value here may favor a "turtle" strategy and rare but decisive attacks
	/// </summary>
	public int baseMaxCommandersPerFaction = 5;

	//empty constructor to enable xml serialization
	public Rules() {}
}
