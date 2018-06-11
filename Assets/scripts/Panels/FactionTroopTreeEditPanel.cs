using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionTroopTreeEditPanel : ListContainerPanel<Troop> {

	public Text maxGarrTroopLvlText;

	public GameObject TroopTreeEntryPrefab;

	public Transform maxGarrTroopLvlDelimiter;

	public void AddTroopTier() {

	}

	public void SelectTierEntry() {

	}

	public void EditTierEntry() {

	}

	public void UpdateTreeValues() {

	}

	public void IncrementMaxGarrTroopLvl() {

	}

	public void DecrementMaxGarrTroopLvl() {

	}

	public List<Troop> BakeIntoTroopTree() {
		Debug.Log("return troop list according to contents of panel");
		return new List<Troop>();
	}
}
