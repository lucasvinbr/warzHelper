using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle {
	public Zone warZone = null;

	public BattleFactionSideInfo attackerSideInfo = new BattleFactionSideInfo(), defenderSideInfo = new BattleFactionSideInfo();

	public delegate void OnBattleEnded();

	public OnBattleEnded onBattleEnded;
	/// <summary>
	/// fills armies' data.
	/// Attackers are picked elsewhere, we just decide who's going to "represent" the attacker group here.
	/// 
	/// </summary>
	/// <param name="attackerCmders"></param>
	/// <param name="defenderFaction"></param>
	/// <param name="warZone"></param>
	public void FillInfo(List<Commander> attackerCmders, Faction defenderFaction, Zone warZone) {
		this.warZone = warZone;

		List<int> attackerFactionIDs = new List<int>();
		List<int> defenderFactionIDs = new List<int>();

		defenderFactionIDs.Add(defenderFaction.ID);

		foreach (Commander cmder in attackerCmders) {
			if (!attackerFactionIDs.Contains(cmder.ownerFaction)) {
				attackerFactionIDs.Add(cmder.ownerFaction);
			}
		}

		//defender cmders are filtered because some might be allied to both an attacker and the defender;
		//in that case, they won't help defend
		List<Commander> defenderCmders = GameController.GetCommandersOfFactionAndAlliesInZone
			(warZone, defenderFaction, attackerFactionIDs);

		foreach (Commander cmder in defenderCmders) {
			if (!defenderFactionIDs.Contains(cmder.ownerFaction)) {
				defenderFactionIDs.Add(cmder.ownerFaction);
			}
		}

		attackerSideInfo.SetupArmyData(GameController.GetFactionsByIDs(attackerFactionIDs), null,
			GameController.CmdersToTroopContainers(attackerCmders));

		defenderSideInfo.SetupArmyData(GameController.GetFactionsByIDs(defenderFactionIDs), warZone,
			GameController.CmdersToTroopContainers(defenderCmders));
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
		int maxArmyForProportions = GameController.instance.curData.rules.maxTroopsInvolvedInBattlePerTurn;

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
		BattleFactionSideInfo loserSide = null;

		if (attackerSideInfo.curArmyPower <= 0) {
			if (defenderSideInfo.curArmyPower <= 0) {
				//both sides are gone!
				battleHasEnded = true;
			}
			else {
				//attackers lost!
				loserSide = attackerSideInfo;
				battleHasEnded = true;
			}
		}
		else if (defenderSideInfo.curArmyPower <= 0) {
			//defenders lost!
			loserSide = defenderSideInfo;
			battleHasEnded = true;
		}

		if (battleHasEnded) {
			attackerSideInfo.ApplyLossesToTroopContainers();
			defenderSideInfo.ApplyLossesToTroopContainers();

			if(loserSide != null)
			{
				if(loserSide == attackerSideInfo) defenderSideInfo.SharePointsBetweenConts(attackerSideInfo.pointsAwardedToVictor);
				else attackerSideInfo.SharePointsBetweenConts(defenderSideInfo.pointsAwardedToVictor);
			}


			onBattleEnded?.Invoke();
		}

		return battleHasEnded;
	}


}