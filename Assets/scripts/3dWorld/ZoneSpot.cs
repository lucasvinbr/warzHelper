using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a zone in the 3d world
/// </summary>
public class ZoneSpot : MonoBehaviour {

    public Zone data;

    public GUIFollowerText myLabel;

    public Transform labelPoint;

	public Renderer spotRenderer;

	public const float CMDER_OFFSET = 0.75f;

	public const int NUM_CMDERS_BEFORE_PILE = 8;


    void Start()
    {
		RefreshDataDisplay();
    }

    void OnDestroy()
    {
        if (myLabel)
        {
            FollowerTextCanvasRecycler.instance.PoolObj(myLabel);
        }
    }

	public void DeleteThisSpot() {
		Destroy(gameObject);

	}

	public void GuardMyLabel() {
		if (!myLabel) {
			myLabel = FollowerTextCanvasRecycler.GetAFollower();
			myLabel.FollowThis = labelPoint;
		}
	}

	/// <summary>
	/// refreshes label text and the spot's 3d representation color
	/// </summary>
    public void RefreshDataDisplay()
    {
		GuardMyLabel();
        if(data != null)
        {
            myLabel.SetText(data.name);
			Faction ownerFac = GameController.GetFactionByID(data.ownerFaction);
			myLabel.myText.color = ownerFac != null ? ownerFac.color : Color.white;
			spotRenderer.sharedMaterial = ownerFac != null ?
				GameController.instance.facMatsHandler.factionMaterialsDict[ownerFac.ID] :
				GameController.instance.facMatsHandler.neutralZoneMaterial;
			gameObject.name = data.name;
        }
    }

	/// <summary>
	/// uses the number of cmders in this zone to find a new spot for a cmder
	/// </summary>
	/// <returns></returns>
	public Vector3 GetGoodSpotForCommander() {
		return GetGoodSpotForCommander(GameController.GetCommandersInZone(data).Count);
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
