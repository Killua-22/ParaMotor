using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AirplanePath))]
public class AirplanePathInspector : Editor 
{
	AirplanePath path;
	
	void Awake()
	{
		path = (AirplanePath)target;
	}
	
	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Create Node"))
		{
			path.CreateNode ();
			EditorUtility.SetDirty(path);
		}
		if(GUILayout.Button("Fix Null Nodes"))
		{
			path.FixNullErrors();
			EditorUtility.SetDirty(path);
		}
		GUILayout.EndHorizontal();
	}
}