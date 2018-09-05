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

	public void OnNewFactionBtnClicked() {
		//opens the edit faction menu with a new faction in it
		GameInterface.instance.EditFaction(new Faction(), true);
		//TODO decide if we should auto create a troop type for this faction if no troop types exist
	}
}
