using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaterialsHandler : MonoBehaviour {

	/// <summary>
	/// materials with the respective factions' colors (factions are picked by ID)
	/// </summary>
	public Dictionary<int, Material> factionMaterialsDict = new Dictionary<int, Material>();

	public Material baseFactionMaterial, neutralZoneMaterial;


	/// <summary>
	/// for each faction, updates (or creates) their colored material
	/// </summary>
	public void ReBakeFactionColorsDict() {
		foreach (Faction f in GameController.instance.curData.factions) {
			if (!factionMaterialsDict.ContainsKey(f.ID)) {
				factionMaterialsDict.Add(f.ID, Instantiate(baseFactionMaterial));
			}

			factionMaterialsDict[f.ID].color = f.color;
		}
	}

	/// <summary>
	/// attempts to clear all entries and destroy all materials in the dict, making it ready for another game
	/// </summary>
	public void PurgeFactionColorsDict() {
		foreach(KeyValuePair<int,Material> kvp in factionMaterialsDict) {
			Destroy(kvp.Value);
		}

		factionMaterialsDict.Clear();
	}
}
