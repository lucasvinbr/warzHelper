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
		//fill data entries with the data already defined for this zone

        ownerFactionDropdown.ClearOptions();
        //add dropdown options for each faction (and the 'no faction' option)
        int initiallyPickedOption = 0;
        ownerFactionDropdown.options.Add(new Dropdown.OptionData("none"));
        List<Faction> factions = GameController.instance.curData.factions;
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
		ModalPanel.Instance().YesNoBox("Confirm Zone Deletion", "You are about to delete this zone. Are you sure?", OnConfirmDelete, null);
    }

    public void EditZoneLocation()
    {
        //TODO menu closes, zone movement mode is activated.
        //menu opens again once a spot is defined
    }


	public override void OnCloseBtnClicked() {
		Zone existingData = GameController.GetZoneByName(dataBeingEdited.name);
		if (existingData != null) {
			ModalPanel.Instance().YesNoBox("Unsaved changes", "Save changes made to this zone?", SaveZoneEdits, CloseWithoutSaving);
		}
	}

	public void CloseWithoutSaving() {
		gameObject.SetActive(false);
		OnWindowIsClosing();
	}

	public void SaveZoneEdits() {
		//takes the panel's current data and sends it to the game's saved zone data
		ZoneSpot thisZoneSpot = World.instance.GetZoneSpotByZoneName(dataBeingEdited.name);
        dataBeingEdited.name = nameInput.text;
		dataBeingEdited.ownerFaction = ownerFactionDropdown.captionText.text;
		dataBeingEdited.coords = new Vector2(thisZoneSpot.transform.localPosition.x, thisZoneSpot.transform.localPosition.z);
		thisZoneSpot.RefreshDataDisplay();
		OnWindowIsClosing();
	}

	public override void OnDeleteBtnClicked() {
		base.OnDeleteBtnClicked();
	}

	public override void OnConfirmDelete() {
		//hide this panel!
		gameObject.SetActive(false);
		//delete this zone's entry in the game data... if there is one
		GameController.RemoveZone(dataBeingEdited);
		World.instance.GetZoneSpotByZoneName(dataBeingEdited.name).DeleteThisSpot();
		dataBeingEdited = null;
		OnWindowIsClosing();
	}

}
