using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanner : MonoBehaviour {

    private Transform theGround;

    private Vector3 curPos;

	// Use this for initialization
	void Start () {
        theGround = World.instance.ground;
	}
	
	// Update is called once per frame
	void Update () {
        if (theGround.gameObject.activeSelf && GameInterface.openedPanelsOverlayLevel == 0)
        {
            curPos = transform.position;
            float inputX = Input.GetAxis("Horizontal"), inputY = Input.GetAxis("Vertical");
            if((inputX > 0 && curPos.x >= theGround.localScale.x / 2) ||
                inputX < 0 && curPos.x <= -theGround.localScale.x / 2)
            {
                inputX = 0;
            }

            if ((inputY > 0 && curPos.z >= theGround.localScale.y / 2) ||
                inputY < 0 && curPos.z <= -theGround.localScale.y / 2)
            {
                inputY = 0;
            }

            transform.Translate(Vector3.right * inputX + Vector3.up * inputY);
        }
	}
}
