using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonHandlingUtils
{

	/// <summary>
	/// converts the troop number pairs to their serializable equivalents, using troop names instead of IDs,
	/// optionally splitting entries that have too many troops (if the limit is 0, the default, or below 0,
	/// there is no splitting limit).
	/// should be used in order to make JSONs
	/// </summary>
	/// <param name="troopList"></param>
	/// <returns></returns>
	public static List<SerializedTroop> TroopListToSerializableTroopList(List<TroopNumberPair> troopList, int splitLargeTroopEntriesLimit = 0) {
		List<SerializedTroop> sTroops = new List<SerializedTroop>();

		foreach (TroopNumberPair tnp in troopList) {
			if (splitLargeTroopEntriesLimit > 0 && tnp.troopAmount > splitLargeTroopEntriesLimit) {
				int exportedAmount = 0;

				while (exportedAmount < tnp.troopAmount) {
					sTroops.Add(new SerializedTroop
						(GameController.GetTroopTypeByID(tnp.troopTypeID).name,
						Mathf.Min(splitLargeTroopEntriesLimit, tnp.troopAmount - exportedAmount)));
					exportedAmount += splitLargeTroopEntriesLimit;
				}
			}
			else {
				sTroops.Add(new SerializedTroop
				(GameController.GetTroopTypeByID(tnp.troopTypeID).name, tnp.troopAmount));
			}

		}

		return sTroops;
	}


}

[System.Serializable]
public class SerializableTroopListObj {
	public List<SerializedTroop> troops;

	public SerializableTroopListObj() {
		troops = new List<SerializedTroop>();
	}

	public SerializableTroopListObj(List<SerializedTroop> troops) {
		this.troops = troops;
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
