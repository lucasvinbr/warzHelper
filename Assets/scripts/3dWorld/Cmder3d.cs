using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a commander in the 3d world
/// </summary>
public class Cmder3d : MonoBehaviour {

    public Commander data;

	public Renderer cmderRenderer;

    void Start()
    {
		RefreshDataDisplay();
    }

	public void DeleteThisSpot() {
		Destroy(gameObject);
	}

	/// <summary>
	/// sets the cmder object color and name
	/// </summary>
    public void RefreshDataDisplay()
    {
        if(data != null)
        {
			Faction ownerFac = GameController.GetFactionByID(data.ownerFaction);
			cmderRenderer.sharedMaterial = ownerFac != null ?
				GameController.instance.facMatsHandler.factionMaterialsDict[ownerFac.ID] :
				GameController.instance.facMatsHandler.neutralZoneMaterial;
			gameObject.name = ownerFac.name + "_" + data.ID;
        }
    }

}
