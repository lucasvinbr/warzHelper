using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TroopType {

	/// <summary>
	/// the troop's unique ID
	/// </summary>
	public int ID;

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


	/// <summary>
	/// any extra info you'd like to store about this troop type
	/// </summary>
	public string extraInfo;

	public TroopType(string name) {
		this.ID = GameController.GetUnusedTroopTypeID();
		this.name = name;
		while (GameController.GetTroopTypeByName(name) != null) {
			this.name = name + " copy";
		}
		GameController.instance.curData.troopTypes.Add(this);
		GameController.instance.LastRelevantTType = this;
		GameInterface.troopDDownsAreStale = true;
	}

	public TroopType(TroopType referenceTT) {
		this.ID = GameController.GetUnusedTroopTypeID();
		this.name = referenceTT.name + "_new";
		while (GameController.GetTroopTypeByName(name) != null) {
			this.name = name + " copy";
		}

		this.extraInfo = referenceTT.extraInfo;
		this.autoResolvePower = referenceTT.autoResolvePower;
		this.pointCost = referenceTT.pointCost;
		GameController.instance.curData.troopTypes.Add(this);
		GameController.instance.LastRelevantTType = this;
		GameInterface.troopDDownsAreStale = true;
	}

	public TroopType() {

	}

	
}

/// <summary>
/// represents the population of a specific troop type in a commander's army or a zone's garrison
/// </summary>
public struct TroopNumberPair {
	public int troopTypeID;
	public int troopAmount;

	public TroopNumberPair(int TTID, int amount) {
		this.troopTypeID = TTID;
		this.troopAmount = amount;
	}

	public static int CompareTroopNumberPairsByAutocalcPower(TroopNumberPair x, TroopNumberPair y) {
		return GameController.GetTroopTypeByID(x.troopTypeID).
			autoResolvePower.CompareTo(GameController.GetTroopTypeByID(y.troopTypeID).autoResolvePower);
	}
}

[System.Serializable]
public class SerializableTroopList {
	public List<SerializedTroop> troops;

	public SerializableTroopList() {
		troops = new List<SerializedTroop>();
	}
}

[System.Serializable]
public class SerializedTroop {
	public string name;
	public int amount;

	public SerializedTroop(string name, int amount) {
		this.name = name;
		this.amount = amount;
	}
}



