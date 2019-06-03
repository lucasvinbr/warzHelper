using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a mercenary caravan in the 3d world
/// </summary>
public class MercCaravan3d : MonoBehaviour {

    public MercCaravan data;

	public Renderer caravanRenderer;

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
			caravanRenderer.material.color = data.caravanColor;
			gameObject.name = "caravan_" + data.ID;
        }
    }

	public void InstantlyUpdatePosition() {
		transform.position = GameController.GetZoneByID(data.zoneIAmIn).MyZoneSpot.transform.position;
	}


	private void Update() {
		transform.Rotate(Vector3.down * Time.deltaTime * 2);
		transform.localScale = Vector3.one * (0.75f + (0.25f * Mathf.Sin(Time.time / 2)));
	}
}
