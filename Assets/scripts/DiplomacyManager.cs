using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
	public const float MIN_REL_DMG_TOOK_ZONE = -0.025F, MAX_REL_DMG_TOOK_ZONE = -0.06F;

	/// <summary>
	/// our relation worsens with the faction that became allies with an enemy of ours
	/// </summary>
	public const float MIN_REL_DMG_ALLIED_TO_ENEMY = -0.35F, MAX_REL_DMG_ALLIED_TO_ENEMY = -0.55F;

	/// <summary>
	/// our relation gets better with the faction that attacked our enemy
	/// </summary>
	public const float MIN_REL_GAIN_ENEMY_ATTACKED = 0.1F, MAX_REL_GAIN_ENEMY_ATTACKED = 0.2F;

	/// <summary>
	/// our relation gets better with the faction that became allies with an ally of ours
	/// </summary>
	public const float MIN_REL_GAIN_ALLIED_TO_ALLY = 0.15F, MAX_REL_GAIN_ALLIED_TO_ALLY = 0.25F;

	/// <summary>
	/// how good must our relations be before we consider an alliance?
	/// </summary>
	public const float MIN_RELPERCENT_REQUIRED_ALLIANCE = 0.75f;


	public static void GlobalReactToAttack(Faction attackerFac, Faction attackedFac) {
		GameInfo gData = GameController.instance.curData as GameInfo;
		if (!gData.factionRelations.lockRelations) {
			foreach (Faction fac in gData.factions) {
				FacReactToAttack(fac, attackerFac, attackedFac);
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
			ourFac.AddRelationWith(attackerFac, GetRelDmgAttacked(), notifyRelationChange: ourFac.isPlayer);
		}else if (attackedFac != null && ourFac != attackerFac) {
			
			GameFactionRelations.FactionStanding standingWithAttacked = 
				attackedFac.GetStandingWith(ourFac);
			if (standingWithAttacked == GameFactionRelations.FactionStanding.enemy) {
				ourFac.AddRelationWith(attackerFac, GetRelGainEnemyAttacked(), notifyRelationChange: ourFac.isPlayer);
			}else if(standingWithAttacked == GameFactionRelations.FactionStanding.ally) {
				ourFac.AddRelationWith(attackerFac, GetRelDmgAllyAttacked(), notifyRelationChange: ourFac.isPlayer);
			}
			//and worsen relations
			ourFac.AddRelationWith(attackerFac, GetRelDmgTookZone());
		}
	}

	public static float GetRelDmgAttacked() {
		return Random.Range(MIN_REL_DMG_ATTACKED, MAX_REL_DMG_ATTACKED);
	}

	public static float GetRelDmgAllyAttacked() {
		return Random.Range(MIN_REL_DMG_ALLY_ATTACKED, MAX_REL_DMG_ALLY_ATTACKED);
	}

	public static float GetRelDmgTookZone() {
		return Random.Range(MIN_REL_DMG_TOOK_ZONE, MAX_REL_DMG_TOOK_ZONE);
	}

	public static float GetRelGainEnemyAttacked() {
		return Random.Range(MIN_REL_GAIN_ENEMY_ATTACKED, MAX_REL_GAIN_ENEMY_ATTACKED);
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
}
