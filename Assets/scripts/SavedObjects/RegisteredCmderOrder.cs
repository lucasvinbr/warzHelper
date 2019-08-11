using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// An order, issued by a player, to a commander, that has not yet been executed.
/// Instances of this object should only exist if the game is being played in "unified" mode
/// </summary>
[System.Serializable]
public class RegisteredCmderOrder {

	/// <summary>
	/// the ID of the ordered Cmder or Faction (it's a faction if it's a "create cmder" order)
	/// </summary>
	public int orderedActorID;

	/// <summary>
	/// if this order is of the "move" or "create cmder" type,
	/// the ID of the zone we should act on
	/// </summary>
	public int zoneTargetID;

	public enum OrderType {
		createCmder,
		move,
		train,
		recruit
	}

	public OrderType orderType;


	public RegisteredCmderOrder() { }

	public RegisteredCmderOrder( OrderType orderType, int orderedCmderID = -1, int zoneTargetID = -1) {
		this.orderedActorID = orderedCmderID;
		this.orderType = orderType;
		this.zoneTargetID = zoneTargetID;
	}

	/// <summary>
	/// executes the order, but doesn't remove it from the registry
	/// </summary>
	/// <returns></returns>
	public bool ExecuteOrder() {
		Commander targetCmder = null; //if we're creating a cmder, we'll only get it later on

		if (orderedActorID >= 0 && orderType != OrderType.createCmder) {
			targetCmder = GameController.GetCmderByID(orderedActorID);

			if (targetCmder == null) {
				Debug.LogError("[RegisteredCmderOrder] Target Cmder Not Found! Cmder ID: " + orderedActorID);
				return false;
			}
		}

		switch (orderType) {
			case OrderType.recruit:
				return targetCmder.RecruitTroops();
			case OrderType.train:
				return targetCmder.TrainTroops();
			case OrderType.move:
				return targetCmder.MoveToZone(zoneTargetID);
			case OrderType.createCmder:
				Faction ownerFac = GameController.GetFactionByID(orderedActorID);
				if (ownerFac == null) return false;

				targetCmder = World.CreateNewCmderAtZone(zoneTargetID, ownerFac);
				if (targetCmder == null) return false;
				//create and recruit troops immediately
				targetCmder.GetPointAwardPoints();
				return targetCmder.RecruitTroops();
		}

		return false;
	}
}

