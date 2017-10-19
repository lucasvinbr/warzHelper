using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditZonePanel : EditDataPanel<Zone> {

    public InputField nameInput;
    public Dropdown ownerFactionDropdown;
    public InputField numGarrisonInput, numTroopGenInput, numIncomeInput;

    void OnEnable()
    {
        nameInput.text = dataBeingEdited.name;
        numGarrisonInput.text = dataBeingEdited.troopsGarrisoned.ToString();
        numTroopGenInput.text = dataBeingEdited.bonusTroopsGeneratedPerTurn.ToString();
        numIncomeInput.text = dataBeingEdited.incomeGeneratedPerTurn.ToString();

        ownerFactionDropdown.ClearOptions();
        //add dropdown options for each faction (and the 'no faction' option)
        int initiallyPickedOption = 0;
        ownerFactionDropdown.options.Add(new Dropdown.OptionData("none"));
        List<Faction> factions = GameController.instance.curGameData.factions;
        for (int i = 0; i < factions.Count; i++)
        {
            ownerFactionDropdown.options.Add(new Dropdown.OptionData(factions[i].name));
            if(dataBeingEdited.ownerFaction == factions[i].name)
            {
                initiallyPickedOption = i;
            }
        }

        ownerFactionDropdown.value = initiallyPickedOption;
    }

    public void DeleteZone()
    {
		ModalPanel.Instance().YesNoBox("Confirm Zone Deletion", "You are about to delete this zone. Are you sure?", OnZoneWillBeDeleted, null);
    }

    public void EditZoneLocation()
    {
        //TODO menu closes, zone movement mode is activated.
        //menu opens again once a spot is defined
    }

    public void OnCloseBtnClick()
    {
		//if this panel has been opened because this is a new zone we're adding,
		//closing this panel saves this zone immediately.
		//if this zone already exists and we're editing it,
		//a modal asking if we want to keep/discard changes should show up
		Zone existingData = GameController.GetZoneByName(dataBeingEdited.name);
        if (existingData != null) {
			ModalPanel.Instance().YesNoBox("Unsaved changes", "Save changes made to this zone?", SaveZoneEdits, CloseWithoutSaving);
		}
    }

	public void CloseWithoutSaving() {
		gameObject.SetActive(false);
		//TODO reset the zone's position: get it back from the data
	}

	public void SaveZoneEdits() {
		//takes the panel's current data and sends it to the game's saved zone data
		ZoneSpot thisZoneSpot = World.instance.GetZoneSpotByZoneName(dataBeingEdited.name);
        dataBeingEdited.name = nameInput.text;
		dataBeingEdited.ownerFaction = ownerFactionDropdown.captionText.text;
		dataBeingEdited.troopsGarrisoned = int.Parse(numGarrisonInput.text);
		dataBeingEdited.bonusTroopsGeneratedPerTurn = int.Parse(numTroopGenInput.text);
		dataBeingEdited.incomeGeneratedPerTurn = int.Parse(numIncomeInput.text);
		dataBeingEdited.coords = new Vector2(thisZoneSpot.transform.localPosition.x, thisZoneSpot.transform.localPosition.z);
		thisZoneSpot.RefreshDataDisplay();
	}

	public void OnZoneWillBeDeleted() {
		//hide this panel!
		gameObject.SetActive(false);
		//delete this zone's entry in the game data... if there is one
		GameController.RemoveZone(dataBeingEdited);
		World.instance.GetZoneSpotByZoneName(dataBeingEdited.name).DeleteThisSpot();
	}
}
