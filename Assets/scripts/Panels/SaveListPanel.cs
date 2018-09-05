using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SaveListPanel : ListContainerPanel<TemplateInfo> {

    public Text promptText;

    public bool inGameMode = false;

    public Button confirmBtn, deleteBtn;

    protected SavePanelListEntry _pickedEntry;

    protected UnityAction actionOnConfirmSelection;

    public SavePanelListEntry PickedEntry
    {
        get
        {
            return _pickedEntry;
        }
        set
        {
            _pickedEntry = value;
            confirmBtn.interactable = _pickedEntry != null;
            deleteBtn.interactable = _pickedEntry != null;
        }
    }

    List<TemplateInfo> savesList = new List<TemplateInfo>();

    /// <summary>
    /// sets this panel as active and shows either saved templates or saved games
    /// </summary>
    /// <param name="showGames"></param>
    public void OpenUp(bool showGames, string promptText, UnityAction onConfirmPickedEntry = null)
    {
        this.promptText.text = promptText;
        inGameMode = showGames;
		actionOnConfirmSelection = onConfirmPickedEntry;
        gameObject.SetActive(true);
    }

    public override void ClearList()
    {
        base.ClearList();
        savesList?.Clear();
    }

    public override void OnEnable()
    {
		PickedEntry = null;
		base.OnEnable();
    }

	public override void FillEntries() {
		if (inGameMode) {
			savesList = PersistenceHandler.LoadFromAllFilesInDirectory<TemplateInfo>(PersistenceHandler.gamesDirectory);
		}
		else {
			savesList = PersistenceHandler.LoadFromAllFilesInDirectory<TemplateInfo>(PersistenceHandler.templatesDirectory);
		}

		if (savesList != null) {
			for (int i = 0; i < savesList.Count; i++) {
				AddEntry(savesList[i]);
			}
		}
	}

	public void SelectEntry(SavePanelListEntry chosenEntry)
    {
        if(PickedEntry != null)
        {
            PickedEntry.ToggleBackgroundHighlightColor(false);
        }
        PickedEntry = chosenEntry;
        PickedEntry.ToggleBackgroundHighlightColor(true);
    }

    public void ConfirmSelection()
    {
        if(actionOnConfirmSelection != null && PickedEntry != null)
        {
            actionOnConfirmSelection();
			actionOnConfirmSelection = null;
        }
    }

    public void DeleteSelected()
    {
        if(PickedEntry != null)
        {
            ModalPanel.Instance().YesNoBox("Confirm deletion", "You are about to delete the save " + PickedEntry.gameNameTxt.text +
                ". Are you sure?", () => {
                    if (inGameMode)
                    {
                        PersistenceHandler.DeleteFile(PersistenceHandler.gamesDirectory + PickedEntry.gameNameTxt.text + ".xml");
                    }
                    else
                    {
                        PersistenceHandler.DeleteFile(PersistenceHandler.templatesDirectory + PickedEntry.gameNameTxt.text + ".xml");
                    }

					OnEnable();
                }, null);
        }
    }
}
