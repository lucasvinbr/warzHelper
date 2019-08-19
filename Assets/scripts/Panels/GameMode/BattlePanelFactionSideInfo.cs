using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class BattlePanelFactionSideInfo : ListPanelEntry<Faction> {

    public TMP_Text factionNameTxt, factionForcesDescTxt, factionTroopNumbersTxt, factionAutocalcPowerTxt;

    public Image factionImg;

	public Image factionPowerBarContent;

	public BattlePanel bPanel;

	public CanvasGroup ourGroup;

	public bool depletingBar = false;

	public const float BAR_DEPLETION_TIME = 0.8f, SIDE_DEFEATED_FADE_TIME = 0.6f, SIDE_DEFEATED_NOTIFY_DELAY = 0.4f;

	public BattleFactionSideInfo sideInfo;

	public override void SetContent(Faction theContent)
    {
        base.SetContent(theContent);

        factionNameTxt.text = theContent.name;
		factionPowerBarContent.color = theContent.color;
		factionPowerBarContent.fillAmount = 1.0f;
		ourGroup.alpha = 1.0f;
		if (string.IsNullOrEmpty(theContent.iconPath)) {
			factionImg.gameObject.SetActive(false);
		}
		else {
			factionImg.gameObject.SetActive(true);
			//TODO set image and all that
		}
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
		

		armyCmders = sideInfo.ourContainers.Count;

		if (ourZone != null) {
			forceDescription = "Garrison";
			armyCmders--; //prevents counting the zone as a commander
		}

		armyNumbers = sideInfo.curArmyNumbers;
		armyPower = sideInfo.curArmyPower;

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
		factionTroopNumbersTxt.text = sideInfo.curArmyNumbers.ToString() + " Troops";
		factionAutocalcPowerTxt.text = "Power: " + sideInfo.curArmyPower.ToString("0.00", CultureInfo.InvariantCulture);

		if(startDepletePowerBarRoutine || sideInfo.curArmyPower <= 0)
			StartCoroutine(DepletePowerBarAccordingToPower(sideInfo.initialArmyPower, sideInfo.curArmyPower));
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
