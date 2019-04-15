using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DiploSendMsgSelectTargetPanel : FactionsRelationsPanel {

	public override void FillEntries() {
		List<Faction> factionList = GameController.instance.curData.factions;
		if(curFactionBeingEdited == null) {
			curFactionBeingEdited = factionList[0];
			curFactionNameTxt.text = curFactionBeingEdited.name;
			curFactionNameTxt.color = curFactionBeingEdited.color;
		}
		for (int i = 0; i < factionList.Count; i++) {
			if(factionList[i] != curFactionBeingEdited) {
				FactionsRelationPanelListEntry newEntry = 
					AddEntry(factionList[i]) as FactionsRelationPanelListEntry;
				newEntry.SetSliderAccordingToRelationWith(curFactionBeingEdited);
			}
		}
	}

}
