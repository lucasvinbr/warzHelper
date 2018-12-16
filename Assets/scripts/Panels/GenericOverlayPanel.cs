using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a class that affects the openedPanelsOverlayLevel of GameInterface
/// </summary>
public class GenericOverlayPanel : MonoBehaviour {

	public virtual void OnEnable() {
		GameInterface.openedPanelsOverlayLevel++;
		GameInterface.instance.overlayPanelsCurrentlyOpen.Add(this);
	}

	public virtual void OnDisable() {
		GameInterface.openedPanelsOverlayLevel--;
		GameInterface.instance.overlayPanelsCurrentlyOpen.Remove(this);
	}
}
