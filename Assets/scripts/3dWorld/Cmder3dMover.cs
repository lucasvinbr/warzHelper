using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this script handles the moving tweens of cmder3ds, using recyclable "CmderTween" objs.
/// The data is set before the tween begins, so this is merely visual.
/// </summary>
public class Cmder3dMover : MonoBehaviour
{

	List<CmderTween> activeTweens = new List<CmderTween>();

	List<CmderTween> inactiveTweens = new List<CmderTween>();

	/// <summary>
	/// the time the commander 3d takes to reach their destination when moving.
	/// Merely visual, the data involved in the movement is changed immediately
	/// </summary>
	public const float CMDER3D_ANIM_MOVE_DURATION = 0.65f;

	public class CmderTween {
		public Cmder3d movingCmder;
		public ZoneSpot destSpot;
		public Vector3 destPos;
		public float elapsedTweenTime = 0f;


		/// <summary>
		/// makes the tween ready to run again
		/// </summary>
		/// <param name="cmder3d"></param>
		/// <param name="destSpot"></param>
		public void ResetData(Cmder3d cmder3d, ZoneSpot destSpot) {
			movingCmder = cmder3d;
			this.destSpot = destSpot;
			elapsedTweenTime = 0;
		}
	}


	public static Cmder3dMover instance;

	private void Awake() {
		instance = this;
	}

	public void StartTween(Cmder3d cmder3d, ZoneSpot destSpot) {
		CmderTween newTween = GetATween();
		newTween.ResetData(cmder3d, destSpot);
		activeTweens.Add(newTween);
		StartCoroutine(TweenRoutine(newTween));
	}

	IEnumerator TweenRoutine(CmderTween tween) {
		Vector3 originalCmderPos = tween.movingCmder.transform.position;
		tween.destPos = tween.destSpot.GetGoodSpotForCommander(
			GameController.GetCommandersInZone(tween.destSpot.data).Count - 1);

		while (tween.elapsedTweenTime < CMDER3D_ANIM_MOVE_DURATION) {
			tween.movingCmder.transform.position =
				Vector3.Slerp(originalCmderPos, tween.destPos,
				tween.elapsedTweenTime / CMDER3D_ANIM_MOVE_DURATION);
			tween.elapsedTweenTime += Time.deltaTime;
			yield return null;
		}

		tween.movingCmder.transform.position = tween.destPos;
		activeTweens.Remove(tween);
		inactiveTweens.Add(tween);
	}


	/// <summary>
	/// If a commander leaves a zone while another is tweening towards it,
	/// the one tweening will have a "wrong" destination and might
	/// overlap with another commander, so we adjust the moving commanders'
	/// destinations
	/// </summary>
	/// <param name="spotWithChanges"></param>
	public void AdjustTweensThatTargetZone(ZoneSpot spotWithChanges) {
		int adjustmentCount = 1;
		int cmdersInSpot = GameController.GetCommandersInZone(spotWithChanges.data).Count;
		foreach (CmderTween tween in activeTweens) {
			if(tween.destSpot == spotWithChanges) {
				tween.destPos = spotWithChanges.GetGoodSpotForCommander(
					cmdersInSpot - adjustmentCount);
				adjustmentCount++;
			}
		}
	}

	public List<CmderTween> GetAllTweensTargetingZone(ZoneSpot targetSpot) {
		List<CmderTween> returnedList = new List<CmderTween>();
		foreach (CmderTween tween in activeTweens) {
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
	public CmderTween GetATween() {
		CmderTween returnedTween = null;
		if(inactiveTweens.Count > 0) {
			returnedTween = inactiveTweens[0];
			inactiveTweens.RemoveAt(0);
		}else {
			returnedTween = new CmderTween();
		}

		return returnedTween;
	}

}
