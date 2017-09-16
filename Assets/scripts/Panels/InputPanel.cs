using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour {

    public Text infoLabelText, actionButtonText;

    public Button actionButton;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(true);
    }

    public void SetPanelInfo(string infoLabelContent, string actionButtonText, UnityEvent onActionButtonPress)
    {
        infoLabelText.text = infoLabelContent;
        this.actionButtonText.text = actionButtonText;
        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(() => onActionButtonPress.Invoke());
    }
}
