using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GamePhaseManager : MonoBehaviour {

	public abstract void OnPhaseStart();

	/// <summary>
	/// starts the nextPhase coroutine
	/// </summary>
	public virtual void OnPhaseEnd(bool noWait = false) {
		StartCoroutine(ProceedToNextPhaseRoutine(noWait));
	}

	/// <summary>
	/// after a while calls the gamemodehandler's GoToNextPhase
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator ProceedToNextPhaseRoutine(bool noWait = false) {
		if (noWait) {
			//very small time just to make it stop if a panel is open
			yield return WaitWhileNoOverlays(0.01f);
		}
		else {
			yield return WaitWhileNoOverlays(0.7f);
		}
		
		GameModeHandler.instance.GoToNextTurnPhase();
	}

	/// <summary>
	/// waits for the desired time, but the time counting stops when there is an overlay menu open
	/// (overlay level greater than 0)
	/// </summary>
	/// <param name="desiredTime"></param>
	/// <returns></returns>
	public IEnumerator WaitWhileNoOverlays(float desiredTime) {
		float elapsedTime = 0;

		while(elapsedTime < desiredTime) {
			if(GameInterface.openedPanelsOverlayLevel <= 0) {
				elapsedTime += Time.deltaTime;
			}

			yield return null;
		}
	}
}
