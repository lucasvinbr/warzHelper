using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour {

    public static GameInterface instance;

    public EditFactionPanel editFactionPanel;

    public EditZonePanel editZonePanel;

	public TextInputPanel textInputPanel;

	public ColorInputPanel colorInputPanel;

    public SaveListPanel saveListPanel;

    public ModeUI startOptionsPanel, templateOptionsPanel, gameOptionsPanel;

	private ModeUI curModeUI;

    public Color positiveUIColor, negativeUIColor, selectedUIElementColor, deselectedUIElementColor;

	public ZonesPanel zonesPanel;

	public FactionsPanel factionsPanel;

	/// <summary>
	/// a variable that should be incremented whenever an overlay panel is opened, and decremented whenever one closes.
	/// it can be used to check if a menu is open
	/// </summary>
	public static int openedPanelsOverlayLevel = 0;

	public static List<Dropdown.OptionData> factionDDownOptions = new List<Dropdown.OptionData>();
	public static List<Dropdown.OptionData> troopDDownOptions = new List<Dropdown.OptionData>();


	/// <summary>
	/// a variable that should always be set to true whenever a troop type is edited.
	/// this makes sure the troop type dropdown options are refreshed
	/// </summary>
	public static bool troopDDownsAreStale = true;

	/// <summary>
	/// a variable that should always be set to true whenever faction is edited.
	/// this makes sure that "pick a faction" dropdowns' options are refreshed
	/// </summary>
	public static bool factionDDownsAreStale = true;

	public enum InterfaceMode
    {
        start,
        template,
        game
    }

    void Awake()
    {
        instance = this;
    }

	public void ShowObject(GameObject obj)
    {
        obj.SetActive(true);
    }

    public void HideObject(GameObject obj)
    {
        obj.SetActive(false);
    }

    public void EditFaction(Faction targetFaction, bool isNewEntry)
    {
        editFactionPanel.Open(targetFaction, isNewEntry);
    }

    public void EditZone(Zone targetZone, bool isNewEntry)
    {
        editZonePanel.Open(targetZone, isNewEntry);
    }

    public void OpenLoadGameMenu(bool templateMode = false)
    {
        saveListPanel.OpenUp(!templateMode, "Select one of the saved entries", UIStartLoadGame);
    }

	public void UIStartLoadGame() {
		GameController.instance.LoadData(saveListPanel.PickedEntry.gameNameTxt.text, !saveListPanel.inGameMode);
	}

    /// <summary>
    /// hides the other modes' UI and shows the desired mode's one
    /// </summary>
    /// <param name="desiredMode"></param>
    public void SwitchInterface(InterfaceMode desiredMode)
    {
        switch (desiredMode)
        {
            case InterfaceMode.start:
                templateOptionsPanel.gameObject.SetActive(false);
                gameOptionsPanel.gameObject.SetActive(false);
                startOptionsPanel.gameObject.SetActive(true);
				curModeUI = startOptionsPanel;
                break;
            case InterfaceMode.game:
                templateOptionsPanel.gameObject.SetActive(false);
                gameOptionsPanel.gameObject.SetActive(true);
                startOptionsPanel.gameObject.SetActive(false);
				curModeUI = gameOptionsPanel;
                break;
            case InterfaceMode.template:
                templateOptionsPanel.gameObject.SetActive(true);
                gameOptionsPanel.gameObject.SetActive(false);
                startOptionsPanel.gameObject.SetActive(false);
				curModeUI = templateOptionsPanel;
                break;
        }
		curModeUI.ShowInitialUI();
	}

	public void ReturnToMenu() {
		ModalPanel.Instance().YesNoBox("Return to Main Menu", "Any unsaved changes will be lost.\n Proceed?", ()=> { curModeUI.ClearUI(); SwitchInterface(InterfaceMode.start); }, null);
	}


	#region dropdowns

	/// <summary>
	/// rebuilds troop dropdown data in order to make sure it's in sync with the available troop types
	/// </summary>
	public static void ReBakeFactionDDowns() {
		factionDDownOptions.Clear();
		if (GameController.GuardGameDataExist()) {
			factionDDownOptions.Add(new Dropdown.OptionData(Rules.NO_FACTION_NAME));
			List<Faction> factions = GameController.instance.curData.factions;
			for (int i = 0; i < factions.Count; i++) {
				factionDDownOptions.Add(new Dropdown.OptionData(factions[i].name));
			}
		}
		else {
			factionDDownOptions.Add(new Dropdown.OptionData("?"));
		}

		factionDDownsAreStale = false;
	}

	/// <summary>
	/// returns the dropdown index for the faction with the name specified, or -1 if it isn't found
	/// </summary>
	/// <param name="factionName"></param>
	/// <returns></returns>
	public static int GetDDownIndexForFaction(string factionName) {
		if (string.IsNullOrEmpty(factionName)) {
			factionName = Rules.NO_FACTION_NAME;
		}
		if (factionDDownsAreStale) {
			ReBakeFactionDDowns();
		}
		for (int i = 0; i < factionDDownOptions.Count; i++) {
			if (factionDDownOptions[i].text == factionName) {
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// rebuilds troop dropdown data in order to make sure it's in sync with the available troop types
	/// </summary>
	public static void ReBakeTroopTypeDDowns() {
		troopDDownOptions.Clear();
		if (GameController.GuardGameDataExist()) {
			List<TroopType> tTypes = GameController.instance.curData.troopTypes;
			for (int i = 0; i < tTypes.Count; i++) {
				troopDDownOptions.Add(new Dropdown.OptionData(tTypes[i].name));
			}
		}
		else {
			troopDDownOptions.Add(new Dropdown.OptionData("?"));
		}

		troopDDownsAreStale = false;
	}

	/// <summary>
	/// returns the dropdown index for the troop type with the name specified, or -1 if it isn't found
	/// </summary>
	/// <param name="troopTypeName"></param>
	/// <returns></returns>
	public static int GetDDownIndexForTType(string troopTypeName) {
		if (troopDDownsAreStale) {
			ReBakeTroopTypeDDowns();
		}
		for (int i = 0; i < troopDDownOptions.Count; i++) {
			if (troopDDownOptions[i].text == troopTypeName) {
				return i;
			}
		}
		return -1;
	}

	#endregion
}
