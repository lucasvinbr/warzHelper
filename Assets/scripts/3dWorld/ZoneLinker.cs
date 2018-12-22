using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZoneLinker : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

    public UnityAction actionOnDoneLinking;

	public ZoneSpot curSelectedSpot;

	public Transform zoneHighlight;

    void Start()
    {
        cam = Camera.main;
    }


	public void StartZoneLinking(UnityAction actionOnConfirmPlace) {
		this.enabled = true;
		actionOnDoneLinking += actionOnConfirmPlace;
		World.instance.zoneGrowScript.enabled = true;
	}

	public void DoneLinking() {
		actionOnDoneLinking?.Invoke();
		enabled = false;
		World.instance.zoneGrowScript.enabled = false;
		actionOnDoneLinking = null;
		TemplateModeUI templateUI = GameInterface.instance.templateOptionsPanel as TemplateModeUI;
		templateUI.SetDisplayedLowerHUD(templateUI.mainLowerHUD);
	}

    void Update()
    {

		if (Input.GetButtonDown("Select")) {
			if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8)) {
				ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
				if (!curSelectedSpot) {
					//select zone if no zone was already selected
					curSelectedSpot = hitSpotScript;
					zoneHighlight.gameObject.SetActive(true);
					zoneHighlight.transform.position = curSelectedSpot.transform.position;
				}
				else {
					//link/unlink to selected if one was already selected
					//...or deselect, if it's the selected zone
					if(curSelectedSpot == hitSpotScript) {
						curSelectedSpot = null;
						zoneHighlight.gameObject.SetActive(false);
					}else {
						if(World.GetLinkLineBetween(curSelectedSpot.data, hitSpotScript.data)) {
							World.RemoveZoneLink(curSelectedSpot.data, hitSpotScript.data, true);
						}else {
							World.PlaceZoneLink(curSelectedSpot, hitSpotScript, true);
						}
					}
				}
			}
			else {
				//deselect
				curSelectedSpot = null;
				zoneHighlight.gameObject.SetActive(false);
			}
		}
    }

}
