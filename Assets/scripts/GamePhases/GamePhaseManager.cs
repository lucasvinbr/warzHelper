using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePhaseManager : MonoBehaviour {

	public abstract void OnPhaseStart();

	/// <summary>
	/// starts the nextPhase coroutine
	/// </summary>
	public virtual void OnPhaseEnd() {
		StartCoroutine(ProceedToNextPhaseRoutine());
	}

	/// <summary>
	/// after a while calls the gamemodehandler's GoToNextPhase
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator ProceedToNextPhaseRoutine() {
		yield return new WaitForSeconds(1.35f);
		GameModeHandler.instance.GoToNextTurnPhase();
	}
}
