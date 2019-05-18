using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

/// <summary>
/// an input panel that may contain more than one input. Input entries should be added after opening the panel
/// </summary>
public class CustomInputPanel : InputPanel {

	public GameObject textInputPrefab, numericInputPrefab;

	public Transform inputEntriesContainer;

	public override void Open()
    {
		base.Open();
		ClearInputEntries();
    }

	public virtual void ClearInputEntries() {
		for (int i = 0; i < inputEntriesContainer.childCount; i++) {
			Destroy(inputEntriesContainer.GetChild(i).gameObject);
		}
	}


	public InputField AddTextInput(string inputFieldPlaceholderText = "", string inputFieldInitialText = "", string inputFieldTooltip = "") {
		GameObject newInput = Instantiate(textInputPrefab, inputEntriesContainer);
		InputField IF = newInput.GetComponent<InputField>();
		(IF.placeholder as Text).text = inputFieldPlaceholderText;
		IF.text = inputFieldInitialText;

		TooltipTrigger TTT = newInput.GetComponent<TooltipTrigger>();

		if (string.IsNullOrEmpty(inputFieldTooltip)) {
			TTT.enabled = false;
		}else {
			TTT.text = inputFieldTooltip;
			TTT.enabled = true;
		}

		return IF;
	}

	public NumericInputFieldBtns AddNumericInput(string inputFieldPlaceholderText = "", bool integersOnly = false,
			float inputFieldInitialText = 0.0f, float minValue = 0.0f, float maxValue = 99.9f, float stepSize = 1.0f,
			string inputFieldTooltip = "") {
		GameObject newInput = Instantiate(numericInputPrefab, inputEntriesContainer);
		NumericInputFieldBtns numBtns = newInput.GetComponentInChildren<NumericInputFieldBtns>();
		(numBtns.targetField.placeholder as Text).text = inputFieldPlaceholderText;

		numBtns.targetField.contentType = integersOnly ? 
			InputField.ContentType.IntegerNumber : InputField.ContentType.DecimalNumber;

		numBtns.targetField.text = inputFieldInitialText.ToString(CultureInfo.InvariantCulture);

		numBtns.minValue = minValue;
		numBtns.maxValue = maxValue;
		numBtns.stepSize = stepSize;

		TooltipTrigger TTT = newInput.GetComponent<TooltipTrigger>();

		if (string.IsNullOrEmpty(inputFieldTooltip)) {
			TTT.enabled = false;
		}
		else {
			TTT.text = inputFieldTooltip;
			TTT.enabled = true;
		}

		return numBtns;
	}

}
