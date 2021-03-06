using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a zone in the 3d world
/// </summary>
public class ZoneSpot : TroopContainer3d {

    public GUIFollowerText myLabel;

    public Transform labelPoint;

	public const float CMDER_OFFSET = 0.75f;

	public const int NUM_CMDERS_BEFORE_PILE = 8;

	private GameObject _highlight;

	/// <summary>
	/// creates, removes or checks if the zone has a child highlight
	/// </summary>
	public bool Highlighted
	{
		get
		{
			return _highlight;
		}
		set
		{
			if (value) {
				if (!_highlight) {
					_highlight = WorldVisualFeedbacks.instance.zoneHighlightCycler.PlaceObjAt(transform);
				}
			}else {
				if (_highlight) {
					WorldVisualFeedbacks.instance.zoneHighlightCycler.PoolObj(_highlight);
					_highlight = null;
				}
			}
		}
	}

    void OnDestroy()
    {
        if (myLabel)
        {
            FollowerTextCanvasRecycler.instance.cycler.PoolObj(myLabel.gameObject);
        }
    }

	public void GuardMyLabel() {
		if (!myLabel) {
			myLabel = FollowerTextCanvasRecycler.GetAFollower();
			myLabel.FollowThis = labelPoint;
		}
	}

	/// <summary>
	/// refreshes label text and the spot's 3d representation color.
	/// Does not update position-related stuff or links!
	/// </summary>
    public override void RefreshDataDisplay()
    {
		GuardMyLabel();
        if(data != null)
        {
			string zoneName = (data as Zone).name;
            myLabel.SetText(zoneName);
			Faction ownerFac = GameController.GetFactionByID(data.ownerFaction);
			myLabel.myText.color = ownerFac != null ? ownerFac.color : Color.white;
			containerRenderer.sharedMaterial = ownerFac != null ?
				GameController.instance.facMatsHandler.factionMaterialsDict[ownerFac.ID] :
				GameController.instance.facMatsHandler.neutralZoneMaterial;
			gameObject.name = zoneName;
        }
    }

	/// <summary>
	/// visually updates things that rely on the zone's position, like merc caravans and links
	/// </summary>
	public void RefreshPositionRelatedStuff() {
		Zone thisZone = data as Zone;

		MercCaravan localCaravan = GameController.GetMercCaravanInZone(thisZone.ID);
		if (localCaravan != null) localCaravan.MeIn3d.InstantlyUpdatePosition();

		foreach (int zoneID in thisZone.linkedZones) {
			Zone linkedZone = GameController.GetZoneByID(zoneID);
			World.GetLinkLineBetween(data, linkedZone).UpdatePositions();
		}
	}

	/// <summary>
	/// uses the number of cmders in this zone to find a new spot for a cmder
	/// </summary>
	/// <returns></returns>
	public Vector3 GetGoodSpotForCommander() {
		return GetGoodSpotForCommander(GameController.GetCommandersInZone(data as Zone).Count);
	}

	/// <summary>
	/// instead of considering the commander count,
	/// this uses the provided int to find a spot for a cmder
	/// </summary>
	/// <param name="placementIndex"></param>
	/// <returns></returns>
	public Vector3 GetGoodSpotForCommander(int placementIndex) {
		float placementRads = (Mathf.PI / (NUM_CMDERS_BEFORE_PILE / 2.0f)) * placementIndex,
			placementOffset = CMDER_OFFSET * (1 + (placementIndex / NUM_CMDERS_BEFORE_PILE));
		return transform.position + new Vector3(Mathf.Cos(placementRads) * placementOffset,
			0,
			Mathf.Sin(placementRads) * placementOffset
			);
		
	}

}
