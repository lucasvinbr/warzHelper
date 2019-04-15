using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LoggerBoxEntry : MonoBehaviour
{

	public TMP_Text textContent;
	public CanvasGroup cg;

	public void SetContent(string txt) {
		textContent.text = txt;
	}

	public void SetContent(string txt, Color txtColor) {
		textContent.text = txt;
		textContent.color = txtColor;
	}

	public void RestoreDefaultLooks() {
		cg.alpha = 1.0f;
		textContent.color = Color.white;
	}
}
