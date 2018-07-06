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
}
