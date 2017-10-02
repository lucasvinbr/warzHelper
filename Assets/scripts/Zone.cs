using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Zone {

    public string name;

    public string ownerFaction;

    public int bonusTroopsGeneratedPerTurn = 0;

    public int troopsGarrisoned = 0;

    public Vector2 coords;
}
