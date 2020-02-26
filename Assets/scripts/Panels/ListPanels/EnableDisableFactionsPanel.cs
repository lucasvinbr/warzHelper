using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableFactionsPanel : ListContainerPanel<Faction>
{


	public override void FillEntries() {
		List<Faction> factionList = new List<Faction>();
		factionList.AddRange(GameController.instance.curData.factions);
		factionList.AddRange(GameController.CurGameData.disabledFactions);
		for (int i = 0; i < factionList.Count; i++) {
			AddEntry(factionList[i]);
		}
	}

	public void Apply()
	{
		Dictionary<Faction, bool> enabledFactionsDict = new Dictionary<Faction, bool>();
		for (int i = 0; i < listContainer.childCount; i++)
		{
			Transform entry = listContainer.GetChild(i);
			FactionsEnableDisablePanelListEntry entryScript =
				entry.GetComponent<FactionsEnableDisablePanelListEntry>();
			if (entryScript)
			{
				enabledFactionsDict.Add(entryScript.myContent, entryScript.isEnabledToggle.isOn);
			}
		}

		//factions are enabled all at once so that we can share neutral zones as equally as possible
		List<Faction> factionsToEnable = new List<Faction>();
		bool anyFactionRemoved = false;

		GameInfo gameData = GameController.CurGameData;
		
		foreach(var kvp in enabledFactionsDict)
		{
			if (kvp.Value)
			{
				if (gameData.disabledFactions.Contains(kvp.Key))
				{
					factionsToEnable.Add(kvp.Key);
				}
			}
			else
			{
				if (gameData.factions.Contains(kvp.Key))
				{
					GameController.DisableFaction(kvp.Key);
					anyFactionRemoved = true;
				}
			}
		}

		GameController.EnableFactions(factionsToEnable);

		if (anyFactionRemoved)
		{
			//since so many things can go wrong with factions instantly disappearing,
			//we just start a new turn after running this
			GameModeHandler.instance.StopAllPhaseMans();

			GameModeHandler.instance.ScheduleTurnStartAfterPanelsClose(GameModeHandler.TurnPhase.newCmder, true);
		}
		
	}

}
