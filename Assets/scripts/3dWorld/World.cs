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
        Zone newZone = new Zone("New Zone");		
        GameObject newSpot = Instantiate(zonePrefab, point, Quaternion.identity);
        newSpot.transform.parent = zonesContainer;
        newSpot.GetComponent<ZoneSpot>().data = newZone;
        if (autoOpenEditMenu)
        {
            GameInterface.instance.EditZone(newZone, true);
        }
    }

	public ZoneSpot GetZoneSpotByZoneName(string zoneName) {
		Transform theZoneTransform = zonesContainer.Find(zoneName);
		if (theZoneTransform) {
			return theZoneTransform.GetComponent<ZoneSpot>();
		}

		return null;
	}

    public void PlaceZone(Zone targetZone)
    {

    }
}
