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

	public const float CMDER_OFFSET = 0.5f;


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
		int cmderCount = GameController.GetCommandersInZone(data).Count;
		return transform.position + new Vector3((CMDER_OFFSET / 2) * (cmderCount % 2 == 1 ? -1 : 1),
			CMDER_OFFSET * (cmderCount / 4),
			CMDER_OFFSET * (cmderCount % 4 <= 1 ? 1 : -1));
	}

	/// <summary>
	/// instead of considering the commander count,
	/// this uses the provided int to find a spot for a cmder
	/// </summary>
	/// <param name="placementIndex"></param>
	/// <returns></returns>
	public Vector3 GetGoodSpotForCommander(int placementIndex) {
		return transform.position + new Vector3((CMDER_OFFSET / 2) * (placementIndex % 2 == 1 ? -1 : 1),
			CMDER_OFFSET * (placementIndex / 4),
			CMDER_OFFSET * (placementIndex % 4 <= 1 ? 1 : -1));
	}

}
