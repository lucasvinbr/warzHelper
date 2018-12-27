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
        GameInterface.instance.textInputPanel.SetPanelInfo("Please provide a name for this save", "Confirm", () =>
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
					GameInterface.instance.SwitchInterface(GameInterface.InterfaceMode.template);
				}
                else
                {
                    curData = new GameInfo(gameName);
					GameInterface.instance.OpenLoadTemplateForNewGame();

                }

                GameInterface.instance.textInputPanel.Close();

			}


        });
        GameInterface.instance.textInputPanel.Open();
    }

	public void LoadDataAndStartGame(string gameName, bool isTemplate = false)
    {
		string fileDir = (isTemplate ? PersistenceHandler.templatesDirectory : PersistenceHandler.gamesDirectory) + gameName + ".xml";
        curData = PersistenceHandler.LoadFromFile<TemplateInfo>(fileDir);
		if(curData != null) {
			Debug.Log("loaded game/template: " + curData.gameName);
			GameInterface.instance.HideObject(GameInterface.instance.saveListPanel.gameObject);
			GameInterface.instance.SwitchInterface(isTemplate ? GameInterface.InterfaceMode.template : GameInterface.InterfaceMode.game);
		}
    }

	/// <summary>
	/// gets a template's data and uses it in the creation of a new game.
	/// this method assumes the game has already been created and just needs the data import from
	/// the template
	/// </summary>
	/// <param name="templateName"></param>
	public void ImportTemplateDataAndStartGame(string templateName) {
		string fileDir = PersistenceHandler.templatesDirectory + templateName + ".xml";
		TemplateInfo loadedTemplate = PersistenceHandler.LoadFromFile<TemplateInfo>(fileDir);
		if (curData != null) {
			Debug.Log("loaded template for game: " + curData.gameName);
			GameInterface.instance.HideObject(GameInterface.instance.saveListPanel.gameObject);
		}
		(curData as GameInfo).ImportDataFromTemplate(loadedTemplate);
		PersistenceHandler.CreateDirIfNotExists(PersistenceHandler.gamesDirectory);
		PersistenceHandler.SaveToFile(curData, PersistenceHandler.gamesDirectory + curData.gameName + ".xml");
		Debug.Log("saved new game");
		GameInterface.instance.SwitchInterface(GameInterface.InterfaceMode.game);
	}

	/// <summary>
	/// runs a straightforward template load, nothing else
	/// </summary>
	/// <param name="gameName"></param>
	/// <returns></returns>
	public TemplateInfo TryLoadTemplate(string gameName) {
		string fileDir = PersistenceHandler.templatesDirectory + gameName + ".xml";
		return PersistenceHandler.LoadFromFile<TemplateInfo>(fileDir);
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


	#region game data removal
	/// <summary>
	/// removes the faction from the game data and sets all of the faction's owned zones to neutral
	/// </summary>
	/// <param name="targetFaction"></param>
	public static void RemoveFaction(Faction targetFaction) {
		foreach(Zone z in targetFaction.OwnedZones) {
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

	#endregion

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

	public static int GetUnusedCmderID() {
		int freeID = 0;
		while (GetCmderByID(freeID) != null) {
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

	public static Commander GetCmderByID(int cmderID) {
		List<Commander> cmderList = instance.curData.deployedCommanders;
		for (int i = 0; i < cmderList.Count; i++) {
			if (cmderList[i].ID == cmderID) {
				return cmderList[i];
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

	/// <summary>
	/// returns the faction which should be next in turn order.
	/// if there is no faction with higher TP, returns the first fac in the order
	/// </summary>
	/// <param name="turnPriotity"></param>
	/// <returns></returns>
	public static Faction GetNextFactionInTurnOrder(int turnPriotity) {
		Faction returnedFac = null;

		foreach(Faction f in instance.curData.factions) {
			if(f.turnPriority > turnPriotity) {
				if(returnedFac != null) {
					if(returnedFac.turnPriority > f.turnPriority) {
						returnedFac = f;
					}
				}
				else {
					returnedFac = f;
				}
			}
		}

		if(returnedFac == null) {
			returnedFac = GetNextFactionInTurnOrder(-99999999);
		}

		return returnedFac;
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

	#endregion

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

	/// <summary>
	/// gets the zone spots from the provided zones
	/// </summary>
	/// <param name="zones"></param>
	/// <returns></returns>
	public List<ZoneSpot> ZonesToZoneSpots(List<Zone> zones) {
		List<ZoneSpot> returnedList = new List<ZoneSpot>();

		foreach(Zone z in zones) {
			returnedList.Add(z.MyZoneSpot);
		}

		return returnedList;
	}

	/// <summary>
	/// eliminates turn priority conflicts due to the same turn priority value
	/// being assigned to two or more factions
	/// </summary>
	public static void MakeFactionTurnPrioritiesUnique() {
		List<Faction> facList = instance.curData.factions;
		facList.Sort(SortByTurnPriority);
		int curComparedTP = facList[0].turnPriority;
		for(int i = 1; i < facList.Count; i++) {
			if(facList[i].turnPriority <= curComparedTP) {
				curComparedTP++;
				facList[i].turnPriority = curComparedTP;
			}
		}
	}

	public static int SortByTurnPriority(Faction x, Faction y) {
		return x.turnPriority.CompareTo(y.turnPriority);
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


}
