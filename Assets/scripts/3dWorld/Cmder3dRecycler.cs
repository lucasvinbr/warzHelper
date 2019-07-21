using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cmder3dRecycler : ScriptedPrefabRecyclerUser<Cmder3d> {

    public static Cmder3dRecycler instance;

    protected override void Awake()
    {
		base.Awake();
        instance = this;
    }

    public static Cmder3d GetACmderObj()
    {
        return instance.cycler.GetScriptedObj();
    }
}
