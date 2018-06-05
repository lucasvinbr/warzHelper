using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Troop {
	/// <summary>
	/// the troop's unique name.
	/// </summary>
	public string name;

	/// <summary>
	/// this troop's "power" in autocalc battles.
	/// it is affected by a random number, between 1 and autoResolveBattleDieSides
	/// </summary>
	public float autoResolvePower = 1;


	/// <summary>
	/// the point cost in order to recruit, or upgrade to, this unit
	/// </summary>
	public int pointCost = 2;
}

public struct TroopNumberPair {
	public string troopName;
	public int troopAmount;
}

