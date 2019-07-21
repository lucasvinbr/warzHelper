using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// basic implementation of a pooling system
/// </summary>
/// <typeparam name="T"></typeparam>
[System.Serializable]
public abstract class Recycler<T>
{

    public List<T> activeObjs = new List<T>();

    public Queue<T> pooledObjs = new Queue<T>();

    /// <summary>
    /// gets an object from the pool or creates a new one
    /// </summary>
    /// <returns></returns>
    public virtual T GetAnObj()
    {
        T pickedObj = default(T);
        if (pooledObjs.Count > 0)
        {
			pickedObj = pooledObjs.Dequeue();
        }
        else {
            pickedObj = CreateNewObj();
        }

        activeObjs.Add(pickedObj);

        return pickedObj;
    }

	public abstract T CreateNewObj();

	/// <summary>
	/// moves target obj from the actives list to the pooleds list
	/// </summary>
	/// <param name="theObj"></param>
    public virtual void PoolObj(T theObj)
    {
        activeObjs.Remove(theObj);
        pooledObjs.Enqueue(theObj);
    }

	public virtual void PoolAllObjs() {
		while(activeObjs.Count > 0) {
			PoolObj(activeObjs[0]);
		}
	}
}