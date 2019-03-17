using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateModeUI : ModeUI {

	public GenericOverlayPanel tempSaveOptionsPanel;

	public GameObject mainLowerHUD, zoneLinkingLowerHUD;

	public override void Cleanup() {
		if (World.instance.zoneLinkerScript.enabled) {
			World.instance.zoneLinkerScript.DoneLinking();
		}
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.ToggleWorldDisplay(false);
		World.instance.zoneEditOnClickScript.enabled = false;
		World.instance.garrDescOnHoverScript.enabled = false;
		tempSaveOptionsPanel.gameObject.SetActive(false);
		GameController.instance.facMatsHandler.PurgeFactionColorsDict();
		TexLoader.PurgeTexDict();
	}

	public override void Initialize()
    {
		GameController.instance.facMatsHandler.ReBakeFactionColorsDict();
		World.CleanZonesContainer();
		World.CleanZoneLinks();
		World.ToggleWorldDisplay(true);
		World.SetupAllZonesFromData();
		World.LinkAllZonesFromData();
		World.SetupBoardDetails();
		SetDisplayedLowerHUD(mainLowerHUD);
		World.instance.zoneEditOnClickScript.enabled = true;
		World.instance.garrDescOnHoverScript.enabled = true;
	}


}
