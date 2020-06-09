using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// a list of troops and their numbers, with extra goodies
/// </summary>
public class TroopList : List<TroopNumberPair> {

	/// <summary>
	/// sum of the amounts of all troops contained in this container
	/// </summary>
	public int TotalTroopAmount
	{
		get
		{
			int total = 0;
			for (int i = 0; i < Count; i++)
			{
				total += this[i].troopAmount;
			}
			return total;
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

			for (int i = 0; i < Count; i++)
			{
				total +=
					GameController.GetTroopTypeByID(this[i].troopTypeID).autoResolvePower *
					this[i].troopAmount;
			}

			return total;
		}
	}

	public void AddTroop(int TTID, int amount)
	{
		int troopIndex = IndexOfTroopInThisList(TTID);
		if (troopIndex < 0)
		{
			Add(new TroopNumberPair(TTID, amount));
		}
		else
		{
			TroopNumberPair curTargetTroopGroup = this[troopIndex];

			curTargetTroopGroup.troopAmount += amount;

			this[troopIndex] = curTargetTroopGroup;
		}


	}

	/// <summary>
	/// returns the amount of troops removed
	/// </summary>
	/// <param name="TTID"></param>
	/// <param name="amount"></param>
	/// <returns></returns>
	public int RemoveTroop(int TTID, int amount)
	{
		int troopIndex = IndexOfTroopInThisList(TTID);
		if (troopIndex < 0) return 0;

		TroopNumberPair curTargetTroopGroup = this[troopIndex];

		int initialAmount = curTargetTroopGroup.troopAmount;
		curTargetTroopGroup.troopAmount -= amount;

		if (curTargetTroopGroup.troopAmount <= 0)
		{
			RemoveAt(troopIndex);
			return initialAmount;
		}
		else
		{
			this[troopIndex] = curTargetTroopGroup;
			return amount;
		}


	}

	public bool HasTroop(int TTID)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].troopTypeID == TTID)
			{
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// returns -1 if it can't find a troop with the provided ID
	/// </summary>
	/// <param name="TTID"></param>
	/// <returns></returns>
	public int IndexOfTroopInThisList(int TTID)
	{
		for (int i = 0; i < Count; i++)
		{
			if (this[i].troopTypeID == TTID)
			{
				return i;
			}
		}

		return -1;
	}


	/// <summary>
	/// returns a new troopList of the combination of both troopLists
	/// </summary>
	/// <param name="theOtherContainerTroops"></param>
	/// <returns></returns>
	public TroopList GetCombinedTroops(TroopList theOtherContainerTroops)
	{
		TroopList returnedList = new TroopList();

		returnedList.AddRange(this);

		int testedTroopIndex = -1;
		TroopNumberPair checkedTNP;
		foreach (TroopNumberPair tnp in theOtherContainerTroops)
		{
			testedTroopIndex = IndexOfTroopInThisList(tnp.troopTypeID);
			if (testedTroopIndex >= 0)
			{
				checkedTNP = returnedList[testedTroopIndex];
				checkedTNP.troopAmount += tnp.troopAmount;
				returnedList[testedTroopIndex] = checkedTNP;
			}
			else
			{
				returnedList.Add(tnp);
			}
		}

		return returnedList;
	}


	/// <summary>
	/// returns a "delta" list, containing lost troops when comparing the current and the "remaining" lists
	/// </summary>
	/// <param name="remainingTroops"></param>
	public TroopList GetLossesList(TroopList remainingTroops)
	{
		int troopIndexInAfter;
		int troopsOfCurTypeLost;
		TroopList resultingList = new TroopList();
		for(int i = 0; i < Count; i++)
		{
			troopIndexInAfter = remainingTroops.IndexOfTroopInThisList(this[i].troopTypeID);
			if(troopIndexInAfter != -1)
			{
				troopsOfCurTypeLost = (remainingTroops[troopIndexInAfter].troopAmount - this[i].troopAmount) * -1;
				if (troopsOfCurTypeLost > 0) resultingList.AddTroop(this[i].troopTypeID, troopsOfCurTypeLost);
			}
			else
			{
				//all troops of that type have been lost, probably
				resultingList.AddTroop(this[i].troopTypeID, this[i].troopAmount);
			}
		}

		return resultingList;
	}

	/// <summary>
	/// returns a "delta" list, containing lost troops according to the percentage of remaining troops provided
	/// </summary>
	/// <param name="remainingTroops"></param>
	public TroopList GetLossesListByPercentage(float remainingPercent)
	{
		TroopList resultingList = new TroopList();
		float lossPercent = 1.0f - remainingPercent;
		for (int i = 0; i < Count; i++)
		{
			resultingList.AddTroop(this[i].troopTypeID, Mathf.RoundToInt(this[i].troopAmount * lossPercent));
		}

		return resultingList;
	}


	/// <summary>
	/// returns a new list with subtracted troop amounts according to the lossesList
	/// </summary>
	/// <param name="lossesList">defines how many troops of each type should be removed</param>
	/// <returns></returns>
	public TroopList GetListWithAppliedLosses(TroopList lossesList)
	{
		TroopList returnedList = new TroopList();

		returnedList.AddRange(this);

		for(int i = 0; i < lossesList.Count; i++)
		{
			returnedList.RemoveTroop(lossesList[i].troopTypeID, lossesList[i].troopAmount);
		}

		return returnedList;
	}

}




