using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle {
	public Zone warZone = null;

	public BattleFactionSideInfo attackerSideInfo = new BattleFactionSideInfo(), defenderSideInfo = new BattleFactionSideInfo();

	public delegate void OnBattleEnded();

	public OnBattleEnded onBattleEnded;
	/// <summary>
	/// fills armies' data and resets "on lost" delegates to the default "reward the other side" method only
	/// </summary>
	/// <param name="attackerCmders"></param>
	/// <param name="defenderFaction"></param>
	/// <param name="warZone"></param>
	public void FillInfo(List<Commander> attackerCmders, Faction defenderFaction, Zone warZone) {
		this.warZone = warZone;

		//find out which is the "main" attacker faction by checking how many cmders of each faction are there
		Faction mainAtkerFaction = null;

		Dictionary<int, int> attackerFactionsCmdersDict = new Dictionary<int, int>();
		int greatestCmderAmount = 0, greatestAtkerFacID = -1;

		foreach (Commander cmder in attackerCmders) {
			if (!attackerFactionsCmdersDict.ContainsKey(cmder.ownerFaction)) {
				attackerFactionsCmdersDict.Add(cmder.ownerFaction, 1);
			}
			else {
				attackerFactionsCmdersDict[cmder.ownerFaction]++;
			}
		}

		foreach (KeyValuePair<int, int> kvp in attackerFactionsCmdersDict) {
			if (kvp.Value > greatestCmderAmount) {
				greatestCmderAmount = kvp.Value;
				greatestAtkerFacID = kvp.Key;
			}
		}

		mainAtkerFaction = GameController.GetFactionByID(greatestAtkerFacID);

		attackerSideInfo.SetArmyData(mainAtkerFaction, null,
			GameController.CmdersToTroopContainers(attackerCmders));

		defenderSideInfo.SetArmyData(defenderFaction, warZone,
			GameController.CmdersToTroopContainers
			(GameController.GetCommandersOfFactionAndAlliesInZone(warZone, defenderFaction)));
	}

	/// <summary>
	/// multiple mini-battles are made between randomized samples of each side's army
	/// until one (or both) of them runs out of troops
	/// </summary>
	public void AutocalcResolution() {
		//Debug.Log("---AUTOBATTLE START---");
		//Debug.Log(attackerSide.factionNameTxt.text + " VS " + defenderSide.factionNameTxt.text);

		float winnerDmgMultiplier = GameController.instance.curData.rules.autoResolveWinnerDamageMultiplier;

		int baseSampleSize = GameController.instance.curData.rules.autoResolveBattleSampleSize;
		int maxArmyForProportions = GameController.instance.curData.rules.autoResolveBattleMaxArmyForProportion;

		do {
			DoMiniBattle(baseSampleSize, winnerDmgMultiplier, maxArmyForProportions);
		}
		while (attackerSideInfo.curArmyPower > 0 && defenderSideInfo.curArmyPower > 0);

		BattleEndCheck();
		//Debug.Log("---AUTOBATTLE END---");
	}

	/// <summary>
	/// makes the involved factions "fight each other" once, returning True if the conflict is over
	/// </summary>
	public void DoMiniBattle(int baseSampleSize, float winnerDmgMultiplier, int maxArmyForProportions) {
		int sampleSize = Mathf.Min(attackerSideInfo.curArmyNumbers, defenderSideInfo.curArmyNumbers, baseSampleSize);

		int attackerSampleSize = sampleSize;
		int defenderSampleSize = sampleSize;

		float armyNumbersProportion = (float)Mathf.Min(attackerSideInfo.curArmyNumbers, maxArmyForProportions) / Mathf.Min(defenderSideInfo.curArmyNumbers, maxArmyForProportions);
		//Debug.Log("army num proportion: " + armyNumbersProportion);
		//the size with a bigger army gets a bigger sample...
		//but not a simple proportion because the more targets you've got,
		//the easier it is to randomly hit a target hahaha
		if (armyNumbersProportion > 1.0f) {
			armyNumbersProportion = Mathf.Max(0.1f + ((armyNumbersProportion * sampleSize) / (armyNumbersProportion + baseSampleSize)), 1.0f);
			attackerSampleSize = Mathf.RoundToInt(sampleSize * armyNumbersProportion);
		}
		else {
			armyNumbersProportion = ((armyNumbersProportion * baseSampleSize) / (armyNumbersProportion + baseSampleSize));
			defenderSampleSize = Mathf.RoundToInt(sampleSize / armyNumbersProportion);
		}

		//Debug.Log("attacker sSize: " + attackerSampleSize);
		//Debug.Log("defender sSize: " + defenderSampleSize);

		float attackerAutoPower = GameController.
			GetRandomBattleAutocalcPower(attackerSideInfo.sideArmy, attackerSampleSize);
		float defenderAutoPower = GameController.
			GetRandomBattleAutocalcPower(defenderSideInfo.sideArmy, defenderSampleSize);

		//Debug.Log("attacker auto power: " + attackerAutoPower);
		//Debug.Log("defender auto power: " + defenderAutoPower);

		//make the winner lose some power as well (or not, depending on the rules)
		if (attackerAutoPower > defenderAutoPower) {
			defenderAutoPower *= winnerDmgMultiplier;
		}
		else {
			attackerAutoPower *= winnerDmgMultiplier;
		}

		defenderSideInfo.SetPostBattleArmyData_PowerLost(attackerAutoPower);
		attackerSideInfo.SetPostBattleArmyData_PowerLost(defenderAutoPower);

	}

	/// <summary>
	/// checks if one (or both) side has run out of troops;
	/// if so, runs the "battle ended" delegate (if any) and rewards the winning side with the loser's points
	/// </summary>
	public bool BattleEndCheck() {
		bool battleHasEnded = false;

		if (attackerSideInfo.curArmyPower <= 0) {
			if (defenderSideInfo.curArmyPower <= 0) {
				//both sides are gone!
				battleHasEnded = true;
			}
			else {
				//attackers lost!
				RewardTheOtherSide(attackerSideInfo);
				battleHasEnded = true;
			}
		}
		else if (defenderSideInfo.curArmyPower <= 0) {
			//defenders lost!
			RewardTheOtherSide(defenderSideInfo);
			battleHasEnded = true;
		}

		if (battleHasEnded && onBattleEnded != null) {
			onBattleEnded();
			onBattleEnded = null;
		}

		return battleHasEnded;
	}

	/// <summary>
	/// rewards the winning side with the points obtained by eliminating the other side
	/// </summary>
	/// <param name="loserSide"></param>
	public void RewardTheOtherSide(BattleFactionSideInfo loserSide) {
		if (loserSide == attackerSideInfo) {
			defenderSideInfo.SharePointsBetweenConts(loserSide.pointsAwardedToVictor);
		}
		else {
			attackerSideInfo.SharePointsBetweenConts(loserSide.pointsAwardedToVictor);
		}
	}
}

public class BattleFactionSideInfo {

	public Faction leadingFaction;

	/// <summary>
	/// combined army of all containers of this side in the battle
	/// </summary>
	public List<TroopNumberPair> sideArmy = new List<TroopNumberPair>();

	public List<TroopContainer> ourContainers = new List<TroopContainer>();

	public float initialArmyPower;

	public float curArmyPower;

	public int curArmyNumbers = 0;

	/// <summary>
	/// if this army is defeated, it will award this amount of points to the other side
	/// </summary>
	public int pointsAwardedToVictor = 0;

	public delegate void OnSideDefeated();

	public OnSideDefeated onSideDefeated;

	/// <summary>
	/// sets the displayed and stored army data, such as autocalc power and numbers.
	/// providing the zone separated from the rest of our containers will affect the description
	/// of forces only
	/// </summary>
	/// <param name="leadingFaction"></param>
	/// <param name="ourZone"></param>
	/// <param name="containers"></param>
	/// <param name="postBattle"></param>
	public void SetArmyData(Faction leadingFaction, Zone ourZone, List<TroopContainer> containers) {
		ourContainers.Clear();
		ourContainers.AddRange(containers);
		sideArmy.Clear();
		this.leadingFaction = leadingFaction;
		pointsAwardedToVictor = 0;
		int armyNumbers = 0;
		int armyCmders = 0;
		float armyPower = 0;
		if (ourZone != null) {

			if (!ourContainers.Contains(ourZone)) {//if our zone wasn't added in ourContainers already...
				ourContainers.Add(ourZone);
				armyCmders--;
			}

		}

		foreach (TroopContainer cont in ourContainers) {
			armyCmders++;
			armyNumbers += cont.TotalTroopsContained;
			armyPower += cont.TotalAutocalcPower;
			sideArmy = cont.GetCombinedTroops(sideArmy);
		}


		curArmyNumbers = armyNumbers;
		initialArmyPower = armyPower;
		curArmyPower = armyPower;
	}

	/// <summary>
	/// updates the army's "power" and "numbers" data.
	/// calls the "onSideDefeated" delegate if it's not null and the army's numbers got to 0 
	/// </summary>
	/// <param name="startDepletePowerBarRoutine"></param>
	public void UpdatePostBattleArmy() {
		sideArmy.Clear();
		int armyNumbers = 0;
		float armyPower = 0;

		foreach (TroopContainer cont in ourContainers) {
			armyNumbers += cont.TotalTroopsContained;
			armyPower += cont.TotalAutocalcPower;
			sideArmy = cont.GetCombinedTroops(sideArmy);
		}

		curArmyNumbers = armyNumbers;
		curArmyPower = armyPower;

		if (armyNumbers <= 0) {
			if (onSideDefeated != null) {
				onSideDefeated();
				onSideDefeated = null;
			}

		}
	}

	/// <summary>
	/// distributes losses among troop containers
	/// </summary>
	/// <param name="remainingArmy"></param>
	public void SetPostBattleArmyData_RemainingArmy(List<TroopNumberPair> remainingArmy) {
		float lossPercent = 0;
		int newTroopAmount = 0;
		int pointsLost = 0;
		int troopIndexInRemainingArmy = -1;
		for (int i = sideArmy.Count - 1; i >= 0; i--) {
			troopIndexInRemainingArmy = GameController.IndexOfTroopInTroopList(remainingArmy, sideArmy[i].troopTypeID);
			if (troopIndexInRemainingArmy != -1) {
				newTroopAmount = remainingArmy[troopIndexInRemainingArmy].troopAmount;
				lossPercent = 1.0f - ((float)newTroopAmount / sideArmy[i].troopAmount);
				pointsLost += RemoveTroopByPercentageInAllConts(sideArmy[i].troopTypeID, sideArmy[i].troopAmount,
					lossPercent, newTroopAmount);
			}
			else {
				//if we can't find the type in the remaining army, we've lost all of them
				lossPercent = 1.0f;
				pointsLost += RemoveTroopByPercentageInAllConts(sideArmy[i].troopTypeID, sideArmy[i].troopAmount,
					lossPercent, 0);
			}
		}

		pointsAwardedToVictor += Mathf.RoundToInt(pointsLost *
			GameController.instance.curData.rules.battleWinnerPointAwardFactor);
		UpdatePostBattleArmy();
	}

	/// <summary>
	/// distributes losses among troop containers
	/// </summary>
	/// <param name="remainingPercent"></param>
	public void SetPostBattleArmyData_RemainingPercent(float remainingPercent) {
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
			GameController.instance.curData.rules.battleWinnerPointAwardFactor);
		UpdatePostBattleArmy();
	}

	/// <summary>
	/// converts the power lost to a percentage of troops lost,
	/// then runs the "set post battle data" according to that percentage.
	/// Also optionally calls the power bar depletion routine
	/// </summary>
	public void SetPostBattleArmyData_PowerLost(float powerLost) {
		float remainingPercentage = Mathf.Max(0.0f, (curArmyPower - powerLost) / curArmyPower);

		SetPostBattleArmyData_RemainingPercent(remainingPercentage);
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
		//Debug.Log("remove by percent: loss is " + lossPercent + " and known remains is " + knownRemainingTroops);
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

		if (knownRemainingTroops >= 0 &&
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
			if (troopIndexInCurContainer < 0) {
				if (amountToAdd > 0) {
					//then just add one to any container
					ourContainers[0].AddTroop(troopID, 1);
					pointAward -= pointsPerTroop;
					adjustmentMade += 1;
				}
				else {
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
		//Debug.Log("points awarded for each cont: " + pointsForEach);
		foreach (TroopContainer tContainer in ourContainers) {
			tContainer.pointsToSpend += pointsForEach;
			tContainer.TrainTroops();
			if (setPointsToZeroAfterTraining) tContainer.pointsToSpend = 0;
		}
	}
}