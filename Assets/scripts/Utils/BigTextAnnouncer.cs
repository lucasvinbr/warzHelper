using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class BigTextAnnouncer : MonoBehaviour {

    public TMP_Text theTxt;
	public float growAndShrinkTime = 0.35f;

	public enum AnnouncementState {
		inactive,
		growing,
		staying,
		shrinking
	}

	public AnnouncementState curState = AnnouncementState.inactive;

	protected float elapsedStateTime = 0f;

	protected float desiredStayTime;

	public void DoAnnouncement(string announceContent, Color txtColor, float stayTime = 1.25f) {
		theTxt.text = announceContent;
		theTxt.color = txtColor;
		desiredStayTime = stayTime;

		if(curState == AnnouncementState.inactive) {
			StartCoroutine(AnnouncementRoutine());
		}else {
			curState = AnnouncementState.growing;
			elapsedStateTime = 0;
		}
	}

	IEnumerator AnnouncementRoutine() {
		curState = AnnouncementState.growing;

		while (curState != AnnouncementState.inactive) {
			switch (curState) {
				case AnnouncementState.growing:
					transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, elapsedStateTime / growAndShrinkTime);
					if(elapsedStateTime >= growAndShrinkTime) {
						curState = AnnouncementState.staying;
						elapsedStateTime = 0;
					}
					break;
				case AnnouncementState.staying:
					if (elapsedStateTime >= desiredStayTime) {
						curState = AnnouncementState.shrinking;
						elapsedStateTime = 0;
					}
					break;
				case AnnouncementState.shrinking:
					transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, elapsedStateTime / growAndShrinkTime);
					if (elapsedStateTime >= growAndShrinkTime) {
						curState = AnnouncementState.inactive;
						elapsedStateTime = 0;
					}
					break;
			}

			elapsedStateTime += Time.deltaTime;
			yield return null;
		}
	}
}
