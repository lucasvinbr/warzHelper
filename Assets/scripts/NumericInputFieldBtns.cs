using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// script for the arrows in a numeric input field. expects to be placed in the parent of the buttons,
/// which should be a child of the target input field.
/// can also add events to ensure validation of the provided input
/// </summary>
public class NumericInputFieldBtns : MonoBehaviour {
	
	public Button upBtn, downBtn;

	public InputField targetField;

	public bool addEventToBtnsOnStart = true;

	public float minValue = 0, maxValue = 99, stepSize = 0.1f;

	private void Reset() {
		
		Transform upBtnTrans = transform.GetChild(0);
		if (upBtnTrans) {
			upBtn = upBtnTrans.GetComponent<Button>();
			Transform downBtnTrans = transform.GetChild(1);
			if (downBtnTrans) {
				downBtn = downBtnTrans.GetComponent<Button>();
			}
		}

		if (transform.parent) {
			targetField = transform.parent.GetComponent<InputField>();
			if (targetField) {
				if(targetField.contentType == InputField.ContentType.IntegerNumber) {
					stepSize = 1;
				}
			}
		}
	}

	// Use this for initialization
	void Start () {
		if (addEventToBtnsOnStart) {
			if (upBtn) {
				upBtn.onClick.AddListener(IncrementTheField);
			}
			if (downBtn) {
				downBtn.onClick.AddListener(DecrementTheField);
			}
			if (targetField) {
				targetField.onEndEdit.AddListener(ClampFieldValue);
			}
		}
		
	}

	public void IncrementTheField() {
		if (targetField) {
			if (targetField.contentType == InputField.ContentType.IntegerNumber) {
				int curValue = int.Parse(targetField.text);
				curValue = (int)Mathf.Min(curValue + stepSize, maxValue);
				targetField.text = curValue.ToString();
			}
			else {
				float curFValue = float.Parse(targetField.text, CultureInfo.InvariantCulture);
				curFValue = Mathf.Min(curFValue + stepSize, maxValue);
				targetField.text = curFValue.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	public void DecrementTheField() {
		if (targetField) {
			if (targetField.contentType == InputField.ContentType.IntegerNumber) {
				int curValue = int.Parse(targetField.text);
				curValue = (int) Mathf.Max(curValue - stepSize, minValue);
				targetField.text = curValue.ToString();
			}
			else {
				float curFValue = float.Parse(targetField.text, CultureInfo.InvariantCulture);
				curFValue = Mathf.Max(curFValue - stepSize, minValue);
				targetField.text = curFValue.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	public void ClampFieldValue(string fieldText) {
		if (targetField) {
			if (targetField.contentType == InputField.ContentType.IntegerNumber) {
				int curValue = int.Parse(fieldText);
				curValue = (int)Mathf.Clamp(curValue, minValue, maxValue);
				targetField.text = curValue.ToString();
			}
			else {
				float curFValue = float.Parse(fieldText, CultureInfo.InvariantCulture);
				curFValue = Mathf.Clamp(curFValue, minValue, maxValue);
				targetField.text = curFValue.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	/// <summary>
	/// sets the field's value to the minimum set in this script
	/// </summary>
	public void MinimizeField() {
		if (targetField) {
			if (targetField.contentType == InputField.ContentType.IntegerNumber) {
				targetField.text = minValue.ToString();
			}
			else {
				targetField.text = minValue.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

	/// <summary>
	/// sets the field's value to the maximum set in this script
	/// </summary>
	public void MaximizeField() {
		if (targetField) {
			if (targetField.contentType == InputField.ContentType.IntegerNumber) {
				targetField.text = maxValue.ToString();
			}
			else {
				targetField.text = maxValue.ToString(CultureInfo.InvariantCulture);
			}
		}
	}

}
