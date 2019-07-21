using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowerTextCanvasRecycler : ScriptedPrefabRecyclerUser<GUIFollowerText> {

    public static FollowerTextCanvasRecycler instance;

    protected override void Awake()
    {
		base.Awake();
        instance = this;
    }

    public static GUIFollowerText GetAFollower()
    {
        return instance.cycler.GetScriptedObj();
    }
}
