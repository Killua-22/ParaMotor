using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(AirplanePath.GizmoColors))]
public class GizmoColorsDrawer : PropertyDrawer 
{
	public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) 
	{
		EditorGUI.BeginProperty(position, label, property);
		GUIContent tangentLabel = new GUIContent("Tangents");
		float tangentLabelSize = GUI.skin.GetStyle("Label").CalcSize(tangentLabel).x;
		GUIContent pathLabel = new GUIContent("Path");
		float pathLabelSize = GUI.skin.GetStyle("Label").CalcSize(pathLabel).x;
		int separation = 10;
		
		
		int indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;
		
		Rect contentPosition = EditorGUI.PrefixLabel(position, label);
		
		float colorWidth = 0.5f * (contentPosition.width - separation - pathLabelSize - tangentLabelSize);
		
		contentPosition.width = colorWidth + pathLabelSize;
		var nextX = contentPosition.x + contentPosition.width + separation;
		
		EditorGUIUtility.labelWidth = pathLabelSize;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("pathColor"), pathLabel);
		
		contentPosition.width = colorWidth + tangentLabelSize;
		contentPosition.x = nextX;
		
		EditorGUIUtility.labelWidth = tangentLabelSize;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("nodeTangentsColor"), tangentLabel);
		EditorGUI.indentLevel = indent;
		
		EditorGUI.EndProperty();
	}
}
