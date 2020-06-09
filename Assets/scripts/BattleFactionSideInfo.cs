using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFactionSideInfo {

	public List<Faction> sideFactions;

	/// <summary>
	/// combined army of all containers of this side in the battle, possibly clamped to the max involved in one turn
	/// </summary>
	public TroopList sideArmy = new TroopList();

	public List<TroopContainer> ourContainers = new List<TroopContainer>();

	private TroopList totalBattleLosses = new TroopList();

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
	/// <param name="sideFactions"></param>
	/// <param name="ourZone"></param>
	/// <param name="containers"></param>
	/// <param name="postBattle"></param>
	public void SetupArmyData(List<Faction> sideFactions, Zone ourZone, List<TroopContainer> containers) {
		totalBattleLosses.Clear();
		ourContainers.Clear();
		ourContainers.AddRange(containers);
		sideArmy.Clear();
		this.sideFactions = sideFactions;
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
			armyNumbers += cont.troopsContained.TotalTroopAmount;
			armyPower += cont.troopsContained.TotalAutocalcPower;
			sideArmy = cont.troopsContained.GetCombinedTroops(sideArmy);
		}

		//clamp the side's forces according to the maxTroopsInvolvedInBattlePerTurn rule

		int maxTroops = GameController.CurGameData.rules.maxTroopsInvolvedInBattlePerTurn;

		sideArmy = GameController.GetRandomSampleArmyFromArmy(sideArmy, maxTroops);

		curArmyNumbers = Mathf.Min(armyNumbers, maxTroops);

		if(curArmyNumbers != armyNumbers)
		{
			armyPower = sideArmy.TotalAutocalcPower;
		}
		
		initialArmyPower = armyPower;
		curArmyPower = armyPower;
	}

	/// <summary>
	/// updates the army's "power" and "numbers" data according to the provided losses list.
	/// calls the "onSideDefeated" delegate if it's not null and the army's numbers got to 0 
	/// </summary>
	public void UpdatePostBattleArmy(TroopList lossesList) {

		totalBattleLosses = totalBattleLosses.GetCombinedTroops(lossesList);
		int armyNumbers;

		sideArmy = sideArmy.GetListWithAppliedLosses(lossesList);

		armyNumbers = sideArmy.TotalTroopAmount;

		curArmyNumbers = armyNumbers;
		curArmyPower = sideArmy.TotalAutocalcPower;

		if (armyNumbers <= 0) {
			if (onSideDefeated != null) {
				onSideDefeated();
				onSideDefeated = null;
			}

		}
	}

	/// <summary>
	/// distributes losses among troop containers, returning a "losses" list
	/// </summary>
	/// <param name="remainingArmy"></param>
	public void SetPostBattleArmyData_RemainingArmy(TroopList remainingArmy) {
		UpdatePostBattleArmy(sideArmy.GetLossesList(remainingArmy));
	}

	/// <summary>
	/// distributes losses among troop containers
	/// </summary>
	/// <param name="remainingPercent"></param>
	public void SetPostBattleArmyData_RemainingPercent(float remainingPercent) {
		UpdatePostBattleArmy(sideArmy.GetLossesListByPercentage(remainingPercent));
	}

	/// <summary>
	/// converts the power lost to a percentage of troops lost,
	/// then runs the "set post battle data" according to that percentage.
	/// </summary>
	public void SetPostBattleArmyData_PowerLost(float powerLost) {
		float remainingPercentage = Mathf.Max(0.0f, (curArmyPower - powerLost) / curArmyPower);

		SetPostBattleArmyData_RemainingPercent(remainingPercentage);
	}


	/// <summary>
	/// uses the totalLosses list to remove troops from the troop containers,
	/// attempting to share the losses equally and storing the points awarded to the victor
	/// </summary>
	public void ApplyLossesToTroopContainers()
	{
		int lossesRemainingToApply;
		int pointsAwardedPerTroop;
		int troopsRemovedFromCont;
		int lossesPerCont;
		List<TroopContainer> contsWithTroop = new List<TroopContainer>();
		foreach(TroopNumberPair lostTroop in totalBattleLosses)
		{
			//store awarded points...
			pointsAwardedPerTroop = GameController.GetTroopTypeByID(lostTroop.troopTypeID).pointCost;
			pointsAwardedToVictor += pointsAwardedPerTroop * lostTroop.troopAmount;
			lossesRemainingToApply = lostTroop.troopAmount;

			contsWithTroop.Clear();

			//find all containers that have the troop...
			foreach(TroopContainer cont in ourContainers)
			{
				if (cont.troopsContained.HasTroop(lostTroop.troopTypeID))
				{
					contsWithTroop.Add(cont);
				}
			}

			lossesPerCont = lostTroop.troopAmount / contsWithTroop.Count;

			//then remove the "equalized" troop amount from all of them
			for (int i = contsWithTroop.Count - 1; i >= 0; i--)
			{
				troopsRemovedFromCont =
					contsWithTroop[i].troopsContained.RemoveTroop(lostTroop.troopTypeID, lossesPerCont);
				lossesRemainingToApply -= troopsRemovedFromCont;

				if(troopsRemovedFromCont < lossesPerCont)
				{
					//this means this cont no longer has this troop type
					contsWithTroop.RemoveAt(i);
				}
			}

			//also remove any remainders of the loss division
			while (lossesRemainingToApply > 0)
			{
				foreach(TroopContainer container in contsWithTroop)
				{
					if (container.troopsContained.HasTroop(lostTroop.troopTypeID))
					{
						lossesRemainingToApply -= 
							container.troopsContained.RemoveTroop(lostTroop.troopTypeID, lossesRemainingToApply);

						if (lossesRemainingToApply <= 0) break;
					}
				}
			}
		}
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

		//apply the battleWinnerPointAwardFactor rule
		points = Mathf.RoundToInt(points *
			GameController.instance.curData.rules.battleWinnerPointAwardFactor);

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