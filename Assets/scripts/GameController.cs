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
	/// shorthand for instance.curData as GameInfo
	/// </summary>
	public static GameInfo CurGameData
	{
		get
		{
			return instance.curData as GameInfo;
		}
	}

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
		GameInfo gData = CurGameData;
		if(gData != null) {
			gData.factionRelations.RemoveAllRelationEntriesWithFaction(targetFaction.ID);
		}
	}

	public static void RemoveZone(Zone targetZone) {
		foreach (Zone z in instance.curData.zones) {
			if (z.linkedZones.Contains(targetZone.ID)) {
				World.RemoveZoneLink(z, targetZone, true);
			}
		}

		MercCaravan localCaravan = GetMercCaravanInZone(targetZone.ID);
		if (localCaravan != null) RemoveMercCaravan(localCaravan);

		instance.curData.zones.Remove(targetZone);		
	}

	public static void RemoveMercCaravan(MercCaravan MC) {
		World.RemoveMercCaravan3d(MC.MeIn3d);
		instance.curData.mercCaravans.Remove(MC);
	}

	public static void RemoveCommander(Commander targetCmder) {
		World.RemoveCmder3d(targetCmder.MeIn3d);
		instance.curData.deployedCommanders.Remove(targetCmder);
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

	public static int GetUnusedMercCaravanID() {
		int freeID = 0;
		while (GetMercCaravanByID(freeID) != null) {
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

	public static MercCaravan GetMercCaravanByID(int mcID) {
		List<MercCaravan> mcList = instance.curData.mercCaravans;
		for (int i = 0; i < mcList.Count; i++) {
			if (mcList[i].ID == mcID) {
				return mcList[i];
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

	public static List<Commander> GetCommandersOfFactionInZone(Zone targetZone, Faction targetFac) {
		List<Commander> friendlyCommandersInTheZone = new List<Commander>();
		foreach (Commander cmder in targetFac.OwnedCommanders) {
			if (cmder.zoneIAmIn == targetZone.ID) {
				friendlyCommandersInTheZone.Add(cmder);
			}
		}

		return friendlyCommandersInTheZone;
	}

	/// <summary>
	/// can only get cmders that are also in the provided whitelist
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <param name="whitelist"></param>
	/// <returns></returns>
	public static List<Commander> GetCommandersOfFactionInZone(Zone targetZone, Faction targetFac, List<Commander> whitelist) {
		List<Commander> friendlyCommandersInTheZone = new List<Commander>();
		foreach (Commander cmder in targetFac.OwnedCommanders) {
			if (cmder.zoneIAmIn == targetZone.ID && whitelist.Contains(cmder)) {
				friendlyCommandersInTheZone.Add(cmder);
			}
		}

		return friendlyCommandersInTheZone;
	}

	public static List<Commander> GetCommandersOfFactionInZone(Zone targetZone, Faction targetFac, out List<TroopNumberPair> cmdersArmy) {
		List<Commander> friendlyCommandersInTheZone = new List<Commander>();
		cmdersArmy = new List<TroopNumberPair>();
		foreach (Commander cmder in targetFac.OwnedCommanders) {
			if (cmder.zoneIAmIn == targetZone.ID) {
				friendlyCommandersInTheZone.Add(cmder);
				cmdersArmy = GetCombinedTroopsFromTwoLists(cmdersArmy, cmder.troopsContained);
			}
		}

		return friendlyCommandersInTheZone;
	}

	/// <summary>
	/// gets both the target faction's cmders and any cmders from allied factions
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static List<Commander> GetCommandersOfFactionAndAlliesInZone(Zone targetZone, Faction targetFac) {
		return GetCommandersOfFactionAndAlliesInZone(targetZone, targetFac.ID);
	}

	public static List<Commander> GetCommandersOfFactionAndAlliesInZone(Zone targetZone, int targetFacID) {
		List<Commander> friendlyCommandersInTheZone = new List<Commander>();
		Faction curCmderFac = null;
		foreach (Commander cmder in GetCommandersInZone(targetZone)) {
			curCmderFac = GetFactionByID(cmder.ownerFaction);
			if (curCmderFac.ID == targetFacID || curCmderFac.GetStandingWith(targetFacID) ==
				GameFactionRelations.FactionStanding.ally) {
				friendlyCommandersInTheZone.Add(cmder);
			}
		}

		return friendlyCommandersInTheZone;
	}

	/// <summary>
	/// gets both the target faction's cmders and any cmders from allied factions
	/// that aren't also allied to the provided enemy faction
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static List<Commander> GetCommandersOfFactionAndAlliesInZone(Zone targetZone, Faction targetFac, Faction enemyFac) {
		List<Commander> friendlyCommandersInTheZone = new List<Commander>();
		Faction curCmderFac = null;
		foreach (Commander cmder in GetCommandersInZone(targetZone)) {
			curCmderFac = GetFactionByID(cmder.ownerFaction);
			if (curCmderFac == targetFac || (curCmderFac.GetStandingWith(targetFac) ==
				GameFactionRelations.FactionStanding.ally && 
				curCmderFac.GetStandingWith(enemyFac) != GameFactionRelations.FactionStanding.ally)) {
				friendlyCommandersInTheZone.Add(cmder);
			}
		}

		return friendlyCommandersInTheZone;
	}

	/// <summary>
	/// gets all commanders in the zone, optionally not getting commanders that are still tweening towards it
	/// (those already count as in the zone for gameplay purposes, but haven't visually arrived there yet)
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="dontGetTweeningCommanders"></param>
	/// <returns></returns>
	public static List<Commander> GetCommandersInZone(Zone targetZone, bool dontGetTweeningCommanders = false) {
		List<Commander> cmdersInZone = new List<Commander>();
		foreach (Commander cmder in instance.curData.deployedCommanders) {
			if (cmder.zoneIAmIn == targetZone.ID) {
				cmdersInZone.Add(cmder);
			}
		}

		if (dontGetTweeningCommanders) {
			List<TransformTweener.TransformTween> tweens =
				TransformTweener.instance.GetAllTweensTargetingZone(targetZone.MyZoneSpot);
			foreach(TransformTweener.TransformTween tween in tweens) {
				Cmder3d tweeningCmder3d = tween.movingTrans.GetComponent<Cmder3d>();
				if(tweeningCmder3d) cmdersInZone.Remove(tweeningCmder3d.data as Commander);
			}
		}

		return cmdersInZone;
	}

	public static MercCaravan GetMercCaravanInZone(int targetZoneID) {

		foreach(MercCaravan mc in instance.curData.mercCaravans) {
			if(mc.zoneIAmIn == targetZoneID) {
				return mc;
			}
		}

		return null;
	}

	/// <summary>
	/// returns the combined troops of all forces from the target faction in the zone,
	/// both from the zone's garrison and from any commanders in the zone
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetCombinedTroopsInZoneFromFaction(Zone targetZone,
		Faction targetFac, bool onlyCommanderArmies = false) {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();
		if (targetZone.ownerFaction == targetFac.ID && !onlyCommanderArmies) {
			returnedList.AddRange(targetZone.troopsContained);
		}

		foreach(Commander cmder in GetCommandersOfFactionInZone(targetZone, targetFac)) {
			returnedList = cmder.GetCombinedTroops(returnedList);
		}

		return returnedList;
	}

	/// <summary>
	/// returns the combined troops of all forces from the target faction AND its allies in the zone,
	/// both from the zone's garrison and from any commanders in the zone
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetCombinedTroopsInZoneFromFactionAndAllies(Zone targetZone,
		Faction targetFac, bool onlyCommanderArmies = false) {
		return GetCombinedTroopsInZoneFromFactionAndAllies
			(targetZone, targetFac != null ? targetFac.ID : -1, onlyCommanderArmies);
	}

	public static List<TroopNumberPair> GetCombinedTroopsInZoneFromFactionAndAllies(Zone targetZone,
		int targetFacID, bool onlyCommanderArmies = false) {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();
		if (targetZone.ownerFaction == targetFacID && !onlyCommanderArmies) {
			returnedList.AddRange(targetZone.troopsContained);
		}

		foreach (Commander cmder in GetCommandersOfFactionAndAlliesInZone(targetZone, targetFacID)) {
			returnedList = cmder.GetCombinedTroops(returnedList);
		}

		return returnedList;
	}

	/// <summary>
	/// just like GetCombinedTroopsInZoneFromFaction, 
	/// but considering everything that is NOT from the target faction
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <param name="onlyCommanderArmies"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetCombinedTroopsInZoneNotFromFaction(Zone targetZone,
		Faction targetFac, bool onlyCommanderArmies = false) {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();
		if (targetZone.ownerFaction != targetFac.ID && !onlyCommanderArmies) {
			returnedList.AddRange(targetZone.troopsContained);
		}

		foreach (Commander cmder in GetCommandersInZone(targetZone)) {
			if(cmder.ownerFaction != targetFac.ID) {
				returnedList = cmder.GetCombinedTroops(returnedList);
			}
		}

		return returnedList;
	}

	/// <summary>
	/// just like GetCombinedTroopsInZoneFromFaction, 
	/// but considering everything that is NOT ALLIED to the target faction
	/// </summary>
	/// <param name="targetZone"></param>
	/// <param name="targetFac"></param>
	/// <param name="onlyCommanderArmies"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetCombinedTroopsInZoneNotAlliedToFaction(Zone targetZone,
		Faction targetFac, bool onlyCommanderArmies = false) {
		Faction curCheckedFac = GetFactionByID(targetZone.ownerFaction);
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();
		if (!onlyCommanderArmies && targetZone.ownerFaction != targetFac.ID &&
			(curCheckedFac == null || curCheckedFac.GetStandingWith(targetFac) != GameFactionRelations.FactionStanding.ally)) {
			returnedList.AddRange(targetZone.troopsContained);
		}

		foreach (Commander cmder in GetCommandersInZone(targetZone)) {
			if (cmder.ownerFaction != targetFac.ID) {
				if (curCheckedFac == null || curCheckedFac.ID != cmder.ownerFaction)
					curCheckedFac = GetFactionByID(cmder.ownerFaction);

				if(curCheckedFac.GetStandingWith(targetFac) != GameFactionRelations.FactionStanding.ally) {
					returnedList = cmder.GetCombinedTroops(returnedList);
				}
			}
		}

		return returnedList;
	}

	/// <summary>
	/// basically joins two troop lists and returns the result
	/// </summary>
	/// <param name="theOtherContainerTroops"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetCombinedTroopsFromTwoLists(List<TroopNumberPair> xList, List<TroopNumberPair> yList) {
		List<TroopNumberPair> returnedList = new List<TroopNumberPair>();

		returnedList.AddRange(xList);

		int testedTroopIndex = -1;
		TroopNumberPair checkedTNP;
		foreach (TroopNumberPair tnp in yList) {
			testedTroopIndex = IndexOfTroopInTroopList(xList, tnp.troopTypeID);
			if (testedTroopIndex >= 0) {
				checkedTNP = returnedList[testedTroopIndex];
				checkedTNP.troopAmount += tnp.troopAmount;
				returnedList[testedTroopIndex] = checkedTNP;
			}
			else {
				returnedList.Add(tnp);
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

	/// <summary>
	/// returns a list with all zones owned by the faction that don't already have a commander in them
	/// and have a recruitment point multiplier greater than 0
	/// </summary>
	/// <param name="fac"></param>
	/// <returns></returns>
	public static List<Zone> GetZonesForNewCmdersOfFaction(Faction fac) {
		List<Zone> availableZones = fac.OwnedZones;
		if (availableZones.Count > 0) {
			bool zoneIsOccupied = false;
			List<Commander> factionCmders = fac.OwnedCommanders;
			for (int i = availableZones.Count - 1; i >= 0; i--) {
				zoneIsOccupied = false;
				foreach (Commander cmd in factionCmders) {
					if (cmd.zoneIAmIn == availableZones[i].ID) {
						zoneIsOccupied = true;
						break;
					}
				}

				if (zoneIsOccupied || availableZones[i].multRecruitmentPoints <= 0) {
					availableZones.RemoveAt(i);
				}
			}
		}

		return availableZones;
	}

	/// <summary>
	/// returns a list of zones owned by allies of target faction, but not by the faction itself
	/// </summary>
	/// <param name="fac"></param>
	/// <returns></returns>
	public static List<Zone> GetZonesOwnedByAlliesOfFac(Faction fac) {
		List<int> foundAlliedFacIDs = new List<int>();
		List<Zone> alliedZones = new List<Zone>();

		foreach(Zone z in instance.curData.zones) {
			if(z.ownerFaction != fac.ID && z.ownerFaction >= 0) {
				if (foundAlliedFacIDs.Contains(z.ownerFaction)) {
					alliedZones.Add(z);
				}else {
					if(fac.GetStandingWith(GetFactionByID(z.ownerFaction)) ==
						GameFactionRelations.FactionStanding.ally) {
						alliedZones.Add(z);
						foundAlliedFacIDs.Add(z.ownerFaction);
					}
				}
			}
		}

		return alliedZones;

	}

	/// <summary>
	/// returns zones not owned by any faction
	/// </summary>
	/// <returns></returns>
	public static List<Zone> GetNeutralZones() {
		List<Zone> neutralZones = new List<Zone>();

		foreach (Zone z in instance.curData.zones) {
			if (z.ownerFaction < 0) {
				neutralZones.Add(z);
			}
		}

		return neutralZones;

	}

	public static int GetTotalTroopAmountFromTroopList(List<TroopNumberPair> troopList) {
		int total = 0;
		for (int i = 0; i < troopList.Count; i++) {
			total += troopList[i].troopAmount;
		}
		return total;
	}

	public static float GetTotalAutocalcPowerFromTroopList(List<TroopNumberPair> troopList) {
		float total = 0;

		for (int i = 0; i < troopList.Count; i++) {
			total +=
				GetTroopTypeByID(troopList[i].troopTypeID).autoResolvePower *
				troopList[i].troopAmount;
		}

		return total;
	}


	/// <summary>
	/// from the provided army, gets random members until the target size is reached.
	/// returns the base army if the sample is of the same size or bigger
	/// </summary>
	/// <param name="baseArmy"></param>
	/// <returns></returns>
	public static List<TroopNumberPair> GetRandomSampleArmyFromArmy(List<TroopNumberPair> baseArmy, int sampleSize) {
		if(sampleSize > 0 && GetTotalTroopAmountFromTroopList(baseArmy) > sampleSize) {
			List<TroopNumberPair> returnedSample = new List<TroopNumberPair>();
			int addedTroops = 0;
			int randomTroopID = -1, randomTroopIndex;
			TroopNumberPair randomTNP;

			while(addedTroops < sampleSize) {
				//get random troop, add to returned sample.
				//let the same troop be picked any number of times,
				//no matter how many of them there are in the base army,
				//for that extra (bad) luck factor
				randomTroopID = baseArmy[Random.Range(0, baseArmy.Count)].troopTypeID;
				randomTroopIndex = IndexOfTroopInTroopList(returnedSample, randomTroopID);
				if(randomTroopIndex < 0) {
					returnedSample.Add(new TroopNumberPair(randomTroopID, 1));
				}else {
					randomTNP = returnedSample[randomTroopIndex];
					randomTNP.troopAmount++;
					returnedSample[randomTroopIndex] = randomTNP;
				}

				addedTroops++;
			}


			return returnedSample;
		}else {
			return baseArmy;
		}
	}

	/// <summary>
	/// gets a sample from the base army and,
	/// using the autoResolveBattleDieSides from the rules,
	/// gets a random power considering that sample's base power
	/// </summary>
	/// <returns></returns>
	public static float GetRandomBattleAutocalcPower(List<TroopNumberPair> baseArmy, int sampleSizeLimit = -1) {
		List<TroopNumberPair> sampleArmy = GetRandomSampleArmyFromArmy(baseArmy, sampleSizeLimit);

		return GetTotalAutocalcPowerFromTroopList(sampleArmy) * 
			Random.Range(1.0f, instance.curData.rules.autoResolveBattleDieSides);

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

	public static bool IsZoneLinkedToAnyZoneOfList(Zone z, List<Zone> targetList) {
		foreach(Zone checkedZone in targetList){
			if(checkedZone != z && AreZonesLinked(z, checkedZone)) {
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// returns -1 if it can't find that ID
	/// </summary>
	/// <param name="troopList"></param>
	/// <param name="troopID"></param>
	/// <returns></returns>
	public static int IndexOfTroopInTroopList(List<TroopNumberPair> troopList, int troopID) {

		for (int i = 0; i < troopList.Count; i++) {
			if(troopList[i].troopTypeID == troopID) {
				return i;
			}
		}

		return -1;
	}

	/// <summary>
	/// gets the zone spots from the provided zones
	/// </summary>
	/// <param name="zones"></param>
	/// <returns></returns>
	public static List<ZoneSpot> ZonesToZoneSpots(List<Zone> zones) {
		List<ZoneSpot> returnedList = new List<ZoneSpot>();

		foreach(Zone z in zones) {
			returnedList.Add(z.MyZoneSpot);
		}

		return returnedList;
	}

	/// <summary>
	/// gets the commander 3ds from the provided Commanders
	/// </summary>
	/// <param name="cmders"></param>
	/// <returns></returns>
	public static List<Cmder3d> CmdersToCmder3ds(List<Commander> cmders) {
		List<Cmder3d> returnedList = new List<Cmder3d>();

		foreach (Commander c in cmders) {
			returnedList.Add(c.MeIn3d);
		}

		return returnedList;
	}

	public static List<TroopContainer> CmdersToTroopContainers(List<Commander> cmders) {
		List<TroopContainer> returnedList = new List<TroopContainer>();

		foreach (Commander c in cmders) {
			returnedList.Add(c);
		}

		return returnedList;
	}



	/// <summary>
	/// eliminates turn priority conflicts due to the same turn priority value
	/// being assigned to two or more factions
	/// </summary>
	public static void MakeFactionTurnPrioritiesUnique() {
		List<Faction> facList = instance.curData.factions;
		facList.Sort(Faction.SortByTurnPriority);
		int curComparedTP = facList[0].turnPriority;
		for(int i = 1; i < facList.Count; i++) {
			if(facList[i].turnPriority <= curComparedTP) {
				curComparedTP++;
				facList[i].turnPriority = curComparedTP;
			}
		}
	}

	/// <summary>
	/// checks if it's worth to give the faction a turn.
	/// Factions without any commanders or zones just can't do anything,
	/// so no need to give them a turn...
	/// unless they're the last faction in turn order and we're in "unified" mode;
	/// in that case, their turn is important because it's when battles and other stuff happen
	/// </summary>
	/// <param name="fac"></param>
	/// <returns></returns>
	public static bool ShouldFactionGetATurn(Faction fac, bool lastFactionException = true) {
		return fac.OwnedCommanders.Count > 0 || fac.OwnedZones.Count > 0 || 
			(lastFactionException && CurGameData.unifyBattlePhase &&
			fac.ID == CurGameData.factions[CurGameData.factions.Count - 1].ID);
	}

	/// <summary>
	/// if the modal is confirmed, tries to share all zones equally between all remaining factions.
	/// the rest of the (zones / factions) division is set to neutral
	/// </summary>
	public void RandomizeZoneOwnerships() {
		ModalPanel.Instance().YesNoBox("Randomize All Zones?", "Are you sure? All zones will be shared equally, with no regard for current ownerships, and some zones may become neutral. Commanders won't be moved.",
			() => {
				TemplateInfo curData = instance.curData;
				//zones are first set to neutral, then shared; finally, their displays are reset
				foreach(Zone z in curData.zones) {
					z.ownerFaction = -1;
				}
				int zonesPerFaction = curData.zones.Count / curData.factions.Count;
				if(zonesPerFaction <= 0) {
					Debug.LogWarning("[RandomizeAllZones] There are more factions than zones! Some factions won't get zones");
					zonesPerFaction = 1;
				}

				List<Zone> availableZones = new List<Zone>(curData.zones);
				List<Zone> zonesGivenToCurFac = new List<Zone>();
				Zone candidateZone;
				foreach(Faction fac in curData.factions) {
					zonesGivenToCurFac.Clear();
					while(zonesGivenToCurFac.Count < zonesPerFaction && availableZones.Count > 0) {
						candidateZone = null;
						if(zonesGivenToCurFac.Count == 0) {
							candidateZone = availableZones[Random.Range(0, availableZones.Count)];
						}else {
							//try to get zones close to each other
							foreach(Zone z in availableZones) {
								if(IsZoneLinkedToAnyZoneOfList(z, zonesGivenToCurFac)) {
									candidateZone = z;
									break;
								}
							}
							//or just a random one if we fail
							if (candidateZone == null) candidateZone = availableZones[Random.Range(0, availableZones.Count)];
						}

						candidateZone.ownerFaction = fac.ID;
						zonesGivenToCurFac.Add(candidateZone);
						availableZones.Remove(candidateZone);
					}
				}

				//aaaand refresh all zones' displays
				foreach (Zone z in curData.zones) {
					z.MyZoneSpot.RefreshDataDisplay();
				}

			}, null);
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
