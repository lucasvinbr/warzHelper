using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// this script defines how faction relations are affected by game events
/// </summary>
public class DiplomacyManager {

	#region consts
	/// <summary>
	/// our relation worsens with the faction that attacked us
	/// </summary>
	public const float MIN_REL_DMG_ATTACKED = -0.78F, MAX_REL_DMG_ATTACKED = -0.95F;

	/// <summary>
	/// our relation worsens with the faction that attacked our ally
	/// </summary>
	public const float MIN_REL_DMG_ALLY_ATTACKED = -0.43F, MAX_REL_DMG_ALLY_ATTACKED = -0.75F;

    /// <summary>
    /// slowly corrode all relations as the war progresses, to make alliances end and
    /// factions that keep taking too many zones become targets
    /// </summary>
    public const float MIN_REL_DMG_AGGRESSIVE = -0.001F, MAX_REL_DMG_AGGRESSIVE = -0.005F;

    /// <summary>
    /// our relation worsens with the faction that became allies with an enemy of ours
    /// </summary>
    public const float MIN_REL_DMG_ALLIED_TO_ENEMY = -0.25F, MAX_REL_DMG_ALLIED_TO_ENEMY = -0.35F;

	/// <summary>
	/// our relation gets better with the faction that attacked our enemy
	/// </summary>
	public const float MIN_REL_GAIN_ENEMY_ATTACKED = 0.27F, MAX_REL_GAIN_ENEMY_ATTACKED = 0.42F;

	/// <summary>
	/// our relation gets better with the faction that was attacked by our enemy
	/// </summary>
	public const float MIN_REL_GAIN_ATTACKED_BY_ENEMY = 0.05F, MAX_REL_GAIN_ATTACKED_BY_ENEMY = 0.11F;

	/// <summary>
	/// our relation gets much better with the faction that joins our attack on a common enemy 
	/// </summary>
	public const float MIN_REL_GAIN_ATK_ENEMY_TOGETHER = 0.52F, MAX_REL_GAIN_ATK_ENEMY_TOGETHER = 0.79F;

	/// <summary>
	/// our relation gets better with the faction that became allies with an ally of ours
	/// </summary>
	public const float MIN_REL_GAIN_ALLIED_TO_ALLY = 0.02F, MAX_REL_GAIN_ALLIED_TO_ALLY = 0.04F;

	/// <summary>
	/// how good must our relations be before we consider an alliance? (unused for now)
	/// </summary>
	public const float MIN_RELPERCENT_REQUIRED_ALLIANCE = 0.75f;

	/// <summary>
	/// our relation gets much better with a faction that gave us a zone
	/// </summary>
	public const float MIN_REL_GAIN_RECEIVED_ZONE_GIFT = 0.35f, MAX_REL_GAIN_RECEIVED_ZONE_GIFT = 0.45f;

	/// <summary>
	/// our relation gets worse with a faction that received a zone from an enemy of ours
	/// </summary>
	public const float MIN_REL_DMG_RECEIVED_ZONE_GIFT_FROM_ENEMY = -0.25f, MAX_REL_DMG_RECEIVED_ZONE_GIFT_FROM_ENEMY = -0.35f;

	/// <summary>
	/// our relation gets much worse with a faction that gave a zone to an enemy of ours
	/// </summary>
	public const float MIN_REL_DMG_GAVE_ZONE_TO_ENEMY = -0.55f, MAX_REL_DMG_GAVE_ZONE_TO_ENEMY = -0.75f;

	/// <summary>
	/// factions that have more enemies than allies should try to get more allies faster
	/// </summary>
	public const float FACTION_IN_DISADVANTAGE_FRIENDLINESS_MULTIPLIER = 1.15f;

	/// <summary>
	/// relations should return to neutral with time, unless something favorable keeps happening
	/// </summary>
	public const float MIN_REL_DMG_ALLIANCE_DECAY = -0.005f, MAX_REL_DMG_ALLIANCE_DECAY = -0.025f;

    /// <summary>
    /// relations should return to neutral with time, unless something bad keeps happening
    /// </summary>
    public const float MIN_REL_GAIN_ENEMY_DECAY = 0.005f, MAX_REL_GAIN_ENEMY_DECAY = 0.025f;

    #endregion

    #region reactions to attacks
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

			List<Faction> factionsInDisadvantage = GetAllFactionsInDisadvantage();

			foreach (Faction fac in gData.factions) {
				FacReactToAttack(fac.ID, attackers, attackedZone.ownerFaction, factionsInDisadvantage.Contains(fac) ? 
																					FACTION_IN_DISADVANTAGE_FRIENDLINESS_MULTIPLIER :
																					1.0f);
			}
		}

	}

	/// <summary>
	/// this makes our faction change its relation levels after an attack
	/// if the attack somehow affected it (attacked faction was an ally, for example)
	/// </summary>
	/// <param name="ourFacID"></param>
	/// <param name="attackerFacs"></param>
	/// <param name="attackedFacID"></param>
	public static void FacReactToAttack(int ourFacID, List<int> attackerFacs, int attackedFacID, float relationGainMultiplier = 1.0f) {
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
					ourFac.AddRelationWith(attackerFacs, GetRelGainEnemyAttacked() * relationGainMultiplier);
				}
				else{
					if (standingWithAttacked == GameFactionRelations.FactionStanding.ally)
					{
						ourFac.AddRelationWith(attackerFacs, GetRelDmgAllyAttacked());
					}

					//add some relation to the attacked if at least one of the attackers is an enemy of ours
					bool attackersContainEnemy = false;
					foreach(int facID in attackerFacs)
					{
						if(ourFac.GetStandingWith(facID) == GameFactionRelations.FactionStanding.enemy)
						{
							attackersContainEnemy = true;
							break;
						}
					}
					if (attackersContainEnemy)
					{
						ourFac.AddRelationWith(attackedFac, GetRelGainAttackedByEnemy() * relationGainMultiplier);
					}
				}
				//and worsen relations
				ourFac.AddRelationWith(attackerFacs, GetRelDmgAggressiveBehaviour());
			}else {
				foreach(int atkerFacID in attackerFacs) {
					if(atkerFacID != ourFacID) {
						ourFac.AddRelationWith(atkerFacID, GetRelGainJoinAttack() * relationGainMultiplier);
					}
				}
			}
		}
	}

	#endregion

	#region reactions to zone gifts
	
	/// <summary>
	/// enemies of the gifting faction get angry with both the gifting and the gifted
	/// </summary>
	/// <param name="IDGiftingFac"></param>
	/// <param name="IDGiftedFac"></param>
	public static void GlobalReactToZoneGift(int IDGiftingFac, int IDGiftedFac)
	{
		GameInfo gData = GameController.CurGameData;
		if (!gData.factionRelations.lockRelations)
		{
			foreach (Faction fac in gData.factions)
			{
				FacReactToZoneGift(fac.ID, IDGiftingFac, IDGiftedFac);
			}
		}

	}

	/// <summary>
	/// this makes our faction change its relation levels after a zone gift
	/// if the gift somehow affected it
	/// </summary>
	/// <param name="ourFacID"></param>
	/// <param name="attackerFacs"></param>
	/// <param name="attackedFacID"></param>
	public static void FacReactToZoneGift(int ourFacID, int IDGiftingFac, int IDGiftedFac)
	{
		GameInfo gData = GameController.CurGameData;

		Faction ourFac = GameController.GetFactionByID(ourFacID);

		if (ourFacID == IDGiftingFac)
		{
			return; //the gifted fac will handle increasing relations
		}
		else if (IDGiftedFac >= 0 && IDGiftingFac >= 0)
		{
			if(ourFacID == IDGiftedFac)
			{
				ourFac.AddRelationWith(IDGiftingFac, GetRelGainReceivedZoneGift());
			}
			else
			{
				//we're not giving nor receiving in this event; 
				//there's no positive effect in our relations then.
				//we should still frown at the event if one of them is an enemy,
				//in order to prevent zone giving from being too OP
				if(ourFac.GetStandingWith(IDGiftingFac) == GameFactionRelations.FactionStanding.enemy)
				{
					ourFac.AddRelationWith(IDGiftedFac, GetRelDmgReceivedZoneFromEnemy());
				}

				if(ourFac.GetStandingWith(IDGiftedFac) == GameFactionRelations.FactionStanding.enemy)
				{
					ourFac.AddRelationWith(IDGiftingFac, GetRelDmgGaveZoneToEnemy());
				}
			}
		}
	}
	#endregion


	#region reactions to alliances

	public static void GlobalReactToAlliance(int factionX, int factionY) {
		GameInfo gData = GameController.CurGameData;
		if (!gData.factionRelations.lockRelations) {

			List<Faction> factionsInDisadvantage = GetAllFactionsInDisadvantage();

			foreach (Faction fac in gData.factions) {
				if(fac.ID != factionX && fac.ID != factionY) {
					FactionReactToAlliance(fac, factionX, factionY, factionsInDisadvantage.Contains(fac) ?
																		FACTION_IN_DISADVANTAGE_FRIENDLINESS_MULTIPLIER :
																		1.0f);
				}
			}
		}

	}

	/// <summary>
	/// reactingFaction enhances or degrades relations with factionReactedAgainst, according to
	/// reactingFaction's relation with theirNewAlly
	/// </summary>
	/// <param name="reactingFaction"></param>
	/// <param name="IDfactionReactedAgainst"></param>
	/// <param name="IDtheirNewAlly"></param>
	public static void FactionReactToAlliance(Faction reactingFaction, int IDfactionReactedAgainst, int IDtheirNewAlly, float relationGainMultiplier = 1.0f) {
		switch (reactingFaction.GetStandingWith(IDtheirNewAlly)) {
			case GameFactionRelations.FactionStanding.enemy:
				//frown at factionReactedAgainst!
				reactingFaction.AddRelationWith(IDfactionReactedAgainst, GetRelDmgAlliedToEnemy());
				break;
			case GameFactionRelations.FactionStanding.ally:
				//get closer to factionReactedAgainst!
				reactingFaction.AddRelationWith(IDfactionReactedAgainst, GetRelGainAlliedToAlly() * relationGainMultiplier);
				break;
			default:
				//if we're neutral to theirNewAlly, don't care much about this alliance
				break;
		}
	}

    #endregion

    #region relation decay

    public static void GlobalDecayRelationsWithFac(int factionX)
    {
        GameInfo gData = GameController.CurGameData;
        if (!gData.factionRelations.lockRelations)
        {

            List<Faction> factionsInDisadvantage = GetAllFactionsInDisadvantage();

            foreach (Faction fac in gData.factions)
            {
                if (fac.ID != factionX)
                {
                    FactionRelationsDecayWithFac(fac, factionX, factionsInDisadvantage.Contains(fac) ?
                                                                        FACTION_IN_DISADVANTAGE_FRIENDLINESS_MULTIPLIER :
                                                                        1.0f);
                }
            }
        }

    }

    /// <summary>
    /// reactingFaction enhances or degrades relations with factionReactedAgainst, according to
    /// reactingFaction's current relation: worsens if it's good, gets better if it's bad
    /// </summary>
    /// <param name="reactingFaction"></param>
    /// <param name="IDfactionDecayedAgainst"></param>
    public static void FactionRelationsDecayWithFac(Faction reactingFaction, int IDfactionDecayedAgainst, float relationGainMultiplier = 1.0f)
    {
        switch (reactingFaction.GetStandingWith(IDfactionDecayedAgainst))
        {
            case GameFactionRelations.FactionStanding.enemy:
                //forget grievances a little!
                reactingFaction.AddRelationWith(IDfactionDecayedAgainst, GetRelGainEnemyDecay() * relationGainMultiplier);
                break;
            case GameFactionRelations.FactionStanding.ally:
                //forget a little why we allied in the first place!
                reactingFaction.AddRelationWith(IDfactionDecayedAgainst, GetRelDmgAllianceDecay());
                break;
            default:
                //if we're neutral, don't care
                break;
        }
    }

    #endregion

    /// <summary>
    /// considers a random value between the MIN_REL_REQUIRED and the max 
    /// and how close our relation is to the max
    /// (unused for now)
    /// </summary>
    /// <returns></returns>
    public static bool ShouldConsiderAlliance(float curRelation) {
		return Random.Range(MIN_RELPERCENT_REQUIRED_ALLIANCE * GameFactionRelations.CONSIDER_ALLY_THRESHOLD,
			GameFactionRelations.CONSIDER_ALLY_THRESHOLD) < curRelation;
	}

	/// <summary>
	/// returns a list with all factions that have more enemies than allies
	/// </summary>
	/// <returns></returns>
	public static List<Faction> GetAllFactionsInDisadvantage()
    {
		List<Faction> returnedList = new List<Faction>();
		GameInfo gData = GameController.CurGameData;

		foreach(Faction f in gData.factions)
        {
			if(f.GetDiplomaticEnemies().Count > f.GetDiplomaticAllies().Count)
            {
				returnedList.Add(f);
            }
        }

		return returnedList;
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

	public static float GetRelDmgAlliedToEnemy() {
		return Random.Range(MIN_REL_DMG_ALLIED_TO_ENEMY, MAX_REL_DMG_ALLIED_TO_ENEMY);
	}

	public static float GetRelGainEnemyAttacked() {
		return Random.Range(MIN_REL_GAIN_ENEMY_ATTACKED, MAX_REL_GAIN_ENEMY_ATTACKED);
	}

	public static float GetRelGainAttackedByEnemy()
	{
		return Random.Range(MIN_REL_GAIN_ATTACKED_BY_ENEMY, MAX_REL_GAIN_ATTACKED_BY_ENEMY);
	}

	public static float GetRelGainJoinAttack() {
		return Random.Range(MIN_REL_GAIN_ATK_ENEMY_TOGETHER, MAX_REL_GAIN_ATK_ENEMY_TOGETHER);
	}

	public static float GetRelGainAlliedToAlly() {
		return Random.Range(MIN_REL_GAIN_ALLIED_TO_ALLY, MAX_REL_GAIN_ALLIED_TO_ALLY);
	}

	public static float GetRelDmgGaveZoneToEnemy()
	{
		return Random.Range(MIN_REL_DMG_GAVE_ZONE_TO_ENEMY, MAX_REL_DMG_GAVE_ZONE_TO_ENEMY);
	}

	public static float GetRelDmgReceivedZoneFromEnemy()
	{
		return Random.Range(MIN_REL_DMG_RECEIVED_ZONE_GIFT_FROM_ENEMY, MAX_REL_DMG_RECEIVED_ZONE_GIFT_FROM_ENEMY);
	}

	public static float GetRelGainReceivedZoneGift()
	{
		return Random.Range(MIN_REL_GAIN_ALLIED_TO_ALLY, MAX_REL_GAIN_ALLIED_TO_ALLY);
	}

    public static float GetRelDmgAllianceDecay()
    {
        return Random.Range(MIN_REL_DMG_ALLIANCE_DECAY, MAX_REL_DMG_ALLIANCE_DECAY);
    }

    public static float GetRelGainEnemyDecay()
    {
        return Random.Range(MIN_REL_GAIN_ENEMY_DECAY, MAX_REL_GAIN_ENEMY_DECAY);
    }
    #endregion


}
