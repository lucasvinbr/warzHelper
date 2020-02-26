using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// this is where orders given using the 'unified' mode should be stored,
/// and then executed all at once
/// </summary>
[System.Serializable]
public class UnifiedOrdersRegistry {

	public List<RegisteredCmderOrder> registeredOrders = new List<RegisteredCmderOrder>();

	public UnifiedOrdersRegistry() { }

	/// <summary>
	/// executes all registered orders and then clears the registry
	/// </summary>
	public void RunAllOrders() {
		HideAllOrderFeedbacks();

		foreach (RegisteredCmderOrder order in registeredOrders) {
			order.ExecuteOrder();
		}

		registeredOrders.Clear();
	}

	/// <summary>
	/// adds the order to the registry.
	/// (orders should be checked for "validness" - check if cmder is full before recruiting, for example -
	/// before being added)
	/// </summary>
	/// <param name="orderType"></param>
	/// <param name="orderedActorID"></param>
	/// <param name="zoneTargetID"></param>
	public void RegisterOrder(RegisteredCmderOrder.OrderType orderType, int orderedActorID = -1, int zoneTargetID = -1, bool addVisualFeedbackNow = false) {
		RegisteredCmderOrder newOrder = new RegisteredCmderOrder(orderType, orderedActorID, zoneTargetID);
		registeredOrders.Add(newOrder);
		if (addVisualFeedbackNow) {
			AddFeedbackForOrder(newOrder);
		}
	}

	/// <summary>
	/// checks all orders that aren't "create cmder" (since, in that case, the ID would be that of a faction)
	/// and returns the one given to the target ID
	/// </summary>
	/// <param name="orderedCmderID"></param>
	/// <returns></returns>
	public RegisteredCmderOrder GetOrderGivenToCmder(int orderedCmderID) {
		foreach (RegisteredCmderOrder order in registeredOrders) {
			if (order.orderType != RegisteredCmderOrder.OrderType.createCmder &&
				order.orderedActorID == orderedCmderID) {
				return order;
			}
		}

		return null;
	}

	/// <summary>
	/// if an order was given to the target cmder, removes it and returns true; if no order is found, returns false.
	/// does not affect order visual feedbacks; they should be refreshed manually afterwards!
	/// </summary>
	/// <param name="orderedCmderID"></param>
	/// <returns></returns>
	public bool RemoveAnyOrderGivenToCmder(int orderedCmderID)
	{
		RegisteredCmderOrder cmderOrder = null;
		foreach (RegisteredCmderOrder order in registeredOrders)
		{
			if (order.orderType != RegisteredCmderOrder.OrderType.createCmder &&
				order.orderedActorID == orderedCmderID)
			{

				cmderOrder = order;
			}
		}

		if(cmderOrder != null)
		{
			registeredOrders.Remove(cmderOrder);
		}

		return cmderOrder != null;
	}

	/// <summary>
	/// returns all orders that have the target zone as destination,
	/// optionally not considering "create cmder" orders
	/// </summary>
	/// <param name="targetZoneID"></param>
	/// <returns></returns>
	public List<RegisteredCmderOrder> GetOrdersTargetingZone(int targetZoneID, bool considerCreateCmderOrders = true) {
		List<RegisteredCmderOrder> returnedOrders = new List<RegisteredCmderOrder>();

		foreach (RegisteredCmderOrder order in registeredOrders) {
			if ((considerCreateCmderOrders || order.orderType != RegisteredCmderOrder.OrderType.createCmder) &&
				order.zoneTargetID == targetZoneID) {
				returnedOrders.Add(order);
			}
		}

		return returnedOrders;
	}

	public void AddFeedbackForOrder(RegisteredCmderOrder order) {
		if(order.orderType == RegisteredCmderOrder.OrderType.createCmder ||
			order.orderType == RegisteredCmderOrder.OrderType.move) {

			Zone targetZone = null;
			if (order.zoneTargetID != -1) {
				targetZone = GameController.GetZoneByID(order.zoneTargetID);
				if (targetZone == null) return;
			}

			if(order.orderType == RegisteredCmderOrder.OrderType.createCmder) {
				WorldVisualFeedbacks.instance.createCmderFBCycler.PlaceObjAt(targetZone.MyZoneSpot.transform);
			}else {
				Commander targetCmder = GameController.GetCmderByID(order.orderedActorID);
				if (targetCmder == null) return;

				Faction ownerFac = GameController.GetFactionByID(targetCmder.ownerFaction);
				if (ownerFac == null) return;

				LinkLine line = WorldVisualFeedbacks.instance.moveFBCycler.GetAnObj().GetComponent<LinkLine>();
				line.SetLink(targetCmder.MeIn3d, targetZone.MyZoneSpot, ownerFac.color, 0.3f);
			}
		}else {
			Commander targetCmder = GameController.GetCmderByID(order.orderedActorID);
			if (targetCmder == null) return;

			if(order.orderType == RegisteredCmderOrder.OrderType.recruit) {
				WorldVisualFeedbacks.instance.recruitFBCycler.PlaceObjAt(targetCmder.MeIn3d.transform);
			}else {
				WorldVisualFeedbacks.instance.trainingFBCycler.PlaceObjAt(targetCmder.MeIn3d.transform);
			}
		}
	}

	public void HideAllOrderFeedbacks() {
		WorldVisualFeedbacks.instance.PoolAllOrderFeedbacks();
	}

	/// <summary>
	/// gets the current playing faction and makes only the feedbacks from allied facs (or the playing fac itself) be displayed
	/// </summary>
	public void RefreshOrderFeedbacksVisibility() {
		HideAllOrderFeedbacks();

		Faction curPlayingFac = GameModeHandler.instance.curPlayingFaction;

		if (curPlayingFac == null) return; //no need to work on feedbacks for a null player

		if (!curPlayingFac.isPlayer) return; //no need to work on feedbacks for the bots either

		Faction orderGivingFac = null;
		Commander orderedCmder = null;

		foreach (RegisteredCmderOrder order in registeredOrders) {
			if (order.orderType == RegisteredCmderOrder.OrderType.createCmder) {
				if (order.orderedActorID == curPlayingFac.ID) {
					orderGivingFac = curPlayingFac;
				}
				else {
					orderGivingFac = GameController.GetFactionByID(order.orderedActorID);
				}
				
			}else {
				orderedCmder = GameController.GetCmderByID(order.orderedActorID);
				if (orderedCmder == null) continue;

				if(orderedCmder.ownerFaction == curPlayingFac.ID) {
					orderGivingFac = curPlayingFac;
				}else {
					orderGivingFac = GameController.GetFactionByID(orderedCmder.ownerFaction);
				}
			}

			if (orderGivingFac == null) continue;

			if(curPlayingFac == orderGivingFac ||
				curPlayingFac.GetStandingWith(orderGivingFac) == GameFactionRelations.FactionStanding.ally) {
				AddFeedbackForOrder(order);
			}
		}
	}
}

