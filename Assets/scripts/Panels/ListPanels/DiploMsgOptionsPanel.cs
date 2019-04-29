using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DiploMsgOptionsPanel: GenericOverlayPanel {

	public GameObject diploOptionBtnPrefab, factionRelatedEntryPrefab;

	public Transform alliesContainer, enemiesContainer, diploOpsContainer;

	public TMP_Text curTargetFacTxt;

	private Faction sendingFac, receivingFac;


	/// <summary>
	/// since we can limit the number of diplomatic messages sent per turn,
	/// we must check if we haven't reached our limit already before opening this panel
	/// </summary>
	/// <param name="sendingFac"></param>
	/// <param name="receivingFac"></param>
	public void OpenIfPossible(Faction sendingFac, Faction receivingFac) {
		//check how many msgs we sent this turn...
		//show warning box instead of opening this if we've already reached the limit

		gameObject.SetActive(true);

		CleanAllContainers();

		this.sendingFac = sendingFac;
		this.receivingFac = receivingFac;

		//fill allies and enemies containers
		FillRelatedFactionsPanel(alliesContainer, 
			GameFactionRelations.GetFactionsWithTargetStandingWithFac(receivingFac,
			GameFactionRelations.FactionStanding.ally));

		FillRelatedFactionsPanel(enemiesContainer,
			GameFactionRelations.GetFactionsWithTargetStandingWithFac(receivingFac,
			GameFactionRelations.FactionStanding.enemy));

		//fill diplo options container

		curTargetFacTxt.text = receivingFac.name;
		curTargetFacTxt.color = receivingFac.color;
	}



	public void FillRelatedFactionsPanel(Transform targetContainer, List<Faction> relatedFacsList) {
		foreach(Faction f in relatedFacsList) {
			GameObject newEntry = Instantiate(factionRelatedEntryPrefab, targetContainer);
			FactionEntryWithNameAndTooltip entryScript = 
				newEntry.GetComponent<FactionEntryWithNameAndTooltip>();
			entryScript.SetNameAndColorToFac(f);
			entryScript.tooltip.text = string.Concat("Standing with ", sendingFac.name, ": ", 
				GameFactionRelations.StandingToNiceName(sendingFac.GetStandingWith(f)));
		}
	}

	public void CleanContainer(Transform container) {
		for (int i = 0; i < container.childCount; i++) {
			Destroy(container.GetChild(i).gameObject);
		}
	}

	public void CleanAllContainers() {
		CleanContainer(alliesContainer);
		CleanContainer(enemiesContainer);
		CleanContainer(diploOpsContainer);
	}
	
}
