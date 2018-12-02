using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine.Events;

[CustomEditor(typeof(DirtableOverlayPanel), true)]
public class DirtablePanelInspector : Editor {

	public override void OnInspectorGUI() {
		DirtableOverlayPanel dirtablePanel = target as DirtableOverlayPanel;

		DrawDefaultInspector();

		EditorGUILayout.Space();

		if(GUILayout.Button("Add SetDirty to all child inputfields' OnValueChanged", GUILayout.ExpandHeight(true), GUILayout.Height(50))) {
			UnityAction<string> dirtyAction = new UnityAction<string>(dirtablePanel.MarkDirty);
			bool alreadyListensToAction = false;
			foreach (InputField IF in dirtablePanel.GetComponentsInChildren<InputField>(true)) {
				alreadyListensToAction = false;
				for(int i = 0; i < IF.onValueChanged.GetPersistentEventCount(); i++) {
					if(IF.onValueChanged.GetPersistentMethodName(i) == "MarkDirty") {
						alreadyListensToAction = true;
						break;
					}
				}
				if (alreadyListensToAction) {
					continue;
				}
				UnityEventTools.AddPersistentListener(IF.onValueChanged, dirtyAction);
				Debug.Log("added event to IF: " + IF.transform.name);
			}
		}
	}
}
