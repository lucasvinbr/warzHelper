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

	public Vector3 GetGoodSpotForCommander() {
		int cmderCount = GameController.GetCommandersInZone(data).Count;
		return transform.position + new Vector3((CMDER_OFFSET / 2) * (cmderCount % 2 == 1 ? -1 : 1),
			CMDER_OFFSET * (cmderCount / 4),
			CMDER_OFFSET * (cmderCount % 4 <= 1 ? 1 : -1));
	}

}
