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
	public int orderedCmderID;

	/// <summary>
	/// if this order is of the "move" type,
	/// the ID of the zone we should move to
	/// </summary>
	public int zoneTarget;

	/// <summary>
	/// the points that will be used in this order.
	/// Usually equal to how many points the commander had available when this order was issued
	/// </summary>
	public int pointsInvested;

	public enum OrderType {
		move,
		train,
		recruit
	}

	public OrderType orderType;


	public RegisteredCmderOrder() { }

	public RegisteredCmderOrder(int orderedCmderID, OrderType orderType, int pointsInvested = 0, int zoneTarget = -1) {
		this.orderedCmderID = orderedCmderID;
		this.orderType = orderType;
		this.pointsInvested = pointsInvested;
		this.zoneTarget = zoneTarget;
	}

	public bool ExecuteOrder() {
		Commander targetCmder = GameController.GetCmderByID(orderedCmderID);

		if(targetCmder == null) {
			Debug.LogError("[RegisteredCmderOrder] Target Cmder Not Found! Cmder ID: " + orderedCmderID);
			return false;
		}
		//switch (orderType) {
		//	case OrderType.move:

		//}TODO actual order executions and stuff, considering the stored order data

		return false;
	}
}

