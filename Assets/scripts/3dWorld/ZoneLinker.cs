using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZoneLinker : MonoBehaviour {

    private Camera cam;
    private RaycastHit hit;

    public UnityAction actionOnDoneLinking;

    void Start()
    {
        cam = Camera.main;
    }


	public void StartZoneLinking(UnityAction actionOnConfirmPlace) {
		this.enabled = true;
		actionOnDoneLinking += actionOnConfirmPlace;
	}

	public void DoneLinking() {
		actionOnDoneLinking?.Invoke();
		enabled = false;
		actionOnDoneLinking = null;
		TemplateModeUI templateUI = GameInterface.instance.templateOptionsPanel as TemplateModeUI;
		templateUI.SetDisplayedLowerHUD(templateUI.mainLowerHUD);
	}

    void Update()
    {
        if(Physics.Raycast( cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 0))
        {
            //zoneBlueprint.position = hit.point;
        }

        if (Input.GetButtonDown("Select"))
        {
			//TODO check if zone is under mouse;
			//if it is,
			//link/unlink to selected if one was already selected
			//select zone if no zone was already selected
        }
    }

}
