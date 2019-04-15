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

		//fill allies and enemies containers
		

		//fill diplo options container

		curTargetFacTxt.text = receivingFac.name;
		curTargetFacTxt.color = receivingFac.color;
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
