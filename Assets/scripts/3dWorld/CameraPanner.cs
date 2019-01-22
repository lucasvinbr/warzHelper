using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPanner : MonoBehaviour {

    private Transform theGround;

    private Vector3 curPos;

	public static CameraPanner instance;

	private Vector3 startingTweenPos, targetTweenPos;
	private float elapsedTweenTime, tweenDuration;
	private bool tweening = false;

	private void Awake() {
		instance = this;	
	}

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

	/// <summary>
	/// instantly centralizes the camera on the spot, disregarding the board size
	/// </summary>
	/// <param name="spot"></param>
	public void JumpToSpot(Vector3 spot) {
		transform.position = new Vector3(spot.x, transform.position.y, spot.z);
	}

	/// <summary>
	/// moves to target position over time
	/// </summary>
	/// <param name="spot"></param>
	/// <param name="tweenDuration"></param>
	public void TweenToSpot(Vector3 spot, float tweenDuration = 0.35f) {
		elapsedTweenTime = 0;
		startingTweenPos = transform.position;
		targetTweenPos = new Vector3(spot.x, transform.position.y, spot.z); ;
		this.tweenDuration = tweenDuration;
		if (!tweening) {
			StartCoroutine(TweenRoutine());
		}
	}

	IEnumerator TweenRoutine() {
		tweening = true;
		while (elapsedTweenTime < tweenDuration) {
			transform.position = Vector3.Lerp
				(startingTweenPos, targetTweenPos, elapsedTweenTime / tweenDuration);
			elapsedTweenTime += Time.deltaTime;
			yield return null;
		}
		tweening = false;
	}
}
