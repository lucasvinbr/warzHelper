using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInfo : TemplateInfo {

    public int lastTurnPriority;

    public int elapsedTurns = 0;

    public GameInfo(string name) : base(name)
    {
        this.gameName = name;
    }
}
