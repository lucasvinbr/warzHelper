using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// a class that affects the openedPanelsOverlayLevel of GameInterface
/// </summary>
public class GrowingOverlayPanel : GenericOverlayPanel {

	public float growTime = 0.5f, shrinkTime = 0.5f;

	protected float elapsedTime;

	protected bool transitioning = false;

	public override void OnEnable() {
		Grow();
		base.OnEnable();
	}

	public virtual void Grow(UnityAction actionOnDoneGrowing = null) {
		StartCoroutine(TransitionRoutine(true, actionOnDoneGrowing));
	}

	public virtual void Shrink(UnityAction actionOnDoneShrinking = null) {
		StartCoroutine(TransitionRoutine(false, actionOnDoneShrinking));
	}


	public virtual IEnumerator TransitionRoutine(bool growing, UnityAction actionOnDoneTransition = null) {
		if (transitioning) {
			Debug.LogWarning("[GrowingPanel] Attempted to start new transition while last one was still running. Aborting");
			yield break;
		}
		transitioning = true;
		elapsedTime = 0;
		Vector3 initialScale = growing ? Vector3.zero : Vector3.one;
		Vector3 targetScale = growing ? Vector3.one : Vector3.zero;
		float transitionDuration = growing ? growTime : shrinkTime;
		while(elapsedTime <= (growing ? growTime : shrinkTime)) {
			transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / transitionDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		actionOnDoneTransition?.Invoke();

		if (!growing) { //deactivate when done if was shrinking
			gameObject.SetActive(false);
		}

		transitioning = false;
	}
}
