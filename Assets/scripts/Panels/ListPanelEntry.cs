using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an entry in one of the panels that contain a list of entries
/// </summary>
public abstract class ListPanelEntry<T> : MonoBehaviour {

    public T myContent;

    public virtual void SetContent(T theContent)
    {
        myContent = theContent;
    }
}
