using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour {

    private Transform theGround;

	private float curZoom;

	private Camera theCam;

	public float minZoom, maxZoom;

	public float zoomSpeed;
	
	// Use this for initialization
	void Start () {
		theCam = Camera.main;
		curZoom = theCam.fieldOfView;
        theGround = World.instance.ground;
	}
	
	// Update is called once per frame
	void Update () {
        if (theGround.gameObject.activeSelf && GameInterface.openedPanelsOverlayLevel == 0) { 

            float inputZ = Input.GetAxis("Mouse ScrollWheel") * -zoomSpeed;
			curZoom = Mathf.Clamp(curZoom + inputZ, minZoom, maxZoom);

			theCam.fieldOfView = curZoom;
        }
	}
}
