using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a TroopList, but owned by a faction and capable of training/recruiting
/// </summary>
public abstract class TroopContainer {

	/// <summary>
	/// ID of the faction that currently owns this container. a negative number probably means this is neutral
	/// </summary>
	public int ownerFaction;

	public int pointsToSpend = 0;

	public TroopList troopsContained;

	/// <summary>
	/// function used when receiving points for victories or training troops
	/// </summary>
	public abstract bool TrainTroops();


	public abstract bool RecruitTroops();
}
