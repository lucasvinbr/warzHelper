using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a commander in the 3d world
/// </summary>
public class Cmder3d : TroopContainer3d {


	/// <summary>
	/// sets the cmder object color and name
	/// </summary>
    public override void RefreshDataDisplay()
    {
        if(data != null)
        {
			Faction ownerFac = GameController.GetFactionByID(data.ownerFaction);
			containerRenderer.sharedMaterial = ownerFac != null ?
				GameController.instance.facMatsHandler.factionMaterialsDict[ownerFac.ID] :
				GameController.instance.facMatsHandler.neutralZoneMaterial;
			gameObject.name = ownerFac.name + "_" + (data as Commander).ID;
        }
    }

}
