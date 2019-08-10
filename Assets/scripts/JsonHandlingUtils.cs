using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
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

	public static TroopNumberPair SerializedTroopToTroopNumberPair(SerializedTroop serialized) {
		TroopType foundType = GameController.GetTroopTypeByName(serialized.name);

		if(foundType != null) {
			return new TroopNumberPair(foundType.ID, serialized.amount);
		}

		return default(TroopNumberPair);
	}

	public static string ToJsonWithExtraVariable(object targetObj, string extraVarName, string extraVarValue) {
		string returnedJson = JsonUtility.ToJson(targetObj);
		returnedJson = returnedJson.Insert(returnedJson.Length - 1, string.Concat(",\"", extraVarName, "\":", extraVarValue));
		return returnedJson;
	}

	public static SerializedTroop[] JsonToSerializedTroopArray(string jsonString) {

		try {
			DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SerializedTroop[]));

			MemoryStream jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));

			SerializedTroop[] readArray = (SerializedTroop[])serializer.ReadObject(jsonStream);

			return readArray;

		}catch(Exception e) {
			Debug.LogWarning("[JSON to SerializedTroop array] json read failed: " + e.Message);
			return null;
		}
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
	[DataMember]
	public string name;

	[DataMember]
	public int amount;

	public SerializedTroop(string name, int amount) {
		this.name = name;
		this.amount = amount;
	}
}
