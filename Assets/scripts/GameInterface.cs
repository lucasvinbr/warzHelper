using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface : MonoBehaviour {

    public static GameInterface instance;

    public EditFactionPanel editFactionPanel;

	public TextInputPanel texInputPanel;

	public ColorInputPanel colorInputPanel;

    public SaveListPanel saveListPanel;

    public Color positiveUIColor, negativeUIColor, selectedUIElementColor, deselectedUIElementColor;

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

    public void OpenLoadGameMenu(bool templateMode = false)
    {
        saveListPanel.OpenUp(!templateMode, "Select one of the saved entries");
    }
}
