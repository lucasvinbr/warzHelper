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
        //modal "are you sure" box shows up and etc
    }

    public void EditZoneLocation()
    {
        //menu closes, zone movement mode is activated.
        //menu opens again once a spot is defined
    }

    public void OnCloseBtnClick()
    {
        //if this panel has been opened because this is a new zone we're adding,
        //closing this panel saves this zone immediately.
        //if this zone already exists and we're editing it,
        //a modal asking if we want to keep/discard changes should show up
    }
}
