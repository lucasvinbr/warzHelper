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
            myLabel.FollowThis = labelPoint;
            RefreshDataDisplay();
        }
    }

    void OnDestroy()
    {
        if (myLabel)
        {
            FollowerTextCanvasRecycler.instance.PoolObj(myLabel);
        }
    }

    public void RefreshDataDisplay()
    {
        if(data != null)
        {
            myLabel.SetText(data.name);
        }
    }

}
