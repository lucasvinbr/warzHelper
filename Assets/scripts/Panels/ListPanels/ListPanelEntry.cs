using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an entry in one of the panels that contain a list of entries.
/// Doesn't really need to be part of a list, but has code commonly used by those
/// </summary>
public abstract class ListPanelEntry<T> : MonoBehaviour {

    public T myContent;

    public virtual void SetContent(T theContent)
    {
        myContent = theContent;
    }
}
