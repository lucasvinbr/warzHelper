using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// a panel that should contain options to edit the target data.
/// it should also have a "close" and a "delete" button to save, delete or discard changes
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EditDataPanel<T> : GenericOverlayPanel{

    public T dataBeingEdited;

	public delegate void OnDoneEditing();

	public OnDoneEditing onDoneEditing;

	public Button closeBtn, deleteBtn;

	/// <summary>
	/// true if this panel was opened because we're creating a new object, not editing an existing one
	/// </summary>
	public bool creatingNewEntry = false;

	/// <summary>
	/// sets the menu's GO to active, sets dataBeingEdited to the target object and sets the close/delete buttons' texts to something relative to whether this is a new entry or not
	/// </summary>
	/// <param name="editedData"></param>
	/// <param name="isNewEntry"></param>
    public virtual void Open(T editedData, bool isNewEntry)
    {
		gameObject.SetActive(true);
        //set data
        dataBeingEdited = editedData;
		creatingNewEntry = isNewEntry;
		ContextualizeFinishBtns();
    }

	public virtual void ContextualizeFinishBtns() {
		if (closeBtn) {
			Text closeBtnText = closeBtn.GetComponentInChildren<Text>();
			if (closeBtnText) {
				if (creatingNewEntry) {
					closeBtnText.text = "Confirm Creation";
				}
				else {
					closeBtnText.text = "Close";
				}
			}
		}

		if (deleteBtn) {
			Text delBtnText = deleteBtn.GetComponentInChildren<Text>();
			if (delBtnText) {
				if (creatingNewEntry) {
					delBtnText.text = "Cancel Creation";
				}
				else {
					delBtnText.text = "Delete";
				}
			}
		}		
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
	/// returns false if any data entry contains invalid info (name is empty, for example); true otherwise
	/// </summary>
	/// <returns></returns>
	public virtual bool DataIsValid() {
		return true;
	}

	/// <summary>
	/// calls onDoneEditing and then sets it to null
	/// </summary>
	public virtual void OnWindowIsClosing() {
		onDoneEditing?.Invoke();
		onDoneEditing = null;
	}
}
