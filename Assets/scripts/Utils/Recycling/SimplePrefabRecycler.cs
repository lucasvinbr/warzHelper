using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// recycler for simple objects that must be placed somewhere
/// </summary>
[System.Serializable]
public class SimplePrefabRecycler : Recycler<GameObject>
{

	public GameObject objPrefab;
	public Transform activeObjsParent, pooledObjsParent;

    /// <summary>
    /// gets an object from the pool or creates a new one
    /// </summary>
    /// <returns></returns>
    public override GameObject GetAnObj()
    {
		GameObject pickedObj = base.GetAnObj();
		pickedObj.SetActive(true);
		pickedObj.transform.SetParent(activeObjsParent, false);
		return pickedObj;
    }

    public override GameObject CreateNewObj()
    {
		GameObject newGO = Object.Instantiate(objPrefab, activeObjsParent);
        return newGO;
    }

    public override void PoolObj(GameObject theObj)
    {
		base.PoolObj(theObj);
		theObj.transform.SetParent(pooledObjsParent, false);
		theObj.SetActive(false);
    }

	public virtual GameObject PlaceObjAt(Vector3 targetPos) {
		GameObject placedObj = GetAnObj();
		placedObj.transform.position = targetPos;
		return placedObj;
	}

	public virtual GameObject PlaceObjAt(Transform parent) {
		GameObject placedObj = GetAnObj();
		placedObj.transform.SetParent(parent, false);
		placedObj.transform.localPosition = Vector3.zero;
		return placedObj;
	}
}