using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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


	public static void SetGroundSizeAccordingToRules() {
		Vector2 boardDim = GameController.instance.curData.rules.boardDimensions;
		instance.ground.localScale = new Vector3(boardDim.x, boardDim.y, 1);
	}

    public static void ToggleWorldDisplay(bool active)
    {
        instance.ground.gameObject.SetActive(active);
		instance.zonesContainer.gameObject.SetActive(active);
    }

	public static void CleanZonesContainer() {
		for(int i = 0; i < instance.zonesContainer.childCount; i++) {
			Destroy(instance.zonesContainer.GetChild(i).gameObject);
		}
	}

	public static void BeginNewZonePlacement() {
		instance.zonePlacerScript.StartNewZonePlacement();
	}

	public static void BeginCustomZonePlacement(UnityAction actionOnConfirmPlacement) {
		instance.zonePlacerScript.StartCustomPlacement(actionOnConfirmPlacement);
	}

    public static void CreateNewZoneAtPoint(Vector3 point, bool autoOpenEditMenu = true)
    {
        Zone newZone = new Zone("New Zone");		
        GameObject newSpot = Instantiate(instance.zonePrefab, point, Quaternion.identity);
        newSpot.transform.parent = instance.zonesContainer;
		ZoneSpot spotScript = newSpot.GetComponent<ZoneSpot>();
		spotScript.data = newZone;
		spotScript.RefreshDataDisplay();

		if (autoOpenEditMenu)
        {
            GameInterface.instance.EditZone(newZone, true);
        } 
    }

	public static ZoneSpot GetZoneSpotByZoneName(string zoneName) {
		Transform theZoneTransform = instance.zonesContainer.Find(zoneName);
		if (theZoneTransform) {
			return theZoneTransform.GetComponent<ZoneSpot>();
		}

		return null;
	}

	public static void SetupAllZonesFromData() {
		List<Zone> zones = GameController.instance.curData.zones;
		for(int i = 0; i < zones.Count; i++) {
			PlaceZone(zones[i]);
		}
	}

	public static void LinkAllZonesFromData() {
		Debug.Log("TODO Zone linking");
	}

	/// <summary>
	/// place a zone using its saved coordinates
	/// </summary>
	/// <param name="targetZone"></param>
    public static void PlaceZone(Zone targetZone)
    {
		GameObject newSpot = Instantiate(instance.zonePrefab, targetZone.CoordsForWorld, Quaternion.identity);
		newSpot.transform.parent = instance.zonesContainer;
		newSpot.GetComponent<ZoneSpot>().data = targetZone;
	}
}
