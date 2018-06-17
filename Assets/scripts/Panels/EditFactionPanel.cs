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
        while (sameNameFaction != null && sameNameFaction.name != dataBeingEdited.name)
        {
            ModalPanel.Instance().OkBox("Name already in use", "A suffix will be added to this faction's name.");
            facNameField.text += " copy";
            sameNameFaction = GameController.GetFactionByName(facNameField.text);
        }
    }

    public void CloseAndSaveChanges()
    {
        dataBeingEdited.name = facNameField.text;
        dataBeingEdited.color = factionColorImg.color;
        dataBeingEdited.turnPriority = int.Parse(turnPriorityField.text);
    }
}
