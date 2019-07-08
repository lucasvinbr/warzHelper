using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduledOrdersVisualFeedbacks : MonoBehaviour
{

	public GameObject trainFeedbackPrefab, recruitFeedbackPrefab;

	List<GameObject> activeTrainFeedbacks = new List<GameObject>();
	List<GameObject> activeRecruitFeedbacks = new List<GameObject>();


	//TODO all methods... like "addFeedback(orderType, cmder)", "clearAllFeedbacks()" - this one is useful for the 'action time' that comes after unified turns
	//pooling for train and recruit feedbacks, linkLineRecycler should take care of the lines (make it handle colors if necessary)
}
