using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public GameInfo curGameData;

    public static GameController instance;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }

    public void StartNewGame(bool isTemplate)
    {

    }


	public void LoadGame(string gameName)
    {
        curGameData = PersistenceHandler.LoadFromFile<GameInfo>(gameName);
    }

    public void SaveGame()
    {
        PersistenceHandler.SaveToFile(curGameData, curGameData.gameName, true);
    }

    public static Faction GetFactionByName(string factionName)
    {
        List<Faction> factionList = instance.curGameData.factions;
        for (int i = 0; i < factionList.Count; i++)
        {
            if(factionList[i].name == factionName)
            {
                return factionList[i];
            }
        }

        return null;
    }
}
