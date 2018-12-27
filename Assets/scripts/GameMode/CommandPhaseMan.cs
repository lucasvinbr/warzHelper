using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommandPhaseMan : GamePhaseManager {

	public Button getIdleCmderBtn;

	public CanvasGroup selectedCmderBtnsGroup;

	public override void OnPhaseStart() {
		//check if we've got any cmder to actually command
		Faction playerFac = GameModeHandler.instance.curPlayingFaction;
		List<Commander> factionCmders = playerFac.OwnedCommanders;
		//TODO actually command things
		OnPhaseEnd();
	}


}
