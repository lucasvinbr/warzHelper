using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour {

    public TemplateInfo curData;

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
                    curData = new TemplateInfo(gameName);
                    PersistenceHandler.SaveToFile(curData, PersistenceHandler.templatesDirectory + gameName + ".xml");
                    Debug.Log("saved new template");
                }
                else
                {
                    curData = new GameInfo(gameName);
                    PersistenceHandler.SaveToFile(curData, PersistenceHandler.gamesDirectory + gameName + ".xml");
                    Debug.Log("saved new game");
                }

                GameInterface.instance.texInputPanel.Close();


            }


        });
        GameInterface.instance.texInputPanel.Open();
    }

	public void LoadData(string gameName, bool isTemplate = false)
    {
		string fileDir = (isTemplate ? PersistenceHandler.templatesDirectory : PersistenceHandler.gamesDirectory) + gameName + ".xml";
        curData = PersistenceHandler.LoadFromFile<TemplateInfo>(fileDir);
		if(curData != null) {
			Debug.Log("loaded game: " + curData.gameName);
			GameInterface.instance.HideObject(GameInterface.instance.saveListPanel.gameObject);
			GameInterface.instance.SwitchInterface(isTemplate ? GameInterface.InterfaceMode.template : GameInterface.InterfaceMode.game);
		}
    }

    public void SaveGame()
    {
        PersistenceHandler.SaveToFile(curData, curData.gameName, true);
    }


    public void GoToTemplate(TemplateInfo templateData)
    {
        GameInterface.instance.SwitchInterface(GameInterface.InterfaceMode.template);
    }

    void OnGameStart()
    {

    }

	public static void RemoveZone(Zone targetZone) {
		instance.curData.zones.Remove(targetZone);
		//TODO all zones should check their links
	}

	public static void RemoveFaction(Faction targetFaction) {
		instance.curData.factions.Remove(targetFaction);
		//TODO set all zones controlled by this faction as neutral
	}

	public static void RemoveTroopType(TroopType targetTroop) {
		instance.curData.troopTypes.Remove(targetTroop);
		//TODO remove all references to this type
	}

	public static int GetUnusedID() {
		instance.curData.lastIDGiven++;
		return instance.curData.lastIDGiven;
	}


	public static Faction GetFactionByName(string factionName)
    {
        List<Faction> factionList = instance.curData.factions;
        for (int i = 0; i < factionList.Count; i++)
        {
            if(factionList[i].name == factionName)
            {
                return factionList[i];
            }
        }

        return null;
    }

    public static Zone GetZoneByName(string zoneName)
    {
        List<Zone> zoneList = instance.curData.zones;
        for (int i = 0; i < zoneList.Count; i++)
        {
            if (zoneList[i].name == zoneName)
            {
                return zoneList[i];
            }
        }

        return null;
    }

	public static TroopType GetTroopTypeByName(string troopTypeName) {
		if (GuardGameDataExist()) {
			List<TroopType> trpList = instance.curData.troopTypes;
			for (int i = 0; i < trpList.Count; i++) {
				if (trpList[i].name == troopTypeName) {
					return trpList[i];
				}
			}
		}
		

		return null;
	}

	/// <summary>
	/// returns true if curData exists; shows a modal warning and returns false otherwise
	/// </summary>
	/// <returns></returns>
	public static bool GuardGameDataExist() {
		if(instance.curData != null) {
			return true;
		}
		else {
			ModalPanel.Instance().OkBox("No Game Data Found", "Unexpected: there is no current game data to save to/load from. Actions won't work properly. Please load or start a new game.");
			return false;
		}
	}

	/// <summary>
	/// returns the product of the baseZonePointAward, the owner faction's multPointAward for zones
	/// and the zoneMult (multTrainingPoints or multRecruitPoints, for example)
	/// </summary>
	/// <param name="ownerFaction"></param>
	/// <param name="zoneMult"></param>
	/// <returns></returns>
	public static int GetResultingPointsForZone(string ownerFaction, float zoneMult) {
		Faction ownerFac = GetFactionByName(ownerFaction);
		
		if(ownerFac != null) {
			return Mathf.RoundToInt(instance.curData.rules.baseZonePointAwardOnTurnStart * ownerFac.multZonePointAwardOnTurnStart * zoneMult);
		}
		else {
			return Mathf.RoundToInt(instance.curData.rules.baseZonePointAwardOnTurnStart * zoneMult);
		}
		
	}

	/// <summary>
	/// returns the product of the baseMaxUnitsPerZone, the owner faction's maxGarrMult
	/// and the zone's maxGarrMult
	/// </summary>
	/// <param name="ownerFaction"></param>
	/// <param name="zoneMult"></param>
	/// <returns></returns>
	public static int GetResultingMaxGarrForZone(string ownerFaction, float zoneMult) {
		Faction ownerFac = GetFactionByName(ownerFaction);

		if (ownerFac != null) {
			return Mathf.RoundToInt(instance.curData.rules.baseMaxUnitsInOneGarrison * ownerFac.multMaxUnitsInOneGarrison * zoneMult);
		}
		else {
			return Mathf.RoundToInt(instance.curData.rules.baseMaxUnitsInOneGarrison * zoneMult);
		}

	}
}
