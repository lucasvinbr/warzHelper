using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkLineRecycler : ScriptedPrefabRecyclerUser<LinkLine>  {

	public static LinkLineRecycler instance;

    protected override void Awake()
    {
		base.Awake();
		instance = this;
    }

    public static LinkLine GetALine()
    {
        return instance.cycler.GetScriptedObj();
    }
}
