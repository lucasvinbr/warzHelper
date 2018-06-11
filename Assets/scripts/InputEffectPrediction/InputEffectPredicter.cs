using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// predicted result texts are an attempt to show the effect of a value provided in the input field.
/// for example, a multiplier is being set in a field; a predicted result text would attempt to retrieve the base value
/// and multiply it by the multiplier.
/// this script expects to be placed alongside the input field, with the predicted text as its next sibling
/// </summary>
public class InputEffectPredicter : MonoBehaviour {

	public Text predictedEffectText;
	public InputField theInput;

	public bool attachEventToInputOnStart = true;

	protected void Reset() {
		theInput = GetComponent<InputField>();
		if (transform.parent && transform.parent.childCount > transform.GetSiblingIndex() + 1) {
			Transform nextSib = transform.parent.GetChild(transform.GetSiblingIndex() + 1);
			if (nextSib) {
				predictedEffectText = nextSib.GetComponent<Text>();
			}
		}
	}

	protected void Start() {
		if (attachEventToInputOnStart && theInput) {
			theInput.onValueChanged.AddListener(WriteProjection);
			theInput.onValueChanged.Invoke(theInput.text);
		}
	}

	public virtual void WriteProjection(string inputText) {
		if (predictedEffectText) {
			predictedEffectText.text = "PROJECTED";
		}
	}
}
