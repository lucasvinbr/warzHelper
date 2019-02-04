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

	public int curArmyNumbers = 0;

	public bool depletingBar = false;

	public const float BAR_DEPLETION_TIME = 0.8f, SIDE_DEFEATED_FADE_TIME = 1.0f, SIDE_DEFEATED_NOTIFY_DELAY = 1.0f;

	/// <summary>
	/// if this army is defeated, it will award this amount of points to the other side
	/// </summary>
	public int pointsAwardedToVictor = 0;

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
		pointsAwardedToVictor = 0;
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
		curArmyNumbers = armyNumbers;
		factionTroopNumbersTxt.text = armyNumbers.ToString() + " Troops";
		factionAutocalcPowerTxt.text = "Power: " + armyPower.ToString(CultureInfo.InvariantCulture);
		initialArmyPower = armyPower;
		curArmyPower = armyPower;
	}

	/// <summary>
	/// updates the army's data and display.
	/// the bar routine is optional unless the side's power is 0;
	/// it will always run in that case
	/// </summary>
	/// <param name="startDepletePowerBarRoutine"></param>
	public void UpdatePostBattleArmy(bool startDepletePowerBarRoutine = true) { 
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

		curArmyNumbers = armyNumbers;
		curArmyPower = armyPower;
		if(startDepletePowerBarRoutine || curArmyPower <= 0)
			StartCoroutine(DepletePowerBarAccordingToPower(initialArmyPower, curArmyPower));
	}

	/// <summary>
	/// distributes losses among troop containers and optionally calls the power bar depletion routine
	/// </summary>
	/// <param name="remainingArmy"></param>
	public void SetPostBattleArmyData_RemainingArmy(List<TroopNumberPair> remainingArmy, bool depleteBarRoutine = true) {
		float lossPercent = 0;
		int initialTroopAmount = 0;
		int pointsLost = 0;
		foreach(TroopNumberPair tnp in remainingArmy) {
			initialTroopAmount = sideArmy[GameController.IndexOfTroopInTroopList
				(sideArmy, tnp.troopTypeID)].troopAmount;
			lossPercent = 1.0f - ((float) tnp.troopAmount / initialTroopAmount);
			pointsLost += RemoveTroopByPercentageInAllConts(tnp.troopTypeID, initialTroopAmount,
				lossPercent, tnp.troopAmount);
		}

		pointsAwardedToVictor += Mathf.RoundToInt(pointsLost *
			GameController.instance.curData.rules.battleVictorPointAwardFactor);
		UpdatePostBattleArmy(depleteBarRoutine);
	}

	/// <summary>
	/// distributes losses among troop containers and optionally calls the power bar depletion routine
	/// </summary>
	/// <param name="remainingPercent"></param>
	public void SetPostBattleArmyData_RemainingPercent(float remainingPercent, bool depleteBarRoutine = true) {
		float lossPercent = Mathf.Clamp(1.0f - remainingPercent, 0.0f, 1.0f);
		int initialTroopAmount = 0;
		int pointAward = 0;
		foreach (TroopNumberPair tnp in sideArmy) {
			initialTroopAmount = sideArmy[GameController.IndexOfTroopInTroopList
				(sideArmy, tnp.troopTypeID)].troopAmount;
			pointAward += RemoveTroopByPercentageInAllConts(tnp.troopTypeID, initialTroopAmount,
				lossPercent);
		}

		pointsAwardedToVictor += Mathf.RoundToInt(pointAward * 
			GameController.instance.curData.rules.battleVictorPointAwardFactor);
		UpdatePostBattleArmy(depleteBarRoutine);
	}

	/// <summary>
	/// converts the power lost to a percentage of troops lost,
	/// then runs the "set post battle data" according to that percentage.
	/// Also optionally calls the power bar depletion routine
	/// </summary>
	public void SetPostBattleArmyData_PowerLost(float powerLost, bool depleteBarRoutine = true) {
		float remainingPercentage = Mathf.Max(0.0f, (curArmyPower - powerLost) / curArmyPower);

		SetPostBattleArmyData_RemainingPercent(remainingPercentage, depleteBarRoutine);
	}

	/// <summary>
	/// returns the points awarded to the other side for our losses 
	/// (The lost troop's point cost is converted to points for the victor)
	/// </summary>
	/// <param name="troopID"></param>
	/// <param name="initialTotalTroopAmount"></param>
	/// <param name="lossPercent"></param>
	/// <param name="knownRemainingTroops"></param>
	/// <returns></returns>
	public int RemoveTroopByPercentageInAllConts(int troopID, int initialTotalTroopAmount,
		float lossPercent, int knownRemainingTroops = -1) {
		int troopIndexInCurContainer = -1;
		int totalRemovedTroops = 0;
		int removedTroopsFromCurContainer = 0;
		int pointLostPerTroop = GameController.GetTroopTypeByID(troopID).pointCost;
		int pointAward = 0;
		TroopNumberPair affectedPair;
		foreach (TroopContainer tContainer in ourContainers) {
			troopIndexInCurContainer = tContainer.IndexOfTroopInContainer(troopID);
			if (troopIndexInCurContainer >= 0) {
				affectedPair = tContainer.troopsContained[troopIndexInCurContainer];
				removedTroopsFromCurContainer = Mathf.RoundToInt(affectedPair.troopAmount * lossPercent);
				tContainer.RemoveTroop(troopID, removedTroopsFromCurContainer);
				pointAward += pointLostPerTroop * removedTroopsFromCurContainer;
				totalRemovedTroops += removedTroopsFromCurContainer;
			}
		}

		if(knownRemainingTroops >= 0 &&
			(initialTotalTroopAmount - totalRemovedTroops) != knownRemainingTroops) {
			//rounding caused some error!
			//adjust manually
			pointAward += AdjustTroopAmountInConts(troopID,
				knownRemainingTroops - (initialTotalTroopAmount - totalRemovedTroops));
		}

		return pointAward;
	}

	/// <summary>
	/// adds (or removes) troops to any of the containers, returning the points awarded in the process
	/// (adding troops results in a negative number).
	/// Should be used to correct rounding errors when using RemoveTroopByPercentageInAllConts
	/// (that method is better for setting remaining troops because it balances losses between the conts)
	/// </summary>
	/// <param name="troopID"></param>
	/// <param name="amountToAdd"></param>
	/// <returns></returns>
	public int AdjustTroopAmountInConts(int troopID, int amountToAdd) {
		int troopIndexInCurContainer = -1;
		int adjustmentMade = 0;
		int pointsPerTroop = GameController.GetTroopTypeByID(troopID).pointCost;
		int pointAward = 0;
		TroopNumberPair affectedPair;
		while (adjustmentMade != amountToAdd) {
			troopIndexInCurContainer = -1;
			foreach (TroopContainer tContainer in ourContainers) {
				troopIndexInCurContainer = tContainer.IndexOfTroopInContainer(troopID);
				if (troopIndexInCurContainer >= 0) {
					affectedPair = tContainer.troopsContained[troopIndexInCurContainer];
					if (amountToAdd > 0) {
						tContainer.AddTroop(troopID, 1);
						pointAward -= pointsPerTroop;
						adjustmentMade += 1;
					}
					else {
						tContainer.RemoveTroop(troopID, 1);
						pointAward += pointsPerTroop;
						adjustmentMade -= 1;
					}
					break;
				}
			}

			//if we couldn't find this troop type in any cont....
			if(troopIndexInCurContainer < 0) {
				if(amountToAdd > 0) {
					//then just add one to any container
					ourContainers[0].AddTroop(troopID, 1);
					pointAward -= pointsPerTroop;
					adjustmentMade += 1;
				}else {
					//abort, there's no one to remove anymore
					Debug.LogWarning("troop adjustment: attempted to remove more troops than possible");
					break;
				}
			}
		}

		return pointAward;
	}

	/// <summary>
	/// gives the points to this side's containers;
	/// all points are immediately used to train troops...
	/// and any remaining points can be removed, optionally
	/// </summary>
	/// <param name="points"></param>
	public void SharePointsBetweenConts(int points, bool setPointsToZeroAfterTraining = true) {
		//divide between the containers...
		//also consider the power lost since the beginning of the battle
		//so that if only a few survive, they won't get tons of points
		int pointsForEach = Mathf.RoundToInt((points / ourContainers.Count) *
			(curArmyPower / initialArmyPower));
		Debug.Log("points awarded for each cont: " + pointsForEach);
		foreach (TroopContainer tContainer in ourContainers) {
			tContainer.pointsToSpend += pointsForEach;
			tContainer.TrainTroops();
			if (setPointsToZeroAfterTraining) tContainer.pointsToSpend = 0;
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
			bPanel.OnBattleResolved(this);
		}

		depletingBar = false;
		bPanel.OnOneSideDoneAnimating();
		
	}

	
}
