using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a Monobehaviour that uses a ScriptedPrefabRecycler.
/// Unity doesn't like generic stuff, so we manually set the recycler's "parents" and "prefab" options
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public class ScriptedPrefabRecyclerUser<T> : MonoBehaviour where T : MonoBehaviour
{

	public GameObject objPrefab;
	public Transform activeObjsParent, pooledObjsParent;

	public ScriptedPrefabRecycler<T> cycler;

	protected virtual void Awake() {
		if (cycler == null) cycler = new ScriptedPrefabRecycler<T>();
		cycler.activeObjsParent = activeObjsParent;
		cycler.pooledObjsParent = pooledObjsParent;
		cycler.objPrefab = objPrefab;
	}
}