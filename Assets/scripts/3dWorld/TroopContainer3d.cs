using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3d world representation of a troop container, like a zone or commander
/// </summary>
public abstract class TroopContainer3d : MonoBehaviour
{

	public TroopContainer data;

	public Renderer containerRenderer;

	public virtual void Start() {
		RefreshDataDisplay();
	}

	public virtual void DeleteThisSpot() {
		Destroy(gameObject);
	}

	/// <summary>
	/// any data shown in this representation should be updated in this method.
	/// It should run after instantiation and whenever something changes
	/// </summary>
	public abstract void RefreshDataDisplay();

}
