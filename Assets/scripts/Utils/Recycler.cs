using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Recycler<T> : MonoBehaviour where T : MonoBehaviour
{

    public List<T> activeObjs = new List<T>();

    public List<T> pooledObjs = new List<T>();

    /// <summary>
    /// gets an object from the pool or creates a new one
    /// </summary>
    /// <returns></returns>
    public virtual T GetAnObj()
    {
        T pickedObj = null;
        if (pooledObjs.Count > 0)
        {
            pickedObj = pooledObjs[0];
            pooledObjs.RemoveAt(0);
        }
        else {
            pickedObj = CreateNewObj();
        }

        pickedObj.gameObject.SetActive(true);
        activeObjs.Add(pickedObj);

        return pickedObj;
    }

    public virtual T CreateNewObj()
    {
        GameObject newGO = new GameObject(typeof(T).ToString());
        return newGO.AddComponent<T>();
    }

    public virtual void PoolObj(T theObj)
    {
        theObj.gameObject.SetActive(false);
        activeObjs.Remove(theObj);
        pooledObjs.Add(theObj);
    }
}