using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// recycler for prefabbed objects with a script of interest.
/// Nothing fancy, just GetComponent on top of the base prefab cycler
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class ScriptedPrefabRecycler<T> : SimplePrefabRecycler where T : MonoBehaviour
{
	  
	public T GetScriptedObj() {
		return GetAnObj().GetComponent<T>();
	}

	public T PlaceSriptedObjAt(Vector3 pos) {
		return PlaceObjAt(pos).GetComponent<T>();
	}
}