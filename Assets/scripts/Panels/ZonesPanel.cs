using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonesPanel : ListContainerPanel<Zone> {

    protected override void OnEnable()
    {
        ClearList();
        List<Zone> zoneList = GameController.instance.curGameData.zones;
        for (int i = 0; i < zoneList.Count; i++)
        {
            AddEntry(zoneList[i]);
        }
    }
}
