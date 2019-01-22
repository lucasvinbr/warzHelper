using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModeUI : MonoBehaviour {

	[HideInInspector]
	public GameObject curDisplayedLowerHUD;

	abstract public void InitializeUI();

	abstract public void ClearUI();

	public virtual void SetDisplayedLowerHUD(GameObject targetHUD) {
		if(curDisplayedLowerHUD) curDisplayedLowerHUD.SetActive(false);
		curDisplayedLowerHUD = targetHUD;
		curDisplayedLowerHUD.SetActive(true);
	}
}
