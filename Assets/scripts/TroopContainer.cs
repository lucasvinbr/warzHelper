using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TroopContainer {

	public List<TroopNumberPair> troopsContained;

	/// <summary>
	/// ID of the faction that currently owns this container. a negative number probably means this is neutral
	/// </summary>
	public int ownerFaction;

	/// <summary>
	/// sum of the amounts of all troops contained in this container
	/// </summary>
	public int TotalTroopsContained
	{
		get
		{
			return GameController.GetArmyAmountFromTroopList(troopsContained);
		}
	}

	/// <summary>
	/// sum of the autocalc power of all troops contained in this container
	/// </summary>
	public float TotalAutocalcPower
	{
		get
		{
			float total = 0;

			for (int i = 0; i < troopsContained.Count; i++) {
				total += 
					GameController.GetTroopTypeByID(troopsContained[i].troopTypeID).autoResolvePower * 
					troopsContained[i].troopAmount;
			}

			return total;
		}
	}

	public void AddTroop(int TTID, int amount) {
		int troopIndex = IndexOfTroopInContainer(TTID);
		if (troopIndex < 0) {
			troopsContained.Add(new TroopNumberPair(TTID, amount));
		}
		else {
			TroopNumberPair curTargetTroopGroup = troopsContained[troopIndex];

			curTargetTroopGroup.troopAmount += amount;

			troopsContained[troopIndex] = curTargetTroopGroup;
		}


	}

	public void RemoveTroop(int TTID, int amount) {
		int troopIndex = IndexOfTroopInContainer(TTID);
		if (troopIndex < 0) return;

		TroopNumberPair curTargetTroopGroup = troopsContained[troopIndex];

		curTargetTroopGroup.troopAmount -= amount;

		if (curTargetTroopGroup.troopAmount <= 0) {
			troopsContained.RemoveAt(troopIndex);
		}
		else {
			troopsContained[troopIndex] = curTargetTroopGroup;
		}


	}

	public bool HasTroop(int TTID) {
		for (int i = 0; i < troopsContained.Count; i++) {
			if (troopsContained[i].troopTypeID == TTID) {
				return true;
			}
		}

		return false;
	}

	public int IndexOfTroopInContainer(int TTID) {
		for (int i = 0; i < troopsContained.Count; i++) {
			if (troopsContained[i].troopTypeID == TTID) {
				return i;
			}
		}

		return -1;
	}

	/// <summary>
	/// returns a garrison list of the combination of both containers' troops
	/// </summary>
	/// <param name="theOtherContainerTroops"></param>
	/// <returns></returns>
	public List<TroopNumberPair> GetCombinedTroops(List<TroopNumberPair> theOtherContainerTroops) {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();

		returnedList.AddRange(troopsContained);

		int testedTroopIndex = -1;
		TroopNumberPair checkedTNP;
		foreach(TroopNumberPair tnp in theOtherContainerTroops) {
			testedTroopIndex = IndexOfTroopInContainer(tnp.troopTypeID);
			if(testedTroopIndex >= 0) {
				checkedTNP = returnedList[testedTroopIndex];
				checkedTNP.troopAmount += tnp.troopAmount;
				returnedList[testedTroopIndex] = checkedTNP;
			}else {
				returnedList.Add(tnp);
			}
		}

		return returnedList;
	}
}
