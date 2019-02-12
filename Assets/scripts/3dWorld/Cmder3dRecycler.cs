using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cmder3dRecycler : Recycler<Cmder3d> {

    public static Cmder3dRecycler instance;

    public GameObject cmder3dPrefab;

    public Transform spawnedCmdersParent;

    void Awake()
    {
        instance = this;
    }

    public static Cmder3d GetACmderObj()
    {
        return instance.GetAnObj();
    }

    public override Cmder3d CreateNewObj()
    {
        GameObject newCmd = Instantiate(cmder3dPrefab);
        newCmd.transform.SetParent(spawnedCmdersParent);
        return newCmd.GetComponent<Cmder3d>();
    }
}
