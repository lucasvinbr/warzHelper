using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkLineRecycler : Recycler<LinkLine> {

    public static LinkLineRecycler instance;

    public GameObject linePrefab;

    public Transform linesParent;

    void Awake()
    {
        instance = this;
    }

    public static LinkLine GetALine()
    {
        return instance.GetAnObj();
    }

    public override LinkLine CreateNewObj()
    {
        GameObject newLine = GameObject.Instantiate(linePrefab);
        newLine.transform.SetParent(linesParent);
        return newLine.GetComponent<LinkLine>();
    }
}
