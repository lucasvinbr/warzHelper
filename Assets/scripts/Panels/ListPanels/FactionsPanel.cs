using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionsPanel : ListContainerPanel<Faction>
{


	public override void FillEntries() {
		List<Faction> factionList = GameController.instance.curData.factions;
		for (int i = 0; i < factionList.Count; i++) {
			AddEntry(factionList[i]);
		}
	}

	/// <summary>
	/// closes the panel and opens the edit faction panel for a new faction
	/// </summary>
	public void OnNewFactionBtnClicked() {
		Faction newFaction = new Faction("New Faction");
		GameInterface.instance.EditFaction(newFaction, true);
		gameObject.SetActive(false);
	}
}
