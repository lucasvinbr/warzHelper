using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldVisualFeedbacks : MonoBehaviour
{
	public static WorldVisualFeedbacks instance;

	public SimplePrefabRecycler recruitFBCycler, trainingFBCycler, moveFBCycler, createCmderFBCycler, zoneHighlightCycler;

	public void PoolAllOrderFeedbacks() {
		recruitFBCycler.PoolAllObjs();
		trainingFBCycler.PoolAllObjs();
		moveFBCycler.PoolAllObjs();
		createCmderFBCycler.PoolAllObjs();
	}

	private void Awake() {
		instance = this;
	}

}