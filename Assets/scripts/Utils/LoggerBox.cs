using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoggerBox : Recycler<LoggerBoxEntry>
{

	public GameObject entryPrefab;

	public Transform entriesContainer, inactivesContainer;

	public static LoggerBox instance;

	public const float LOGMSG_STAY_TIME = 15.0f, LOGMSG_FADE_TIME = 1.5f;

	private WaitForSeconds waitForStayTime;

	public void Awake() {
		instance = this;
		waitForStayTime = new WaitForSeconds(LOGMSG_STAY_TIME);
	}

	public LoggerBoxEntry WriteText(string txt) {
		LoggerBoxEntry newTxt = GetAnObj();
		newTxt.SetContent(txt);
		StartCoroutine(EntryStayRoutine(newTxt));
		return newTxt;
	}

	public LoggerBoxEntry WriteText(string txt, Color txtColor) {
		LoggerBoxEntry newTxt = GetAnObj();
		newTxt.SetContent(txt, txtColor);
		StartCoroutine(EntryStayRoutine(newTxt));
		return newTxt;
	}

	/// <summary>
	/// the entry stays for a while,
	/// then fades and is pooled
	/// </summary>
	/// <param name="entry"></param>
	/// <returns></returns>
	IEnumerator EntryStayRoutine(LoggerBoxEntry entry) {
		float elapsedTime = 0.0f;
		yield return waitForStayTime;

		while(elapsedTime <= LOGMSG_FADE_TIME) {
			entry.cg.alpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / LOGMSG_FADE_TIME);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		entry.cg.alpha = 0.0f;
		PoolObj(entry);
	}

	public override LoggerBoxEntry CreateNewObj() {
		GameObject newGO = Instantiate(entryPrefab);
		return newGO.GetComponent<LoggerBoxEntry>();
	}

	public override LoggerBoxEntry GetAnObj() {
		LoggerBoxEntry gotObj = base.GetAnObj();
		gotObj.RestoreDefaultLooks();
		gotObj.transform.SetParent(entriesContainer, false);
		return gotObj;
	}

	public override void PoolObj(LoggerBoxEntry theObj) {
		base.PoolObj(theObj);
		theObj.transform.SetParent(inactivesContainer, false);
	}
}
