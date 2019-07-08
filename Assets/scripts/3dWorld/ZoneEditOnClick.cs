using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ZoneEditOnClick : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

    void Start()
    {
        cam = Camera.main;
    }


    void Update()
    {
		if (EventSystem.current.IsPointerOverGameObject() || 
			GameInterface.openedPanelsOverlayLevel > 0) {
			return;
		}

		if (Input.GetButtonDown("Select")) {
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
				ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
				if (hitSpotScript) {
					GameInterface.instance.EditZone(hitSpotScript.data as Zone, false);
				}
			}
		}
    }

}
