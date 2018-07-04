using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ZonePlacer : MonoBehaviour {

    public Transform zoneBlueprint;

    private Camera cam;
    private RaycastHit hit;

    public UnityAction actionOnSpotSelect;

    void Start()
    {
        cam = Camera.main;
    }

    void OnEnable()
    {
        zoneBlueprint.gameObject.SetActive(true);
    }

    void OnDisable()
    {
        zoneBlueprint.gameObject.SetActive(false);
    }

    public void StartNewZonePlacement(bool zoneIsNew)
    {
        this.enabled = true;
		if (zoneIsNew) {
			actionOnSpotSelect += OnNewZoneConfirmPlacement;
		}
        
    }

    void Update()
    {
        if(Physics.Raycast( cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 0))
        {
            zoneBlueprint.position = hit.point;
        }

        if (Input.GetButtonDown("Select"))
        {
            if(actionOnSpotSelect != null)
            {
                actionOnSpotSelect();
                enabled = false;
                actionOnSpotSelect = null;
            }
        }
    }

    void OnNewZoneConfirmPlacement()
    {
        World.CreateNewZoneAtPoint(zoneBlueprint.position);
    }
}
