using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;
using System.Globalization;

public class BattlePanelFactionSideInfo : ListPanelEntry<BattleFactionSideInfo> {

    public TMP_Text factionNameTxt, factionForcesDescTxt, factionTroopNumbersTxt, factionAutocalcPowerTxt;

	public TooltipTrigger facNamesTooltipTrigger;

    public Image factionImg;

	public Image factionPowerBarContent;

	public BattlePanel bPanel;

	public CanvasGroup ourGroup;

	public bool depletingBar = false;

	public const float BAR_DEPLETION_TIME = 0.8f, SIDE_DEFEATED_FADE_TIME = 0.6f, SIDE_DEFEATED_NOTIFY_DELAY = 0.4f;

	public override void SetContent(BattleFactionSideInfo theContent)
    {
        base.SetContent(theContent);

		//TODO figure out the main attacking faction by picking the one with the most cmders or something like that?
		Faction baseSideFaction = theContent.sideFactions.Count > 0 ? theContent.sideFactions[0] : null;

		if(baseSideFaction != null) {
			factionNameTxt.text = baseSideFaction.name;

			if (theContent.sideFactions.Count > 1) {
				factionImg.gameObject.SetActive(false);

				factionNameTxt.text = string.Concat(factionNameTxt.text, "\n + ",
					(theContent.sideFactions.Count - 1).ToString(), " other(s)");

				facNamesTooltipTrigger.enabled = true;
				facNamesTooltipTrigger.text = "";

				for(int i = 1; i < theContent.sideFactions.Count; i++) {
					facNamesTooltipTrigger.text = string.Concat
						(facNamesTooltipTrigger.text, theContent.sideFactions[i].name,
						i < theContent.sideFactions.Count - 1 ? "\n" : "");
				}
			}
			else {
				facNamesTooltipTrigger.enabled = false;

				if (string.IsNullOrEmpty(baseSideFaction.iconPath)) {
					factionImg.gameObject.SetActive(false);
				}
				else {
					factionImg.gameObject.SetActive(true);
					//TODO set image and all that
				}
			}

			factionPowerBarContent.color = baseSideFaction.color;
		}
		else {
			factionNameTxt.text = Rules.NO_FACTION_NAME;
			factionPowerBarContent.color = Color.white;
			factionImg.gameObject.SetActive(false);
			facNamesTooltipTrigger.enabled = false;
		}

        
		factionPowerBarContent.fillAmount = 1.0f;
		ourGroup.alpha = 1.0f;
		
	}

	/// <summary>
	/// sets the displayed army data, such as autocalc power and numbers.
	/// providing the zone separated from the rest of our containers will affect the description
	/// of forces only
	/// </summary>
	/// <param name="ourZone"></param>
	/// <param name="containers"></param>
	/// <param name="postBattle"></param>
	public void SetArmyDataDisplay(Zone ourZone) {
		int armyNumbers = 0;
		int armyCmders = 0;
		float armyPower = 0;
		string forceDescription = "";
		

		armyCmders = myContent.ourContainers.Count;

		if (ourZone != null) {
			forceDescription = "Garrison";
			armyCmders--; //prevents counting the zone as a commander
		}

		armyNumbers = myContent.curArmyNumbers;
		armyPower = myContent.curArmyPower;

		if(armyCmders > 0) {

			if (ourZone != null) {
				forceDescription += ", ";
			}

			forceDescription += armyCmders.ToString() + " Commander";
			if(armyCmders > 1) {
				forceDescription += "s";
			}
		}

		factionForcesDescTxt.text = forceDescription;
		factionTroopNumbersTxt.text = armyNumbers.ToString() + " Troops";
		factionAutocalcPowerTxt.text = "Power: " + armyPower.ToString("0.00", CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// updates the army's data display.
	/// the bar routine is optional unless the side's power is 0;
	/// it will always run in that case
	/// </summary>
	/// <param name="startDepletePowerBarRoutine"></param>
	public void UpdatePostBattleArmyDisplay(bool startDepletePowerBarRoutine = true) { 
		factionTroopNumbersTxt.text = myContent.curArmyNumbers.ToString() + " Troops";
		factionAutocalcPowerTxt.text = "Power: " + myContent.curArmyPower.ToString("0.00", CultureInfo.InvariantCulture);

		if(startDepletePowerBarRoutine || myContent.curArmyPower <= 0)
			StartCoroutine(DepletePowerBarAccordingToPower(myContent.initialArmyPower, myContent.curArmyPower));
	}

	/// <summary>
	/// depletes power bars and fades this side out if it now has 0 power
	/// </summary>
	/// <param name="initialPower"></param>
	/// <param name="curPower"></param>
	/// <returns></returns>
	public IEnumerator DepletePowerBarAccordingToPower(float initialPower, float curPower) {
		float elapsedTime = 0, initialFillAmount = factionPowerBarContent.fillAmount;
		depletingBar = true;
		//prevent any changes while transition is running
		bPanel.resolutionBtnsGroup.interactable = false;

		while (elapsedTime <= BAR_DEPLETION_TIME) {
			factionPowerBarContent.fillAmount = Mathf.Lerp(initialFillAmount, curPower / initialPower,
				elapsedTime / BAR_DEPLETION_TIME);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		if(curPower == 0) {
			elapsedTime = 0;

			while (elapsedTime <= SIDE_DEFEATED_FADE_TIME) {
				ourGroup.alpha = Mathf.Lerp(1.0f, 0.0f,
					elapsedTime / SIDE_DEFEATED_FADE_TIME);
				elapsedTime += Time.deltaTime;
				yield return null;
			}

			//this side's been defeated! the battle's resolved then
			//notify the panel
			yield return new WaitForSeconds(SIDE_DEFEATED_NOTIFY_DELAY);
			bPanel.BattleResolved(this);
		}

		depletingBar = false;
		bPanel.OnOneSideDoneAnimating();
		
	}

	
}
