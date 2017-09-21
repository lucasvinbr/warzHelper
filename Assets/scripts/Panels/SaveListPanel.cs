using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveListPanel : ListContainerPanel<TemplateInfo> {

    public bool inGameMode = false;

    List<TemplateInfo> savesList = new List<TemplateInfo>();

    /// <summary>
    /// sets this panel as active and shows either saved templates or saved games
    /// </summary>
    /// <param name="showGames"></param>
    public void OpenUp(bool showGames)
    {
        inGameMode = showGames;
        gameObject.SetActive(true);
    }

    protected override void ClearList()
    {
        base.ClearList();
        savesList.Clear();
    }

    protected override void OnEnable()
    {
        ClearList();

        if (inGameMode)
        {
            savesList = PersistenceHandler.LoadFromAllFilesInDirectory<TemplateInfo>(PersistenceHandler.gamesDirectory);
        }
        else
        {
            savesList = PersistenceHandler.LoadFromAllFilesInDirectory<TemplateInfo>(PersistenceHandler.templatesDirectory);
        }

        for(int i = 0; i < savesList.Count; i++)
        {
            AddEntry(savesList[i]);
        }
    }
}
