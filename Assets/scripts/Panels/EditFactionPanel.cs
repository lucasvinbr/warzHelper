using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditFactionPanel : MonoBehaviour {

    public Faction factionBeingEdited;

    public InputField facNameField, turnPriorityField;

    public Image factionColorImg;



    public void Open(Faction editedFaction)
    {
        //set data
        factionBeingEdited = editedFaction;
        facNameField.text = factionBeingEdited.name;
        turnPriorityField.text = factionBeingEdited.turnPriority.ToString();
        factionColorImg.color = factionBeingEdited.color;
    }

    public void CheckIfNameIsAlreadyInUse()
    {
        Faction sameNameFaction = GameController.GetFactionByName(facNameField.text);
        if (sameNameFaction != null && sameNameFaction.name != factionBeingEdited.name)
        {
            //TODO modal warning: name already in use, adding "copy" to name
            facNameField.text += " copy";
        }
    }

    public void CloseAndSaveChanges()
    {
        factionBeingEdited.name = facNameField.text;
        factionBeingEdited.color = factionColorImg.color;
        factionBeingEdited.turnPriority = int.Parse(turnPriorityField.text);
    }
}
