using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePhaseManager : MonoBehaviour {

	public abstract void OnPhaseStart();

	public virtual void OnPhaseEnd() {
		StartCoroutine(ProceedToNextPhaseRoutine());
	}

	public virtual IEnumerator ProceedToNextPhaseRoutine() {
		yield return new WaitForSeconds(0.8f);
		GameModeHandler.instance.GoToNextTurnPhase();
	}
}
