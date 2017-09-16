using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone {

    public string name;

    public Faction ownerFaction;

    public int bonusTroopsGeneratedPerTurn = 0;

    public int troopsGarrisoned = 0;

    public Vector2 coords;
}
