using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionsPanel : MonoBehaviour
{

    public Transform factionsListContainer;

    public GameObject factionListEntryPrefab;

    /// <summary>
    /// creates an entry in the faction list with the targetFaction's data
    /// </summary>
    /// <param name="targetFaction"></param>
    public void AddFactionToList(Faction targetFaction)
    {
        GameObject newEntry = Instantiate(factionListEntryPrefab);
        newEntry.transform.SetParent(factionsListContainer, false);
        newEntry.GetComponent<FactionsPanelListEntry>().SetContent(targetFaction);
    }

    /// <summary>
    /// destroys all of the list's entries
    /// </summary>
    public void ClearFactionList()
    {
        for (int i = 0; i < factionsListContainer.childCount; i++)
        {
            Destroy(factionsListContainer.GetChild(i).gameObject);
        }
    }

    void OnEnable()
    {
        ClearFactionList();
        List<Faction> factionList = GameController.instance.curGameData.factions;
        for (int i = 0; i < factionList.Count; i++)
        {
            AddFactionToList(factionList[i]);
        }
    }
}
