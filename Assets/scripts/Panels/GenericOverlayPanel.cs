using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a class that affects the openedPanelsOverlayLevel of GameInterface
/// </summary>
public class GenericOverlayPanel : MonoBehaviour {

	public virtual void OnEnable() {
		GameInterface.openedPanelsOverlayLevel++;
	}

	public virtual void OnDisable() {
		GameInterface.openedPanelsOverlayLevel--;
	}
}
