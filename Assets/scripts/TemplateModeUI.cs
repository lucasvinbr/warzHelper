using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplateModeUI : ModeUI {

	public override void ClearUI() {
		World.CleanZonesContainer();
		World.ToggleWorldDisplay(false);
	}

	public override void ShowInitialUI()
    {
		World.CleanZonesContainer();
		World.ToggleWorldDisplay(true);
		World.SetupAllZonesFromData();
		World.LinkAllZonesFromData();
    }
}
