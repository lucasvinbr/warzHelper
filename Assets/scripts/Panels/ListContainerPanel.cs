﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a panel that contains multiple entries in a scroll view.
/// this entry list is cleared and filled again every time this panel is opened
/// </summary>
public class ListContainerPanel<T> : MonoBehaviour {

    public Transform listContainer;

    public GameObject entryPrefab, listIsEmptyObject;

    public virtual GameObject AddEntry(T entryData)
    {
        GameObject newEntry = Instantiate(entryPrefab);
        newEntry.transform.SetParent(listContainer, false);
        newEntry.GetComponent<ListPanelEntry<T>>().SetContent(entryData);

        if(listIsEmptyObject) listIsEmptyObject.SetActive(false);

		return newEntry;
    }

    public virtual void ClearList()
    {
        for (int i = 0; i < listContainer.childCount; i++)
        {
            Destroy(listContainer.GetChild(i).gameObject);
        }

		if (listIsEmptyObject) listIsEmptyObject.SetActive(true);
    }

    
	/// <summary>
	/// calls ClearList
	/// </summary>
    public virtual void OnEnable()
    {
        ClearList();
    }
}
