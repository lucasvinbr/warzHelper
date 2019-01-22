using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TroopsPanel : ListContainerPanel<TroopType> {

	public override void FillEntries() {
		List<TroopType> TTList = GameController.instance.curData.troopTypes;
		for (int i = 0; i < TTList.Count; i++) {
			AddEntry(TTList[i]);
		}
	}

	/// <summary>
	/// closes the panel and opens the edit TT panel for a new troop
	/// </summary>
	public void OnNewTroopBtnClicked() {
		TroopType newTT = new TroopType("New Troop");
		GameInterface.instance.EditTroopType(newTT, true);
		gameObject.SetActive(false);
	}
}
