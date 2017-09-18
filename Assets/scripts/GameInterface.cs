using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInterface : MonoBehaviour {

    public static GameInterface instance;

    public EditFactionPanel editFactionPanel;

	public TextInputPanel texInputPanel;

	public ColorInputPanel colorInputPanel;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
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
}
