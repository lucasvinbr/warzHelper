using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonesPanel : ListContainerPanel<Zone> {

    public override void OnEnable()
    {
        ClearList();
        List<Zone> zoneList = GameController.instance.curData.zones;
        for (int i = 0; i < zoneList.Count; i++)
        {
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
}
