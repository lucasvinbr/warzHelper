using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// contains entries with export actions
/// </summary>
public class ExportOptionsPanel : ListContainerPanel<UnityAction> {

	public Text panelTitle;


	public void Open(string title, List<KeyValuePair<string, UnityAction>> optionsList) {
		gameObject.SetActive(true);
		panelTitle.text = title;
		foreach(KeyValuePair<string, UnityAction> kvp in optionsList) {
			AddNamedEntry(kvp.Key, kvp.Value);
		}
	}

	/// <summary>
	/// addentry, but sets the btn's text as well
	/// </summary>
	/// <param name="entryName"></param>
	/// <param name="entryAction"></param>
	public void AddNamedEntry(string entryName, UnityAction entryAction) {
		ExportOptionsPanelListEntry addedEntry = AddEntry(entryAction) as ExportOptionsPanelListEntry;
		addedEntry.btnTxt.text = entryName;
	}

}
