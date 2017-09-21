using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ModalPanel : MonoBehaviour
{

    public class QueuedModalInfo
    {
        public Sprite iconPic;
        public string title, question;
        public UnityAction yesEvent, noEvent, cancelEvent, okEvent;
        public bool iconActive;
        public modalMessageType messageType;

        

        public QueuedModalInfo(Sprite iconPic, string title, string question, UnityAction yesEvent, UnityAction noEvent, UnityAction cancelEvent,
            UnityAction okEvent, bool iconActive, modalMessageType messageType)
        {
            this.iconPic = iconPic;
            this.title = title;
            this.question = question;
            this.yesEvent = yesEvent;
            this.noEvent = noEvent;
            this.cancelEvent = cancelEvent;
            this.okEvent = okEvent;
            this.iconActive = iconActive;
            this.messageType = messageType;
        }
    }

    public Text Title;     //The Modal Window Title
    public Text Question;  //The Modal Window Question (or statement)
    public Button Button1;   //The first button
    public Button Button2;   //The second button
    public Button Button3;   //The third button
    public Image IconImage; //The Icon Image, if any

    public GameObject ModalPanelObject;       //Reference to the Panel Object
    private static ModalPanel MainModalPanel; //Reference to the Modal Panel, to make sure it's been included

    private List<QueuedModalInfo> queuedModals = new List<QueuedModalInfo>();

    public enum modalMessageType
    {
        YesNo,
        YesNoCancel,
        Ok,
    }

    public static ModalPanel Instance()
    {
        if (!MainModalPanel)
        {
            MainModalPanel = FindObjectOfType(typeof(ModalPanel)) as ModalPanel;
            if (!MainModalPanel)
            {
                Debug.LogError("There needs to be one active ModalPanel script on a GameObject in your scene.");
            }
        }
        return MainModalPanel;
    }

    public void MessageBox(Sprite IconPic, string Title, string Question, UnityAction YesEvent, UnityAction NoEvent, UnityAction CancelEvent, UnityAction OkEvent, bool IconActive, modalMessageType MessageType)
    {
        //if the panel is already open, we queue a new opening with the desired info.
        //the new panel will open as soon as the current one is closed
        if (ModalPanelObject.activeSelf)
        {
            queuedModals.Add(new QueuedModalInfo(IconPic, Title, Question, YesEvent, NoEvent, CancelEvent, OkEvent, IconActive, MessageType));
            return;
        }
        ModalPanelObject.SetActive(true);  //Activate the Panel; its default is "off" in the Inspector

        Button1.onClick.RemoveAllListeners();
        Button2.onClick.RemoveAllListeners();
        Button3.onClick.RemoveAllListeners();

        switch (MessageType)
        {
            case modalMessageType.YesNoCancel:
                //Button1 is on the far left; Button2 is in the center and Button3 is on the right
                //Each can be activated and labeled individually
                if(YesEvent != null) Button1.onClick.AddListener(YesEvent);
                Button1.onClick.AddListener(ClosePanel);
                Button1.GetComponentInChildren<Text>().text = "Yes";

                if(NoEvent != null) Button2.onClick.AddListener(NoEvent);
                Button2.onClick.AddListener(ClosePanel);
                Button2.GetComponentInChildren<Text>().text = "No";

                if(CancelEvent != null) Button3.onClick.AddListener(CancelEvent);
                Button3.onClick.AddListener(ClosePanel);
                Button3.GetComponentInChildren<Text>().text = "Cancel";

                Button1.gameObject.SetActive(true); //We always turn on ONLY the buttons we need, and leave the rest off
                Button2.gameObject.SetActive(true);
                Button3.gameObject.SetActive(true);
                break;
            case modalMessageType.YesNo:
                if(YesEvent != null) Button1.onClick.AddListener(YesEvent);
                Button1.onClick.AddListener(ClosePanel);
                Button1.GetComponentInChildren<Text>().text = "Yes";

                if(NoEvent != null) Button3.onClick.AddListener(NoEvent);
                Button3.onClick.AddListener(ClosePanel);
                Button3.GetComponentInChildren<Text>().text = "No";

                Button1.gameObject.SetActive(true);
                Button2.gameObject.SetActive(false);
                Button3.gameObject.SetActive(true);
                break;
            case modalMessageType.Ok:
                if(OkEvent != null) Button2.onClick.AddListener(OkEvent);
                Button2.onClick.AddListener(ClosePanel);
                Button2.GetComponentInChildren<Text>().text = "OK";

                Button1.gameObject.SetActive(false);
                Button2.gameObject.SetActive(true);
                Button3.gameObject.SetActive(false);
                break;
        }

        this.Title.text = Title;           //Fill in the Title part of the Message Box
        this.Question.text = Question;     //Fill in the Question/statement part of the Messsage Box
        if (IconActive)                    //If the Icon is active (true)...
        {
            this.IconImage.gameObject.SetActive(true);  //Turn on the icon,
            this.IconImage.sprite = IconPic;            //and assign the picture.
        }
        else
        {
            //this.IconImage.gameObject.SetActive(false); //Turn off the icon.
        }
    }

    public void MessageBox(QueuedModalInfo info)
    {
        MessageBox(info.iconPic, info.title, info.question, info.yesEvent, info.noEvent, info.cancelEvent, info.okEvent, info.iconActive, info.messageType);
    }

    /// <summary>
    /// basic yes/no window with yes/no actions
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <param name="windowText"></param>
    /// <param name="yesEvent"></param>
    /// <param name="noEvent"></param>
    public void YesNoBox(string windowTitle, string windowText, UnityAction yesEvent, UnityAction noEvent)
    {
        MessageBox(null, windowTitle, windowText, yesEvent, noEvent, null, null, false, modalMessageType.YesNo);
    }

    /// <summary>
    /// window used only to notify about something
    /// </summary>
    /// <param name="windowTitle"></param>
    /// <param name="windowText"></param>
    public void OkBox(string windowTitle, string windowText)
    {
        MessageBox(null, windowTitle, windowText, null,
            null, null, null, false, modalMessageType.Ok);
    }

    void ClosePanel()
    {
        ModalPanelObject.SetActive(false); //Close the Modal Dialog
        if (queuedModals.Count > 0)
        {
            MessageBox(queuedModals[0]);
            queuedModals.RemoveAt(0);
        }
    }
}
