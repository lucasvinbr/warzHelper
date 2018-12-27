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

	public ZoneLinker zoneLinkerScript;

	public ZoneGrowOnHover zoneGrowScript;

	public CommanderPlacer cmderPlacerScript;

	private List<LinkLine> linkLines = new List<LinkLine>();

	private List<Cmder3d> spawnedCmders = new List<Cmder3d>();

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

	/// <summary>
	/// destroys all zone spots
	/// </summary>
	public static void CleanZonesContainer() {
		for(int i = 0; i < instance.zonesContainer.childCount; i++) {
			Destroy(instance.zonesContainer.GetChild(i).gameObject);
		}
	}

	/// <summary>
	/// destroys (pools) all zone links
	/// </summary>
	public static void CleanZoneLinks() {
		foreach(LinkLine line in instance.linkLines) {
			LinkLineRecycler.instance.PoolObj(line);
		}

		instance.linkLines.Clear();
	}

	public static void CleanCmders() {
		foreach (Cmder3d cmder in instance.spawnedCmders) {
			Cmder3dRecycler.instance.PoolObj(cmder);
		}

		instance.spawnedCmders.Clear();
	}

	public static void BeginNewZonePlacement() {
		instance.zonePlacerScript.StartNewZonePlacement();
	}

	public static void BeginCustomZonePlacement(UnityAction actionOnConfirmPlacement) {
		instance.zonePlacerScript.StartCustomPlacement(actionOnConfirmPlacement);
	}

	public static void BeginZoneLinking(UnityAction actionOnDoneLinking) {
		instance.zoneLinkerScript.StartZoneLinking(actionOnDoneLinking);
	}

	public static void BeginNewCmderPlacement(UnityAction actionOnPlaced, List<ZoneSpot> allowedSpots) {
		instance.cmderPlacerScript.StartNewPlacement(actionOnPlaced, allowedSpots);
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

	public static void CreateNewCmderAtZone(Zone targetZone, Faction ownerFac) {
		Commander newCmder = new Commander(ownerFac.ID, targetZone.ID);
		Cmder3d cmd3d = Cmder3dRecycler.GetACmderObj();
		cmd3d.transform.position = targetZone.MyZoneSpot.GetGoodSpotForCommander();
		cmd3d.data = newCmder;
		cmd3d.RefreshDataDisplay();
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
		List<Zone> zones = GameController.instance.curData.zones;
		Zone theLinkedZone = null;
		foreach(Zone z in zones) {
			foreach(int linkedZoneID in z.linkedZones) {
				theLinkedZone = GameController.GetZoneByID(linkedZoneID);
				if (GetLinkLineBetween(z, theLinkedZone)) {
					continue;
				}else {
					PlaceZoneLink(z, theLinkedZone);
				}
			}
		}
	}

	public static void PlaceZoneLink(Zone z1, Zone z2, bool alsoUpdateTheirLinkedList = false) {
		LinkLine theLink = LinkLineRecycler.GetALine();
		theLink.SetLink(z1.MyZoneSpot, z2.MyZoneSpot);
		instance.linkLines.Add(theLink);
		if (alsoUpdateTheirLinkedList) {
			z1.linkedZones.Add(z2.ID);
			z2.linkedZones.Add(z1.ID);
		}
	}

	public static void PlaceZoneLink(ZoneSpot z1, ZoneSpot z2, bool alsoUpdateTheirLinkedList = false) {
		LinkLine theLink = LinkLineRecycler.GetALine();
		theLink.SetLink(z1, z2);
		instance.linkLines.Add(theLink);
		if (alsoUpdateTheirLinkedList) {
			z1.data.linkedZones.Add(z2.data.ID);
			z2.data.linkedZones.Add(z1.data.ID);
		}
	}

	/// <summary>
	/// destroys (pools) the link line between two zones, optionally updating both their linkedZones list
	/// </summary>
	/// <param name="z1"></param>
	/// <param name="z2"></param>
	/// <param name="alsoUpdateTheirLinkedList"></param>
	public static void RemoveZoneLink(Zone z1, Zone z2, bool alsoUpdateTheirLinkedList = false) {
		LinkLine theLink = GetLinkLineBetween(z1, z2);
		LinkLineRecycler.instance.PoolObj(theLink);
		instance.linkLines.Remove(theLink);
		if (alsoUpdateTheirLinkedList) {
			z1.linkedZones.Remove(z2.ID);
			z2.linkedZones.Remove(z1.ID);
		}
	}

	public static LinkLine GetLinkLineBetween(Zone z1, Zone z2) {
		foreach(LinkLine line in instance.linkLines) {
			if(line.LinksZone(z1) && line.LinksZone(z2)) {
				return line;
			}
		}

		return null;
	}

	/// <summary>
	/// makes all zoneSpots use the respective faction's colors
	/// </summary>
	public static void RefreshZoneSpotsOwnedByFaction(Faction fac) {
		foreach(Zone z in fac.OwnedZones) {
			z.MyZoneSpot.RefreshDataDisplay();
		}
	}

	/// <summary>
	/// place a zone using its saved coordinates
	/// </summary>
	/// <param name="targetZone"></param>
    public static void PlaceZone(Zone targetZone)
    {
		GameObject newSpot = Instantiate(instance.zonePrefab, targetZone.CoordsForWorld, Quaternion.identity);
		newSpot.transform.parent = instance.zonesContainer;
		ZoneSpot zsScript = newSpot.GetComponent<ZoneSpot>();
		zsScript.data = targetZone;
		zsScript.RefreshDataDisplay();
	}
}
