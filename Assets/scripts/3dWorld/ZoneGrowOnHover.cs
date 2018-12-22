using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// script that exists just as a small feedback to users when hovering zones.
/// it makes zones get a bit bigger when hovered
/// </summary>
public class ZoneGrowOnHover : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

	private Transform curHoveredZone;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
		//zones' colliders are in the "zone" layer (num. 8)
		//hovered zones should grow a little
		if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
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

}
