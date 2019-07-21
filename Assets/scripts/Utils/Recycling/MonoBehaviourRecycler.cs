using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
/// <summary>
/// recycler for objects that have scripts of interest on them
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoBehaviourRecycler<T> : Recycler<T> where T : MonoBehaviour
{

    /// <summary>
    /// gets an object from the pool or creates a new one
    /// </summary>
    /// <returns></returns>
    public override T GetAnObj()
    {
		T pickedObj = base.GetAnObj();
		pickedObj.gameObject.SetActive(true);
		return pickedObj;
    }

    public override T CreateNewObj()
    {
        GameObject newGO = new GameObject(typeof(T).ToString());
        return newGO.AddComponent<T>();
    }

    public override void PoolObj(T theObj)
    {
		base.PoolObj(theObj);
        theObj.gameObject.SetActive(false);
    }
}