using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoneSpot : MonoBehaviour {

    public Zone data;

    public GUIFollowerText myLabel;

    public Transform labelPoint;

    void Start()
    {
        if (!myLabel)
        {
            myLabel = FollowerTextCanvasRecycler.GetAFollower();
            myLabel.SetText(data.name);
            myLabel.FollowThis = labelPoint;
        }
    }

    void OnDestroy()
    {
        if (myLabel)
        {
            FollowerTextCanvasRecycler.instance.PoolObj(myLabel);
        }
    }

}
