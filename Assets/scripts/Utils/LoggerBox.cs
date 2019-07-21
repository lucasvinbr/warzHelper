using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoggerBox : ScriptedPrefabRecyclerUser<LoggerBoxEntry>
{

	public static LoggerBox instance;

	public const float LOGMSG_STAY_TIME = 15.0f, LOGMSG_FADE_TIME = 1.5f;

	private WaitForSeconds waitForStayTime;

	protected override void Awake() {
		base.Awake();
		instance = this;
		waitForStayTime = new WaitForSeconds(LOGMSG_STAY_TIME);
	}

	public LoggerBoxEntry WriteText(string txt) {
		LoggerBoxEntry newTxt = GetAnEntry();
		newTxt.SetContent(txt);
		StartCoroutine(EntryStayRoutine(newTxt));
		return newTxt;
	}

	public LoggerBoxEntry WriteText(string txt, Color txtColor) {
		LoggerBoxEntry newTxt = GetAnEntry();
		newTxt.SetContent(txt, txtColor);
		StartCoroutine(EntryStayRoutine(newTxt));
		return newTxt;
	}

	public LoggerBoxEntry GetAnEntry() {
		LoggerBoxEntry gotObj = cycler.GetScriptedObj();
		gotObj.RestoreDefaultLooks();
		return gotObj;
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
		cycler.PoolObj(entry.gameObject);
	}

}
