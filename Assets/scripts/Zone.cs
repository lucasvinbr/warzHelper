using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Zone {

    public string name;

    public string ownerFaction;

    public int bonusTroopsGeneratedPerTurn = 0;

    public int troopsGarrisoned = 0;

    public int incomeGeneratedPerTurn = 0;

    public Vector2 coords;

    public Zone(string name)
    {
        this.name = name;
        while(GameController.GetZoneByName(name) != null)
        {
            this.name = name + " copy";
        }
        
    }
}
