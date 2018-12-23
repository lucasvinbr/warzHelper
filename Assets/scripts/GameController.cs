using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameController : MonoBehaviour {

    public TemplateInfo curData;

    public static GameController instance;

	public GameMaterialsHandler facMatsHandler;

	private TroopType m_lastRelevantTType;

	/// <summary>
	/// the last relevant troop type. It's the one that will be used when adding a new troop tier to a faction, for example.
	/// Could be the last troop type created or edited, or one automatically created by the GameController
	/// </summary>
	public TroopType LastRelevantTType
	{
		get
		{
			if(m_lastRelevantTType == null || !curData.troopTypes.Contains(m_lastRelevantTType)) {
				//get the first troop from the available ones... or create a new one if no troops exist at all
				if(curData.troopTypes.Count > 0) {
					m_lastRelevantTType = curData.troopTypes[0];
				}
				else {
					m_lastRelevantTType = new TroopType("New Troop");
				}

			}

			return m_lastRelevantTType;
		}
		set
		{
			m_lastRelevantTType = value;
		}
	}

    void Awake()
    {
        instance = this;
    }

    public void StartNewGame(bool isTemplate)
    {
        GameInterface.instance.textInputPanel.SetPanelInfo("Please provide a name for this game", "Confirm", () =>
        {
            string gameName = GameInterface.instance.textInputPanel.theInputField.text;
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
					PersistenceHandler.CreateDirIfNotExists(PersistenceHandler.templatesDirectory);
                    PersistenceHandler.SaveToFile(curData, PersistenceHandler.templatesDirectory + gameName + ".xml");
                    Debug.Log("saved new template");
                }
                else
                {
                    curData = new GameInfo(gameName);
					PersistenceHandler.CreateDirIfNotExists(PersistenceHandler.gamesDirectory);
					PersistenceHandler.SaveToFile(curData, PersistenceHandler.gamesDirectory + gameName + ".xml");
                    Debug.Log("saved new game");
                }

                GameInterface.instance.textInputPanel.Close();

				GameInterface.instance.SwitchInterface(isTemplate ? GameInterface.InterfaceMode.template : GameInterface.InterfaceMode.game);
			}


        });
        GameInterface.instance.textInputPanel.Open();
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
        PersistenceHandler.SaveToFile(curData, (curData.isATemplate ? PersistenceHandler.templatesDirectory : PersistenceHandler.gamesDirectory) + curData.gameName + ".xml", true);
    }

	public void SaveAs() {
		bool isTemplate = curData.isATemplate;
		GameInterface.instance.textInputPanel.SetPanelInfo("Please provide a name for this save", "Confirm", () => {
			string gameName = GameInterface.instance.textInputPanel.theInputField.text;
			if (!PersistenceHandler.IsAValidFilename(gameName)) {
				ModalPanel.Instance().OkBox("Invalid name",
					"The name provided is invalid for a save. The name must follow the same rules that apply when you create a file.");
				return;
			}

			TemplateInfo existingData = null;

			if (isTemplate) {
				existingData = PersistenceHandler.LoadFromFile<TemplateInfo>(PersistenceHandler.templatesDirectory + gameName + ".xml");
			}
			else {
				existingData = PersistenceHandler.LoadFromFile<TemplateInfo>(PersistenceHandler.gamesDirectory + gameName + ".xml");
			}

			if (existingData != null) {
				ModalPanel.Instance().YesNoBox("Save Exists", "A save with the same name already exists. Overwrite?", null, () => { existingData = null; });
			}

			//even if there actually is data, we pretend there isn't in case we plan to overwrite
			if (existingData == null) {
				curData.gameName = gameName;
				if (isTemplate) {
					PersistenceHandler.SaveToFile(curData, PersistenceHandler.templatesDirectory + gameName + ".xml");
				}
				else {
					PersistenceHandler.SaveToFile(curData, PersistenceHandler.gamesDirectory + gameName + ".xml");
				}

				GameInterface.instance.textInputPanel.Close();

			}


		});
		GameInterface.instance.textInputPanel.Open();
	}



	public void GoToTemplate(TemplateInfo templateData)
    {
        GameInterface.instance.SwitchInterface(GameInterface.InterfaceMode.template);
    }

    void OnGameStart()
    {

    }

	/// <summary>
	/// removes the faction from the game data and sets all of the faction's owned zones to neutral
	/// </summary>
	/// <param name="targetFaction"></param>
	public static void RemoveFaction(Faction targetFaction) {
		foreach(Zone z in GetZonesOwnedByFaction(targetFaction)) {
			z.ownerFaction = -1;
			z.MyZoneSpot.RefreshDataDisplay();
		}
		instance.curData.factions.Remove(targetFaction);
		GameInterface.factionDDownsAreStale = true;
	}

	public static void RemoveZone(Zone targetZone) {
		foreach (Zone z in instance.curData.zones) {
			if (z.linkedZones.Contains(targetZone.ID)) {
				World.RemoveZoneLink(z, targetZone, true);
			}
		}

		instance.curData.zones.Remove(targetZone);
		//TODO all zones should check their links
		
	}

	public static void RemoveTroopType(TroopType targetTroop) {
		instance.curData.troopTypes.Remove(targetTroop);
		GameInterface.troopDDownsAreStale = true;
		//remove all references to this type
		foreach (Faction f in instance.curData.factions) {
			for(int i = 0; i < f.troopLine.Count; i++) {
				if(f.troopLine[i] == targetTroop.ID) {
					f.troopLine[i] = instance.LastRelevantTType.ID;
				}
			}
		}
	}

	#region game data getters

	public static int GetUnusedFactionID() {
		int freeID = 0;
		while(GetFactionByID(freeID) != null) {
			freeID++;
		}

		return freeID;
	}

	public static int GetUnusedZoneID() {
		int freeID = 0;
		while (GetZoneByID(freeID) != null) {
			freeID++;
		}

		return freeID;
	}

	public static int GetUnusedTroopTypeID() {
		int freeID = 0;
		while (GetTroopTypeByID(freeID) != null) {
			freeID++;
		}

		return freeID;
	}


	public static Faction GetFactionByID(int factionID) {
		List<Faction> factionList = instance.curData.factions;
		for (int i = 0; i < factionList.Count; i++) {
			if (factionList[i].ID == factionID) {
				return factionList[i];
			}
		}

		return null;
	}

	public static Zone GetZoneByID(int zoneID) {
		List<Zone> zoneList = instance.curData.zones;
		for (int i = 0; i < zoneList.Count; i++) {
			if (zoneList[i].ID == zoneID) {
				return zoneList[i];
			}
		}

		return null;
	}

	public static TroopType GetTroopTypeByID(int troopID) {
		List<TroopType> TTList = instance.curData.troopTypes;
		for (int i = 0; i < TTList.Count; i++) {
			if (TTList[i].ID == troopID) {
				return TTList[i];
			}
		}

		return null;
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
	/// returns the NO_FACTION_NAME if not found
	/// </summary>
	/// <param name="factionID"></param>
	/// <returns></returns>
	public static string GetFactionNameByID(int factionID) {
		List<Faction> factionList = instance.curData.factions;
		for (int i = 0; i < factionList.Count; i++) {
			if (factionList[i].ID == factionID) {
				return factionList[i].name;
			}
		}

		return Rules.NO_FACTION_NAME;
	}

	/// <summary>
	/// returns -1 if not found, which should mean "no faction"
	/// </summary>
	/// <param name="factionName"></param>
	/// <returns></returns>
	public static int GetFactionIDByName(string factionName) {
		List<Faction> factionList = instance.curData.factions;
		for (int i = 0; i < factionList.Count; i++) {
			if (factionList[i].name == factionName) {
				return factionList[i].ID;
			}
		}

		return -1;
	}

	public static List<Zone> GetZonesOwnedByFaction(Faction fac) {
		List<Zone> returnedList = new List<Zone>();
		foreach(Zone z in instance.curData.zones) {
			if(z.ownerFaction == fac.ID) {
				returnedList.Add(z);
			}
		}
		return returnedList;
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

		if (ownerFac != null) {
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

	public static bool AreZonesLinked(Zone z1, Zone z2) {
		foreach(int i in z1.linkedZones) {
			if(GetZoneByID(i) == z2) {
				return true;
			}
		}

		foreach (int i in z2.linkedZones) {
			if (GetZoneByID(i) == z1) {
				return true;
			}
		}

		return false;
	}

	#endregion

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


}
