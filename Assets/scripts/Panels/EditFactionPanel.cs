using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditFactionPanel : EditDataPanel<Faction> {

    public InputField facNameField, turnPriorityField;

    public Image factionColorImg;



    public override void Open(Faction editedFaction)
    {
        //set data
        base.Open(editedFaction);
        facNameField.text = dataBeingEdited.name;
        turnPriorityField.text = dataBeingEdited.turnPriority.ToString();
        factionColorImg.color = dataBeingEdited.color;
    }

    public void CheckIfNameIsAlreadyInUse()
    {
        Faction sameNameFaction = GameController.GetFactionByName(facNameField.text);
        if (sameNameFaction != null && sameNameFaction.name != dataBeingEdited.name)
        {
            //TODO modal warning: name already in use, adding "copy" to name
            facNameField.text += " copy";
        }
    }

    public void CloseAndSaveChanges()
    {
        dataBeingEdited.name = facNameField.text;
        dataBeingEdited.color = factionColorImg.color;
        dataBeingEdited.turnPriority = int.Parse(turnPriorityField.text);
    }
}
