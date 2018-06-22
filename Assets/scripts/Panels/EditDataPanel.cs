using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// a panel that should contain options to edit the target data.
/// it should also have a "close" and a "delete" button to save, delete or discard changes
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EditDataPanel<T> : MonoBehaviour{

    public T dataBeingEdited;

	public delegate void OnDoneEditing();

	public OnDoneEditing onDoneEditing;

    public virtual void Open(T editedData)
    {
        //set data
        dataBeingEdited = editedData;
    }

	/// <summary>
	/// ask if the user is sure about deleting what's being edited
	/// </summary>
	public virtual void OnDeleteBtnClicked() {
		ModalPanel.Instance().YesNoBox("Confirm Deletion", "You are about to delete this. Are you sure?", OnConfirmDelete, null);
	}

	/// <summary>
	/// calls OnWindowIsClosing and closes the window
	/// </summary>
	public virtual void OnConfirmDelete() {
		OnWindowIsClosing();
	}

	/// <summary>
	///if this panel has been opened because this is a new object we're adding,
	///closing this panel should save immediately.
	///if the object already exists and we're editing it,
	///a modal asking if we want to keep/discard changes should show up.
	///onDoneEditing should be called afterwards
	/// </summary>
	public virtual void OnCloseBtnClicked() {
		Debug.Log("SAVE CHANGES?");
	}

	/// <summary>
	/// calls onDoneEditing and then sets it to null
	/// </summary>
	public virtual void OnWindowIsClosing() {
		onDoneEditing();
		onDoneEditing = null;
	}
}
