using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTextCanvasRecycler : Recycler<GUIFollowerText> {

    public static FollowerTextCanvasRecycler instance;

    public GameObject followerPrefab;

    public Transform followerTextsParent;

    void Awake()
    {
        instance = this;
    }

    public static GUIFollowerText GetAFollower()
    {
        return instance.GetAnObj();
    }

    public override GUIFollowerText CreateNewObj()
    {
        GameObject newFollowerText = GameObject.Instantiate(followerPrefab);
        newFollowerText.transform.SetParent(followerTextsParent);
        return newFollowerText.GetComponent<GUIFollowerText>();
    }
}
