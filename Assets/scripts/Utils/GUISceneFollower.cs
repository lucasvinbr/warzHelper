using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GUISceneFollower : MonoBehaviour
{

    protected Camera cam;
    protected Camera Cam
    {
        get
        {
            if (!cam)
            {
                cam = Camera.main;
            }
            return cam;
        }
    }
    public Transform FollowThis;

    protected void Update()
    {
        if (FollowThis == null)
        {
            return;
        }

        Vector2 sp = Cam.WorldToScreenPoint(FollowThis.position);

        this.transform.position = sp;
    }

    protected void OnEnable()
    {
        // this is here because there can be a single frame where the position is incorrect
        // when the object (or its parent) is activated.
        if (gameObject.activeInHierarchy)
        {
            Update();
        }
    }
}