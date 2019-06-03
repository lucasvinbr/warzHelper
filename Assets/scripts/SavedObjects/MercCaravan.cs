using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A moving mercenary caravan.
/// Can't be attacked by the factions - caravans stay around forever.
/// When a caravan is in a zone,
/// the troops recruited there will be the of the type specified by the caravan.
/// Caravans' AI avoids making two caravans get to the same zone, 
/// so normally only one caravan can be in one zone at a time
/// </summary>
[System.Serializable]
public class MercCaravan {
	public int ID;

	public int zoneIAmIn;

	/// <summary>
	/// the troop type that will be recruited in the zone the caravan is in
	/// </summary>
	public int containedTroopType;

	/// <summary>
	/// the color of the 3d representation of the caravan
	/// </summary>
	public Color caravanColor;

	private MercCaravan3d _meIn3d;

	public MercCaravan3d MeIn3d
	{
		get
		{
			if(_meIn3d == null) {
				_meIn3d = World.GetCaravan3dForMC(this);
				if(_meIn3d == null) {
					Debug.LogWarning("no 3d for MC! mcID: " + ID);
				}
			}
			return _meIn3d;
		}
	}

	public MercCaravan() { }

	public MercCaravan(int zoneStartingLocation) {
		this.ID = GameController.GetUnusedMercCaravanID();
		this.zoneIAmIn = zoneStartingLocation;
		this.containedTroopType = GameController.instance.LastRelevantTType.ID;
		this.caravanColor = Random.ColorHSV();
		GameController.instance.curData.mercCaravans.Add(this);
	}

	public MercCaravan(int zoneStartingLocation, int containedTroopType, Color caravanColor) {
		this.ID = GameController.GetUnusedMercCaravanID();
		this.zoneIAmIn = zoneStartingLocation;
		this.containedTroopType = containedTroopType;
		this.caravanColor = caravanColor;
		GameController.instance.curData.mercCaravans.Add(this);
	}



	public void MoveTo(Zone targetZone) {
		zoneIAmIn = targetZone.ID;
		TransformTweener.instance.StartTween(MeIn3d.transform, targetZone.MyZoneSpot, false);
	}

	/// <summary>
	/// possibly moves to a random, non-occupied by caravans, zone
	/// </summary>
	public void CaravanThinkMove() {
		Zone zoneMCIsIn = GameController.GetZoneByID(zoneIAmIn);

		if(Random.value > GameController.CurGameData.rules.caravanStayChance) {
			List<int> occupiedZones = new List<int>();

			while(occupiedZones.Count < zoneMCIsIn.linkedZones.Count) {
				int candidateZone = zoneMCIsIn.linkedZones[Random.Range(0, zoneMCIsIn.linkedZones.Count)];
				if (occupiedZones.Contains(candidateZone)) continue;

				if(GameController.GetMercCaravanInZone(candidateZone) == null) {
					MoveTo(GameController.GetZoneByID(candidateZone));
				}else {
					occupiedZones.Add(candidateZone);
				}
			}
			 
		}
		
		 
	}


}

