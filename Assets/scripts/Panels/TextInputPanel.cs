using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TextInputPanel : InputPanel {

    public InputField theInputField;

    public void SetPanelInfo(string infoLabelContent, string inputPanelPlaceholderText, string inputPanelInitialText, string actionButtonText, UnityAction onActionButtonPress)
    {
        SetPanelInfo(infoLabelContent, actionButtonText, onActionButtonPress);
        (theInputField.placeholder as Text).text = inputPanelPlaceholderText;
        theInputField.text = inputPanelInitialText;
    }

    public void OnEnterPressedWhileTyping(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            actionButton.onClick.Invoke();
        }
    }
}
