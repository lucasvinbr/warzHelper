using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// script that exists just as a small feedback to users when hovering zones.
/// it makes zones get a bit bigger when hovered
/// </summary>
public class ZoneGrowOnHover : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

	private Transform curHoveredZone;

	public List<ZoneSpot> zoneWhitelist;


    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
		if (EventSystem.current.IsPointerOverGameObject()) {
			return;
		}
		//zones' colliders are in the "zone" layer (num. 8)
		//hovered zones should grow a little
		if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
			if(zoneWhitelist != null) {
				ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
				if (!zoneWhitelist.Contains(hitSpotScript)) {
					return;
				}
			}
			
			if (!curHoveredZone) {
				curHoveredZone = hit.transform;
				curHoveredZone.localScale = Vector3.one * 1.25f;
			}
		}else {
			if (curHoveredZone) {
				curHoveredZone.localScale = Vector3.one;
				curHoveredZone = null;
			}
		}

    }


	private void OnDisable() {
		zoneWhitelist = null;
		if (curHoveredZone) {
			curHoveredZone.localScale = Vector3.one;
			curHoveredZone = null;
		}
	}

}
