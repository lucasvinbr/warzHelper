using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// the representation of a zone in the 3d world
/// </summary>
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

	public void DeleteThisSpot() {
		Destroy(gameObject);

	}

    public void RefreshDataDisplay()
    {
        if(data != null)
        {
            myLabel.SetText(data.name);
        }
    }

}
