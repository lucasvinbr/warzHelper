using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactionsRelationPanelListEntry : ListPanelEntry<Faction> {

    public Text nameTxt;

    public Image backgroundImg;

	public Slider relationSlider;

	public Text curRelationTxt;

	public Faction thePanelsCurFaction;

	public override void SetContent(Faction targetFaction)
    {
        myContent = targetFaction;
        nameTxt.text = targetFaction.name;
        backgroundImg.color = targetFaction.color;
    }

	/// <summary>
	/// does some other required setup steps and sets the slider's value to the current relation value
	/// </summary>
	/// <param name="theOtherFac"></param>
	public void SetSliderAccordingToRelationWith(Faction theOtherFac) {
		thePanelsCurFaction = theOtherFac;

		float ourRelations = myContent.GetRelationWith(theOtherFac);

		relationSlider.minValue = GameFactionRelations.MIN_RELATIONS;
		relationSlider.maxValue = GameFactionRelations.MAX_RELATIONS;

		relationSlider.value = ourRelations;

		curRelationTxt.text = GameFactionRelations.RelationValueToNiceName(ourRelations);
	}

	public void UpdateFactionRelationsAccordingToSlider(float newSliderValue) {
		myContent.SetRelationWith(thePanelsCurFaction, newSliderValue);
		curRelationTxt.text = GameFactionRelations.RelationValueToNiceName(newSliderValue);
	}
}
