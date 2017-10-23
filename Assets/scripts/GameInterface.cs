using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface : MonoBehaviour {

    public static GameInterface instance;

    public EditFactionPanel editFactionPanel;

    public EditZonePanel editZonePanel;

	public TextInputPanel texInputPanel;

	public ColorInputPanel colorInputPanel;

    public SaveListPanel saveListPanel;

    public ModeUI startOptionsPanel, templateOptionsPanel, gameOptionsPanel;

    public Color positiveUIColor, negativeUIColor, selectedUIElementColor, deselectedUIElementColor;

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

    public void EditFaction(Faction targetFaction)
    {
        editFactionPanel.Open(targetFaction);
    }

    public void EditZone(Zone targetZone)
    {
        editZonePanel.Open(targetZone);
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
                startOptionsPanel.ShowInitialUI();
                break;
            case InterfaceMode.game:
                templateOptionsPanel.gameObject.SetActive(false);
                gameOptionsPanel.gameObject.SetActive(true);
                startOptionsPanel.gameObject.SetActive(false);
                gameOptionsPanel.ShowInitialUI();
                break;
            case InterfaceMode.template:
                templateOptionsPanel.gameObject.SetActive(true);
                gameOptionsPanel.gameObject.SetActive(false);
                startOptionsPanel.gameObject.SetActive(false);
                templateOptionsPanel.ShowInitialUI();
                break;
        }
    }
}
