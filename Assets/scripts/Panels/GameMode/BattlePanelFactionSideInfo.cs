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

	/// <summary>
	/// combined army of all containers of this side in the battle
	/// </summary>
	public List<TroopNumberPair> sideArmy = new List<TroopNumberPair>();

	public List<TroopContainer> ourContainers = new List<TroopContainer>();

	public BattlePanel bPanel;

	public CanvasGroup ourGroup;

	private float initialArmyPower;

	public float curArmyPower;

	public bool depletingBar = false;

	public const float BAR_DEPLETION_TIME = 0.8f, SIDE_DEFEATED_FADE_TIME = 1.0f, SIDE_DEFEATED_NOTIFY_DELAY = 1.0f;

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
	/// sets the displayed and stored army data, such as autocalc power and numbers.
	/// providing the zone separated from the rest of our containers will affect the description
	/// of forces only
	/// </summary>
	/// <param name="ourZone"></param>
	/// <param name="containers"></param>
	/// <param name="postBattle"></param>
	public void SetArmyData(Zone ourZone, List<TroopContainer> containers) {
		ourContainers.Clear();
		ourContainers.AddRange(containers);
		sideArmy.Clear();
		int armyNumbers = 0;
		int armyCmders = 0;
		float armyPower = 0;
		string forceDescription = "";
		if (ourZone != null) {

			if (!ourContainers.Contains(ourZone)) {//if our zone wasn't added in ourContainers already...
				ourContainers.Add(ourZone);
				armyCmders--; 
			}
			
			forceDescription = "Garrison";
			
		}

		foreach(TroopContainer cont in ourContainers) {
			armyCmders++;
			armyNumbers += cont.TotalTroopsContained;
			armyPower += cont.TotalAutocalcPower;
			sideArmy = cont.GetCombinedTroops(sideArmy);
		}

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
		factionAutocalcPowerTxt.text = "Power: " + armyPower.ToString(CultureInfo.InvariantCulture);
		initialArmyPower = armyPower;

	}

	public void UpdatePostBattleArmy() { 
		sideArmy.Clear();
		int armyNumbers = 0;
		float armyPower = 0;

		foreach (TroopContainer cont in ourContainers) {
			armyNumbers += cont.TotalTroopsContained;
			armyPower += cont.TotalAutocalcPower;
			sideArmy = cont.GetCombinedTroops(sideArmy);
		}

		factionTroopNumbersTxt.text = armyNumbers.ToString() + " Troops";
		factionAutocalcPowerTxt.text = "Power: " + armyPower.ToString(CultureInfo.InvariantCulture);

		curArmyPower = armyPower;
		StartCoroutine(DepletePowerBarAccordingToPower(initialArmyPower, curArmyPower));
	}

	/// <summary>
	/// distributes losses among troop containers and calls the power bar depletion routine
	/// </summary>
	/// <param name="remainingArmy"></param>
	public void SetPostBattleArmyData(List<TroopNumberPair> remainingArmy) {
		float lossPercent = 0;
		int initialTroopAmount = 0;
		foreach(TroopNumberPair tnp in remainingArmy) {
			initialTroopAmount = sideArmy[GameController.IndexOfTroopInTroopList
				(sideArmy, tnp.troopTypeID)].troopAmount;
			lossPercent = 1.0f - ((float) tnp.troopAmount / initialTroopAmount);
			RemoveTroopByPercentageInAllConts(tnp.troopTypeID, initialTroopAmount,
				lossPercent, tnp.troopAmount);
		}

		UpdatePostBattleArmy();
	}

	/// <summary>
	/// distributes losses among troop containers and calls the power bar depletion routine
	/// </summary>
	/// <param name="remainingPercent"></param>
	public void SetPostBattleArmyData(float remainingPercent) {
		Debug.Log("set remaining to " + remainingPercent);
		float lossPercent = 1.0f - remainingPercent;
		int initialTroopAmount = 0;
		foreach (TroopNumberPair tnp in sideArmy) {
			initialTroopAmount = sideArmy[GameController.IndexOfTroopInTroopList
				(sideArmy, tnp.troopTypeID)].troopAmount;
			RemoveTroopByPercentageInAllConts(tnp.troopTypeID, initialTroopAmount,
				lossPercent);
		}

		UpdatePostBattleArmy();
	}

	/// <summary>
	/// autocalc resolution of army data
	/// </summary>
	public void SetPostBattleArmyData() {
		//TODO autocalc!
	}

	public void RemoveTroopByPercentageInAllConts(int troopID, int initialTotalTroopAmount,
		float lossPercent, int knownRemainingTroops = -1) {
		Debug.Log("remove this soldier " + GameController.GetTroopTypeByID(troopID).name);
		Debug.Log("losspercent " + lossPercent);
		int troopIndexInCurContainer = -1;
		int totalRemovedTroops = 0;
		int removedTroopsFromCurContainer = 0;
		TroopNumberPair affectedPair;
		foreach (TroopContainer tContainer in ourContainers) {
			troopIndexInCurContainer = tContainer.IndexOfTroopInContainer(troopID);
			if (troopIndexInCurContainer >= 0) {
				affectedPair = tContainer.troopsContained[troopIndexInCurContainer];
				removedTroopsFromCurContainer = Mathf.RoundToInt(affectedPair.troopAmount * lossPercent);
				tContainer.RemoveTroop(troopID, removedTroopsFromCurContainer);
				totalRemovedTroops += removedTroopsFromCurContainer;
			}
		}

		Debug.Log("total removed troops: " + totalRemovedTroops);
		if(knownRemainingTroops >= 0 &&
			(initialTotalTroopAmount - totalRemovedTroops) != knownRemainingTroops) {
			//rounding caused some error
			Debug.LogWarning("rounding error in loss calculation : known remaining is " + 
				knownRemainingTroops + ", actual remaining is " + 
				(initialTotalTroopAmount - totalRemovedTroops));

		}
	}

	/// <summary>
	/// depletes power bars and fades this side out if it now has 0 power
	/// </summary>
	/// <param name="initialPower"></param>
	/// <param name="curPower"></param>
	/// <returns></returns>
	public IEnumerator DepletePowerBarAccordingToPower(float initialPower, float curPower) {
		float elapsedTime = 0, initialFillAmount = factionPowerBarContent.fillAmount;
		Debug.Log("init power: " + initialPower + ", curPOwer: " + curPower);
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
			bPanel.OnBattleResolved();
		}

		depletingBar = false;
		bPanel.OnOneSideDoneAnimating();
		
	}
}
