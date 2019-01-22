using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonesPanel : ListContainerPanel<Zone> {

	public override void FillEntries() {
		List<Zone> zoneList = GameController.instance.curData.zones;
		for (int i = 0; i < zoneList.Count; i++) {
			AddEntry(zoneList[i]);
		}
	}

	/// <summary>
	/// closes the panel and begins the zone placement procedure.
	/// if a spot is selected (placement isnt canceled),
	/// open the edit zone menu for this new zone
	/// </summary>
	public void OnNewZoneBtnClicked() {
		World.BeginNewZonePlacement();
		gameObject.SetActive(false);
	}

	public void EditZoneLinks() {
		gameObject.SetActive(false);
		TemplateModeUI templateUI = GameInterface.instance.templateModeUI as TemplateModeUI;
		templateUI.SetDisplayedLowerHUD(templateUI.zoneLinkingLowerHUD);
		World.BeginZoneLinking(null);
	}
}
