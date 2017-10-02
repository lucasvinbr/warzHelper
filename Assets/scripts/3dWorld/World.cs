using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    public Transform ground;
    public Transform zonesContainer;

    public GameObject zonePrefab;

    public static World instance;

    void Awake()
    {
        instance = this;
    }

    public void ToggleWorldDisplay(bool active)
    {
        ground.gameObject.SetActive(active);
        zonesContainer.gameObject.SetActive(active);
    }

    public void CreateZone(Zone targetZone)
    {

    }
}
