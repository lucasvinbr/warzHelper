using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldVisualFeedbacks : MonoBehaviour
{
	public static WorldVisualFeedbacks instance;

	public SimplePrefabRecycler recruitFBCycler, trainingFBCycler, zoneHighlightCycler;


	private void Awake() {
		instance = this;
	}

	//TODO all methods... like "addFeedback(orderType, cmder)", "clearAllFeedbacks()" - this one is useful for the 'action time' that comes after unified turns
	//pooling for train and recruit feedbacks, linkLineRecycler should take care of the lines (make it handle colors if necessary)
}