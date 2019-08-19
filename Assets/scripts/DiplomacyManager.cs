using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this script defines how faction relations are affected by game events
/// </summary>
public class DiplomacyManager {

	/// <summary>
	/// our relation worsens with the faction that attacked us
	/// </summary>
	public const float MIN_REL_DMG_ATTACKED = -0.35F, MAX_REL_DMG_ATTACKED = -0.55F;

	/// <summary>
	/// our relation worsens with the faction that attacked our ally
	/// </summary>
	public const float MIN_REL_DMG_ALLY_ATTACKED = -0.1F, MAX_REL_DMG_ALLY_ATTACKED = -0.25F;

	/// <summary>
	/// slowly corrode all relations as the war progresses, to make alliances end and
	/// factions that keep taking too many zones become targets
	/// </summary>
	public const float MIN_REL_DMG_AGGRESSIVE = -0.025F, MAX_REL_DMG_AGGRESSIVE = -0.06F;

	/// <summary>
	/// our relation worsens with the faction that became allies with an enemy of ours
	/// </summary>
	public const float MIN_REL_DMG_ALLIED_TO_ENEMY = -0.35F, MAX_REL_DMG_ALLIED_TO_ENEMY = -0.55F;

	/// <summary>
	/// our relation gets better with the faction that attacked our enemy
	/// </summary>
	public const float MIN_REL_GAIN_ENEMY_ATTACKED = 0.1F, MAX_REL_GAIN_ENEMY_ATTACKED = 0.2F;

	/// <summary>
	/// our relation gets much better with the faction that joins our attack on a common enemy 
	/// </summary>
	public const float MIN_REL_GAIN_ATK_ENEMY_TOGETHER = 0.2F, MAX_REL_GAIN_ATK_ENEMY_TOGETHER = 0.3F;

	/// <summary>
	/// our relation gets better with the faction that became allies with an ally of ours
	/// </summary>
	public const float MIN_REL_GAIN_ALLIED_TO_ALLY = 0.15F, MAX_REL_GAIN_ALLIED_TO_ALLY = 0.25F;

	/// <summary>
	/// how good must our relations be before we consider an alliance?
	/// </summary>
	public const float MIN_RELPERCENT_REQUIRED_ALLIANCE = 0.75f;

	/// <summary>
	/// gets attackers and defenders according to the zone owner's diplomacy.
	/// the attacked should get angry with the attacker;
	/// everyone gets a little relation decrease with the attacker as well...
	/// unless we're enemies with the attacked
	/// </summary>
	/// <param name="attackedZone"></param>
	public static void GlobalReactToAttack(Zone attackedZone) {
		GameInfo gData = GameController.CurGameData;
		if (!gData.factionRelations.lockRelations) {
			List<int> attackers = new List<int>();
			foreach (Commander cmder in GameController.GetCommandersInZone(attackedZone)) {
				if(cmder.ownerFaction != attackedZone.ownerFaction && 
					gData.factionRelations.GetStandingBetweenFactions
					(cmder.ownerFaction, attackedZone.ownerFaction) != GameFactionRelations.FactionStanding.ally &&
					!attackers.Contains(cmder.ownerFaction)) {
					attackers.Add(cmder.ownerFaction);
				}
			}
			foreach (Faction fac in gData.factions) {
				FacReactToAttack(fac.ID, attackers, attackedZone.ownerFaction);
			}
		}

	}

	/// <summary>
	/// this makes our faction change its relation levels after an attack
	/// if the attack somehow affected it (attacked faction was an ally, for example)
	/// </summary>
	/// <param name="ourFac"></param>
	/// <param name="attackerFac"></param>
	/// <param name="attackedFac"></param>
	public static void FacReactToAttack(Faction ourFac, Faction attackerFac, Faction attackedFac) {
		if(ourFac == attackedFac) {
			ourFac.AddRelationWith(attackerFac, GetRelDmgAttacked());
		}else if (attackedFac != null && ourFac != attackerFac) {
			
			GameFactionRelations.FactionStanding standingWithAttacked = 
				attackedFac.GetStandingWith(ourFac);
			if (standingWithAttacked == GameFactionRelations.FactionStanding.enemy) {
				ourFac.AddRelationWith(attackerFac, GetRelGainEnemyAttacked());
			}else if(standingWithAttacked == GameFactionRelations.FactionStanding.ally) {
				ourFac.AddRelationWith(attackerFac, GetRelDmgAllyAttacked());
			}
			//and worsen relations
			ourFac.AddRelationWith(attackerFac, GetRelDmgAggressiveBehaviour());
		}
	}

	/// <summary>
	/// this makes our faction change its relation levels after an attack
	/// if the attack somehow affected it (attacked faction was an ally, for example)
	/// </summary>
	/// <param name="ourFacID"></param>
	/// <param name="attackerFacs"></param>
	/// <param name="attackedFacID"></param>
	public static void FacReactToAttack(int ourFacID, List<int> attackerFacs, int attackedFacID) {
		GameInfo gData = GameController.CurGameData;

		Faction ourFac = GameController.GetFactionByID(ourFacID);

		if (ourFacID == attackedFacID) {
			gData.factionRelations.AddRelationBetweenFactions(ourFacID, attackerFacs, GetRelDmgAttacked(),
				false);
		}
		else if (attackedFacID >= 0) {
			if (!attackerFacs.Contains(ourFacID)) {
				Faction attackedFac = GameController.GetFactionByID(attackedFacID);
				GameFactionRelations.FactionStanding standingWithAttacked =
					attackedFac.GetStandingWith(ourFacID);
				if (standingWithAttacked == GameFactionRelations.FactionStanding.enemy) {
					ourFac.AddRelationWith(attackerFacs, GetRelGainEnemyAttacked());
				}
				else if (standingWithAttacked == GameFactionRelations.FactionStanding.ally) {
					ourFac.AddRelationWith(attackerFacs, GetRelDmgAllyAttacked());
				}
				//and worsen relations
				ourFac.AddRelationWith(attackerFacs, GetRelDmgAggressiveBehaviour());
			}else {
				foreach(int atkerFacID in attackerFacs) {
					if(atkerFacID != ourFacID) {
						ourFac.AddRelationWith(atkerFacID, GetRelGainJoinAttack());
					}
				}
			}
		}
	}


	/// <summary>
	/// considers a random value between the MIN_REL_REQUIRED and the max 
	/// and how close our relation is to the max
	/// </summary>
	/// <returns></returns>
	public static bool ShouldConsiderAlliance(float curRelation) {
		return Random.Range(MIN_RELPERCENT_REQUIRED_ALLIANCE * GameFactionRelations.CONSIDER_ALLY_THRESHOLD,
			GameFactionRelations.CONSIDER_ALLY_THRESHOLD) < curRelation;
	}

	#region randomized value getters
	public static float GetRelDmgAttacked() {
		return Random.Range(MIN_REL_DMG_ATTACKED, MAX_REL_DMG_ATTACKED);
	}

	public static float GetRelDmgAllyAttacked() {
		return Random.Range(MIN_REL_DMG_ALLY_ATTACKED, MAX_REL_DMG_ALLY_ATTACKED);
	}

	public static float GetRelDmgAggressiveBehaviour() {
		return Random.Range(MIN_REL_DMG_AGGRESSIVE, MAX_REL_DMG_AGGRESSIVE);
	}

	public static float GetRelGainEnemyAttacked() {
		return Random.Range(MIN_REL_GAIN_ENEMY_ATTACKED, MAX_REL_GAIN_ENEMY_ATTACKED);
	}

	public static float GetRelGainJoinAttack() {
		return Random.Range(MIN_REL_GAIN_ATK_ENEMY_TOGETHER, MAX_REL_GAIN_ATK_ENEMY_TOGETHER);
	}
	#endregion


}
