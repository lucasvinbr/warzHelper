using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script handles the moving tweens of cmder3ds and other 3d objects, using recyclable "TransformTween" objs.
/// The data is (should be) set before the tween begins, so this is merely visual.
/// </summary>
public class TransformTweener : MonoBehaviour
{

	List<TransformTween> activeTweens = new List<TransformTween>();

	List<TransformTween> inactiveTweens = new List<TransformTween>();

	/// <summary>
	/// the time the commander 3d takes to reach their destination when moving.
	/// Merely visual, the data involved in the movement is changed immediately
	/// </summary>
	public const float TWEEN_ANIM_MOVE_DURATION = 0.65f;

	public class TransformTween {
		public Transform movingTrans;
		public ZoneSpot destSpot;
		public Vector3 destPos;
		public float elapsedTweenTime = 0f;
		public bool cmderAdjusted = false;

		/// <summary>
		/// makes the tween ready to run again
		/// </summary>
		/// <param name="cmder3d"></param>
		/// <param name="destSpot"></param>
		public void ResetData(Transform transf, ZoneSpot destSpot, bool cmderAdjusted = false) {
			movingTrans = transf;
			this.destSpot = destSpot;
			this.cmderAdjusted = cmderAdjusted;
			elapsedTweenTime = 0;
		}
	}


	public static TransformTweener instance;

	private void Awake() {
		instance = this;
	}

	/// <summary>
	/// starts a tween to the target spot, optionally setting the destination position to a "GoodSpotForCmder"
	/// </summary>
	/// <param name="trans"></param>
	/// <param name="destSpot"></param>
	/// <param name="cmderAdjusted"></param>
	public void StartTween(Transform trans, ZoneSpot destSpot, bool cmderAdjusted) {
		TransformTween newTween = GetATween();
		newTween.ResetData(trans, destSpot, cmderAdjusted);
		activeTweens.Add(newTween);
		StartCoroutine(TweenRoutine(newTween));
	}

	IEnumerator TweenRoutine(TransformTween tween) {
		Vector3 originalCmderPos = tween.movingTrans.transform.position;
		tween.destPos = tween.cmderAdjusted ? tween.destSpot.GetGoodSpotForCommander(
			GameController.GetCommandersInZone(tween.destSpot.data as Zone).Count - 1) : 
			tween.destSpot.transform.position;

		while (tween.elapsedTweenTime < TWEEN_ANIM_MOVE_DURATION) {
			tween.movingTrans.transform.position =
				Vector3.Slerp(originalCmderPos, tween.destPos,
				tween.elapsedTweenTime / TWEEN_ANIM_MOVE_DURATION);
			tween.elapsedTweenTime += Time.deltaTime;
			yield return null;
		}

		tween.movingTrans.transform.position = tween.destPos;
		activeTweens.Remove(tween);
		inactiveTweens.Add(tween);
	}


	/// <summary>
	/// If a commander leaves a zone while another is tweening towards it,
	/// the one tweening will have a "wrong" destination and might
	/// overlap with another commander, so we adjust the moving commanders'
	/// destinations. (This is only for cmder-adjusted tweens)
	/// </summary>
	/// <param name="spotWithChanges"></param>
	public void AdjustTweensThatTargetZone(ZoneSpot spotWithChanges) {
		int adjustmentCount = 1;
		int cmdersInSpot = GameController.GetCommandersInZone(spotWithChanges.data as Zone).Count;
		foreach (TransformTween tween in activeTweens) {
			if (!tween.cmderAdjusted) continue;
			if(tween.destSpot == spotWithChanges) {
				tween.destPos = spotWithChanges.GetGoodSpotForCommander(
					cmdersInSpot - adjustmentCount);
				adjustmentCount++;
			}
		}
	}

	public List<TransformTween> GetAllTweensTargetingZone(ZoneSpot targetSpot) {
		List<TransformTween> returnedList = new List<TransformTween>();
		foreach (TransformTween tween in activeTweens) {
			if (tween.destSpot == targetSpot) {
				returnedList.Add(tween);
			}
		}

		return returnedList;
	}


	/// <summary>
	/// interrupts all tweens and puts them into the inactive list
	/// </summary>
	public void StopAllTweens() {
		StopAllCoroutines();
		inactiveTweens.AddRange(activeTweens);
		activeTweens.Clear();
	}

	/// <summary>
	/// recycles or creates a new tween
	/// </summary>
	/// <returns></returns>
	public TransformTween GetATween() {
		TransformTween returnedTween = null;
		if(inactiveTweens.Count > 0) {
			returnedTween = inactiveTweens[0];
			inactiveTweens.RemoveAt(0);
		}else {
			returnedTween = new TransformTween();
		}

		return returnedTween;
	}

}
