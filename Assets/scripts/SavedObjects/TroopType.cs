using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TroopType {
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

	public TroopType(string name) {
		this.name = name;
		while (GameController.GetTroopTypeByName(name) != null) {
			this.name = name + " copy";
		}

	}

	public TroopType() {

	}
}

/// <summary>
/// represents the population of a specific troop type in a commander's army or a zone's garrison
/// </summary>
public struct TroopNumberPair {
	public string troopName;
	public int troopAmount;
}



