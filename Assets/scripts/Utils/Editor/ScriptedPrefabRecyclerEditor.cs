using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimplePrefabRecycler), true)]
public class PrefabRecyclerEditor : Editor
{
	//public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
	//	// Using BeginProperty / EndProperty on the parent property means that
	//	// prefab override logic works on the entire property.
	//	EditorGUI.BeginProperty(position, label, property);

	//	// Draw label
	//	position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

	//	// Don't make child fields be indented
	//	var indent = EditorGUI.indentLevel;
	//	EditorGUI.indentLevel = 0;

	//	// Calculate rects
	//	var amountRect = new Rect(position.x, position.y, 30, position.height);
	//	var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
	//	var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

	//	// Draw fields - passs GUIContent.none to each so they are drawn without labels
	//	EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("objPrefab"));
	//	EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("activeObjsParent"));
	//	EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("pooledObjsParent"));

	//	// Set indent back to what it was
	//	EditorGUI.indentLevel = indent;

	//	EditorGUI.EndProperty();
	//}

	public override void OnInspectorGUI() {
		DirtableOverlayPanel dirtablePanel = target as DirtableOverlayPanel;

		DrawDefaultInspector();

		EditorGUILayout.Space();

		if (GUILayout.Button("Add SetDirty to all child inputfields' OnValueChanged", GUILayout.ExpandHeight(true), GUILayout.Height(50))) {
			//UnityAction<string> dirtyAction = new UnityAction<string>(dirtablePanel.MarkDirty);
			//bool alreadyListensToAction = false;
			//foreach (InputField IF in dirtablePanel.GetComponentsInChildren<InputField>(true)) {
			//	alreadyListensToAction = false;
			//	for (int i = 0; i < IF.onValueChanged.GetPersistentEventCount(); i++) {
			//		if (IF.onValueChanged.GetPersistentMethodName(i) == "MarkDirty") {
			//			alreadyListensToAction = true;
			//			break;
			//		}
			//	}
			//	if (alreadyListensToAction) {
			//		continue;
			//	}
			//	UnityEventTools.AddPersistentListener(IF.onValueChanged, dirtyAction);
			//	Debug.Log("added event to IF: " + IF.transform.name);
			//}
		}
	}
}
