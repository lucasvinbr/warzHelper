using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiploSendMsgTargetListEntry : FactionsRelationPanelListEntry {

	public void OnSelectedAsTarget() {
		GameInterface.instance.diploMsgOpsPanel.OpenIfPossible(thePanelsCurFaction, myContent);
	}

}
