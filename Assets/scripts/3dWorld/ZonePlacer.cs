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

	/// <summary>
	/// enables this script, and when the user clicks on a spot, a new zone is created there
	/// </summary>
    public void StartNewZonePlacement()
    {
        StartCustomPlacement(OnNewZoneConfirmPlacement);
    }

	public void StartCustomPlacement(UnityAction actionOnConfirmPlace) {
		this.enabled = true;
		actionOnSpotSelect += actionOnConfirmPlace;
	}

    void Update()
    {
        if(Physics.Raycast( cam.ScreenPointToRay(Input.mousePosition), out hit, 100, 1 << 0))
        {
            zoneBlueprint.position = hit.point;
			zoneBlueprint.Rotate(Vector3.up * Time.deltaTime * 100); //just a little rotation to make it prettier haha
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
