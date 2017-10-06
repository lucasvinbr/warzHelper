using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    public Transform ground;
    public Transform zonesContainer;

    public GameObject zonePrefab;

    public static World instance;

    public ZonePlacer zonePlacerScript;

    void Awake()
    {
        instance = this;
    }

    public void ToggleWorldDisplay(bool active)
    {
        ground.gameObject.SetActive(active);
        zonesContainer.gameObject.SetActive(active);
    }

    public void CreateNewZoneAtPoint(Vector3 point, bool autoOpenEditMenu = true)
    {
        Zone newZone = new Zone();
        GameObject newSpot = Instantiate(zonePrefab, point, Quaternion.identity);
        newSpot.transform.parent = zonesContainer;
        newSpot.GetComponent<ZoneSpot>().data = newZone;
        if (autoOpenEditMenu)
        {
            GameInterface.instance.EditZone(newZone);
        }
    }

    public void CreateZone(Zone targetZone)
    {

    }
}
