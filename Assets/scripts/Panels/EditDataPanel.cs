using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EditDataPanel<T> : MonoBehaviour{

    public T dataBeingEdited;

    public virtual void Open(T editedData)
    {
        //set data
        dataBeingEdited = editedData;
    }
}
