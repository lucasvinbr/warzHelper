using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class World : MonoBehaviour {

    public Transform ground;
    public Transform zonesContainer, mercCaravansContainer;

    public GameObject zonePrefab, mercCaravan3dPrefab;

    public static World instance;

    public ZonePlacer zonePlacerScript;

	public ZoneMover zoneMoverScript;

	public ZoneLinker zoneLinkerScript;

	public ZoneGrowOnHover zoneGrowScript;

	public ZoneEditOnClick zoneEditOnClickScript;

	public CommanderPlacer cmderPlacerScript;

	public WorldGarrDescOnHover garrDescOnHoverScript;

	private List<LinkLine> linkLines = new List<LinkLine>();

	private List<Cmder3d> spawnedCmders = new List<Cmder3d>();

	private List<MercCaravan3d> spawnedMCs = new List<MercCaravan3d>();

	void Awake() {
		instance = this;
	}

	/// <summary>
	/// sets the board's size and texture according to the stored data
	/// </summary>
	public static void SetupBoardDetails() {
		Rules curRules = GameController.instance.curData.rules;
		instance.ground.localScale = new Vector3(curRules.boardDimensions.x, curRules.boardDimensions.y, 1);
		if (!string.IsNullOrEmpty(curRules.boardTexturePath)) {
			Texture loadedTex = TexLoader.GetOrLoadTex(curRules.boardTexturePath);
			if (loadedTex) {
				instance.ground.GetComponent<Renderer>().sharedMaterial.mainTexture = loadedTex;
			}
		}
	}

	/// <summary>
	/// toggles display of the "ground" used in templates and games
	/// </summary>
	/// <param name="active"></param>
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
		LinkLineRecycler.instance.cycler.PoolAllObjs();

		instance.linkLines.Clear();
	}

	/// <summary>
	/// Removes all cmder3ds
	/// </summary>
	public static void CleanCmders() {
		while(instance.spawnedCmders.Count > 0) {
			RemoveCmder3d(instance.spawnedCmders[0]);
		}
	}

	/// <summary>
	/// Removes all mercCaravan3ds
	/// </summary>
	public static void CleanMCaravans() {
		while (instance.spawnedMCs.Count > 0) {
			RemoveMercCaravan3d(instance.spawnedMCs[0]);
		}
	}

	public static void BeginNewZonePlacement() {
		instance.zonePlacerScript.StartNewZonePlacement();
	}

	public static void BeginCustomZonePlacement(UnityAction actionOnConfirmPlacement) {
		instance.zonePlacerScript.StartCustomPlacement(actionOnConfirmPlacement);
	}

	public static void BeginZoneMoving() {
		instance.zoneMoverScript.StartSelectionProcedure();
	}

	public static void BeginZoneLinking(UnityAction actionOnDoneLinking) {
		instance.zoneLinkerScript.StartZoneLinking(actionOnDoneLinking);
	}

	public static void BeginNewCmderPlacement(UnityAction actionOnPlaced, List<ZoneSpot> allowedSpots) {
		instance.cmderPlacerScript.StartNewPlacement(actionOnPlaced, allowedSpots);
	}

	public static void NewCmderPlacementUpdateAllowedZones(List<ZoneSpot> allowedSpots)
	{
		instance.cmderPlacerScript.UpdateAllowedSpots(allowedSpots);
	}

    public static void CreateNewZoneAtPoint(Vector3 point, bool autoOpenEditMenu = true)
    {
        Zone newZone = new Zone("New Zone", point);		
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

	public static Commander CreateNewCmderAtZone(int targetZoneID, Faction ownerFac) {
		Zone targetZone = GameController.GetZoneByID(targetZoneID);

		if(targetZone != null) {
			return CreateNewCmderAtZone(targetZone, ownerFac);
		}

		return null;
	}

	public static Commander CreateNewCmderAtZone(Zone targetZone, Faction ownerFac) {
		Commander newCmder = new Commander(ownerFac.ID, targetZone.ID);
		Cmder3d cmd3d = Cmder3dRecycler.GetACmderObj();
		cmd3d.transform.position = targetZone.MyZoneSpot.
			GetGoodSpotForCommander(GameController.GetCommandersInZone(targetZone).Count - 1);
		cmd3d.data = newCmder;
		cmd3d.RefreshDataDisplay();
		instance.spawnedCmders.Add(cmd3d);

		return newCmder;
	}

	/// <summary>
	/// places a new merc caravan in the target zone. There shouldn't be two caravans in the same zone!
	/// </summary>
	/// <param name="targetZone"></param>
	public static MercCaravan CreateNewMercCaravanAtZone(Zone targetZone) {
		MercCaravan newCaravan = new MercCaravan(targetZone.ID);
		
		return PlaceSavedMercCaravan(newCaravan).data;
	}

	public static ZoneSpot GetZoneSpotByZoneName(string zoneName) {
		Transform theZoneTransform = instance.zonesContainer.Find(zoneName);
		if (theZoneTransform) {
			return theZoneTransform.GetComponent<ZoneSpot>();
		}

		return null;
	}

	public static Cmder3d GetCmder3dForCommander(Commander theCmder) {
		foreach(Cmder3d cmd3d in instance.spawnedCmders) {
			if((cmd3d.data as Commander).ID == theCmder.ID) {
				return cmd3d;
			}
		}

		return null;
	}

	public static MercCaravan3d GetCaravan3dForMC(MercCaravan theMC) {
		foreach (MercCaravan3d MC3d in instance.spawnedMCs) {
			if (MC3d.data.ID == theMC.ID) {
				return MC3d;
			}
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

	/// <summary>
	/// places all commanders stored in the current game data
	/// </summary>
	public static void SetupAllCommandersFromData() {
		Dictionary<Zone, int> placedCmdersInEachZone = new Dictionary<Zone, int>();
		foreach(Zone z in GameController.instance.curData.zones) {
			placedCmdersInEachZone.Add(z, 0);
		}
		List<Commander> cmders = GameController.instance.curData.deployedCommanders;
		Cmder3d cmd3d = null;
		Zone curZone = null;
		for (int i = 0; i < cmders.Count; i++) {
			cmd3d = Cmder3dRecycler.GetACmderObj();
			curZone = GameController.GetZoneByID(cmders[i].zoneIAmIn);
			cmd3d.transform.position = 
				curZone.MyZoneSpot.GetGoodSpotForCommander(placedCmdersInEachZone[curZone]);
			cmd3d.data = cmders[i];
			cmd3d.RefreshDataDisplay();
			instance.spawnedCmders.Add(cmd3d);
			placedCmdersInEachZone[curZone]++;
		}
	}

	/// <summary>
	/// places all caravans stored in the current game data
	/// </summary>
	public static void SetupAllMercCaravansFromData() {
		foreach (MercCaravan MC in GameController.instance.curData.mercCaravans) {
			PlaceSavedMercCaravan(MC);
		}
		
	}

	/// <summary>
	/// Reset the positions of all the cmder3ds in the target zone, so that future cmder3ds 
	/// won't overlap with those who stay if some switch zones
	/// </summary>
	public static void TidyZone(Zone targetZone) {
		if (targetZone == null) return;

		int resetCmders = 0;
		//adjust positions for commanders that are visually in our zone already
		foreach(Commander cmd in GameController.GetCommandersInZone(targetZone, true)) {
			cmd.MeIn3d.transform.position = targetZone.MyZoneSpot.GetGoodSpotForCommander(resetCmders);
			resetCmders++;
		}
		//then reset destinations for cmders still tweening towards this zone
		TransformTweener.instance.AdjustTweensThatTargetZone(targetZone.MyZoneSpot);
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

	public static void PlaceZoneLink(ZoneSpot zs1, ZoneSpot zs2, bool alsoUpdateTheirLinkedList = false) {
		LinkLine theLink = LinkLineRecycler.GetALine();
		Zone z1 = zs1.data as Zone, z2 = zs2.data as Zone;
		theLink.SetLink(zs1, zs2);
		instance.linkLines.Add(theLink);
		if (alsoUpdateTheirLinkedList) {
			z1.linkedZones.Add(z2.ID);
			z2.linkedZones.Add(z1.ID);
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
		LinkLineRecycler.instance.cycler.PoolObj(theLink.gameObject);
		instance.linkLines.Remove(theLink);
		if (alsoUpdateTheirLinkedList) {
			z1.linkedZones.Remove(z2.ID);
			z2.linkedZones.Remove(z1.ID);
		}
	}



	public static LinkLine GetLinkLineBetween(TroopContainer c1, TroopContainer c2) {
		foreach(LinkLine line in instance.linkLines) {
			if(line.LinksContainer(c1) && line.LinksContainer(c2)) {
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

	/// <summary>
	/// place a merc caravan in the world using its saved location
	/// </summary>
	/// <param name="targetZone"></param>
	public static MercCaravan3d PlaceSavedMercCaravan(MercCaravan MCdata) {
		Zone targetZone = GameController.GetZoneByID(MCdata.zoneIAmIn);
		GameObject mc3d = Instantiate(instance.mercCaravan3dPrefab, targetZone.CoordsForWorld, Quaternion.identity);
		mc3d.transform.parent = instance.mercCaravansContainer;
		MercCaravan3d MC3dScript = mc3d.GetComponent<MercCaravan3d>();
		MC3dScript.data = MCdata;
		MC3dScript.RefreshDataDisplay();
		instance.spawnedMCs.Add(MC3dScript);
		return MC3dScript;
	}

	public static void RemoveCmder3d(Cmder3d target3d) {
		Cmder3dRecycler.instance.cycler.PoolObj(target3d.gameObject);
		instance.spawnedCmders.Remove(target3d);
		target3d.transform.localScale = Vector3.one;
	}

	public static void RemoveMercCaravan3d(MercCaravan3d target3d) {
		instance.spawnedMCs.Remove(target3d);
		Destroy(target3d.gameObject);
	}
}
