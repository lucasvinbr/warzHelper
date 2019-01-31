﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rules {
	/// <summary>
	/// the maximum multiplier applied to the sum of all troops' autoResolvePower during autocalc battles.
	/// it might help a losing faction win some battles, but may make troop training lose relevance if too high
	/// </summary>
	public float autoResolveBattleDieSides = 6;

	/// <summary>
	/// when a battle is won, the victor gets bonus points according to the autocalc power of 
	/// the defeated troops. This value multiplies the received points 
	/// (use 0 to disable post-battle point awards)
	/// </summary>
	public float battleVictorPointAwardFactor = 0.4f;

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


	public const string NO_FACTION_NAME = "No Faction";

	/// <summary>
	/// the time the commander 3d takes to reach their destination when moving.
	/// Merely visual, the data involved in the movement is changed immediately
	/// </summary>
	public const float CMDER3D_ANIM_MOVE_DURATION = 0.65f;

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
