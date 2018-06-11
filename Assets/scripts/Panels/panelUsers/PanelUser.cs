using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// some convenience stuff for objects that do "Open Panel" tasks
/// </summary>
public class PanelUser<T> : MonoBehaviour where T : InputPanel {

	public T thePanel;

	protected virtual void Reset() {
		T[] foundPanels = Resources.FindObjectsOfTypeAll<T>();
		if (foundPanels.Length > 0) {
			thePanel = foundPanels[0];
		}
	}
}
