using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class FactionsRelationsPanel : FactionsPanel
{

	public Faction curFactionBeingEdited;
	public TMP_Text curFactionNameTxt;

	public override void OnEnable() {
		curFactionBeingEdited = GameModeHandler.instance.curPlayingFaction;

		base.OnEnable();
	}

	public override void FillEntries() {
		List<Faction> factionList = GameController.instance.curData.factions;
		if(curFactionBeingEdited == null) {
			curFactionBeingEdited = factionList[0];
		}
		curFactionNameTxt.text = curFactionBeingEdited.name;
		curFactionNameTxt.color = curFactionBeingEdited.color;
		for (int i = 0; i < factionList.Count; i++) {
			if(factionList[i] != curFactionBeingEdited) {
				FactionsRelationPanelListEntry newEntry = 
					AddEntry(factionList[i]) as FactionsRelationPanelListEntry;
				newEntry.SetSliderAccordingToRelationWith(curFactionBeingEdited);
			}
		}
	}

	/// <summary>
	/// changes the faction of which we're viewing relations.
	/// Can go to the "next" or "previous" faction
	/// </summary>
	/// <param name="nextFaction"></param>
	public void ChangeCurFaction(bool nextFaction) {
		List<Faction> factionList = GameController.instance.curData.factions;
		int curIndex = factionList.IndexOf(curFactionBeingEdited);

		if (nextFaction) {
			curIndex++;
			if(curIndex >= factionList.Count) {
				curIndex = 0;
			}
		}else {
			curIndex--;
			if(curIndex < 0) {
				curIndex = factionList.Count - 1;
			}
		}

		curFactionBeingEdited = factionList[curIndex];

		RefillList();
	}

	
}
