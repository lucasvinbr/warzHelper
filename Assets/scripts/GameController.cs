using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour {

    public TemplateInfo curGameData;

    public static GameController instance;

    void Awake()
    {
        instance = this;
    }

    public void StartNewGame(bool isTemplate)
    {
        GameInterface.instance.texInputPanel.SetPanelInfo("Please provide a name for this game", "Confirm", () =>
        {
            string gameName = GameInterface.instance.texInputPanel.theInputField.text;
            if (!PersistenceHandler.IsAValidFilename(gameName))
            {
                ModalPanel.Instance().OkBox("Invalid name",
                    "The name provided is invalid for a save. The name must follow the same rules that apply when you create a file.");
                return;
            }

            TemplateInfo existingData = null;

            if (isTemplate)
            {
                existingData = PersistenceHandler.LoadFromFile<TemplateInfo>(PersistenceHandler.templatesDirectory + gameName + ".xml");
            }
            else
            {
                existingData = PersistenceHandler.LoadFromFile<TemplateInfo>(PersistenceHandler.gamesDirectory + gameName + ".xml");
            }

            if(existingData != null)
            {
                ModalPanel.Instance().YesNoBox("Save Exists", "A save with the same name already exists. Overwrite?", null, () => { existingData = null; });
            }

            //even if there actually is data, we pretend there isn't in case we plan to overwrite
            if (existingData == null)
            {
                if (isTemplate)
                {
                    curGameData = new TemplateInfo(gameName);
                    PersistenceHandler.SaveToFile(curGameData, PersistenceHandler.templatesDirectory + gameName + ".xml");
                    Debug.Log("saved new template");
                }
                else
                {
                    curGameData = new GameInfo(gameName);
                    PersistenceHandler.SaveToFile(curGameData, PersistenceHandler.gamesDirectory + gameName + ".xml");
                    Debug.Log("saved new game");
                }

                GameInterface.instance.texInputPanel.Close();


            }


        });
        GameInterface.instance.texInputPanel.Open();
    }

	public void LoadGame(string gameName)
    {
        curGameData = PersistenceHandler.LoadFromFile<GameInfo>(gameName);
    }

    public void SaveGame()
    {
        PersistenceHandler.SaveToFile(curGameData, curGameData.gameName, true);
    }


    public void GoToTemplate(TemplateInfo templateData)
    {
        GameInterface.instance.SwitchInterface(GameInterface.InterfaceMode.template);
    }

    void OnGameStart()
    {

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
