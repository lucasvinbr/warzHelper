using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CommanderPlacer : MonoBehaviour {

    public Transform cmderBlueprint;

    private Camera cam;
    private RaycastHit hit;

    public UnityAction actionOnSpotSelect;

	List<ZoneSpot> allowedSpots;

	public ZoneSpot curHoveredValidZone;

    void Start()
    {
        cam = Camera.main;
    }


    void OnDisable()
    {
		actionOnSpotSelect = null;
        cmderBlueprint.gameObject.SetActive(false);
    }

	public void StartNewPlacement(UnityAction actionOnConfirmPlace, List<ZoneSpot> allowedSpots) {
		this.enabled = true;
		this.allowedSpots = allowedSpots;
		actionOnSpotSelect += actionOnConfirmPlace;
	}

    void Update()
    {
		if (GameInterface.openedPanelsOverlayLevel != 0) {
			return;
		}

		if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 8))
        {
			ZoneSpot hitSpotScript = hit.transform.GetComponent<ZoneSpot>();
			if (allowedSpots.Contains(hitSpotScript)) {
				//select zone if no zone was already selected
				curHoveredValidZone = hitSpotScript;
				cmderBlueprint.gameObject.SetActive(true);
				cmderBlueprint.position = hitSpotScript.GetGoodSpotForCommander();
				cmderBlueprint.Rotate(Vector3.up * Time.deltaTime * 100); //just a little rotation to make it prettier haha
			}
			else {
				curHoveredValidZone = null;
				cmderBlueprint.gameObject.SetActive(false);
			}
		}
		else {
			curHoveredValidZone = null;
			cmderBlueprint.gameObject.SetActive(false);
		}

        if (Input.GetButtonDown("Select"))
        {
            if(curHoveredValidZone != null)
            {
				NewCmderPhaseMan.OrderPlaceNewCmder((curHoveredValidZone.data as Zone).ID, GameModeHandler.instance.curPlayingFaction);
                actionOnSpotSelect();
                enabled = false;
            }
        }
    }


}
