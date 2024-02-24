using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AirplaneNode))]
public class AirplaneNodeInspector : Editor 
{
	AirplaneNode node;
	
	void Awake()
	{
		node = (AirplaneNode)target;
	}
	
	public override void OnInspectorGUI() 
	{
		DrawDefaultInspector();
		if(GUILayout.Button("Delete Node"))
		{
			var path = node.path;
			Undo.RecordObjects(node.path.GetAllObjects(), "Delete Node");
			node.path.OnDestroyedNode(node);
			Undo.IncrementCurrentGroup ();
			Undo.DestroyObjectImmediate(node.gameObject);
			EditorUtility.SetDirty(path);
		}
	}
	
	void OnSceneGUI()
	{
		TangentHandle();
	}
	
	void TangentHandle()
	{
		var path = node.path;
		if (path.showGizmos)
		{
			Handles.color = path.gizmoColors.nodeTangentsColor;
			EditorGUI.BeginChangeCheck();
			Vector3 pos0 = node[0];
			Vector3 pos1 = node[1];
			Vector3 dir0 = pos0 - node.Position;
			Vector3 dir1 = pos1 - node.Position;
			Vector3 normal;
			float dist0 = Vector3.Distance(pos0, node.Position);
			float dist1 = Vector3.Distance(pos1, node.Position);
			float angle0 = Mathf.Atan2(dir0.x, dir0.z) * Mathf.Rad2Deg;
			float angle1 = Mathf.Atan2(dir1.x, dir1.z) * Mathf.Rad2Deg;
			Vector3 handle0 = pos0;
			Vector3 handle1 = pos1;
			if (path.IsLast (node))
			{
				normal = Vector3.Cross(dir0, new Vector3(dir0.z, 0, -dir0.x)).normalized;
				Handles.DrawLine(pos0, node.Position);
				handle0 = Handles.FreeMoveHandle(pos0, Quaternion.identity, HandleUtility.GetHandleSize(pos0) * 0.15f, Vector3.zero, Handles.SphereHandleCap);
			}
			else if(path.IsFirst (node))
			{
				normal = Vector3.Cross(dir1, new Vector3(dir1.z, 0, -dir1.x)).normalized;
				Handles.DrawLine(node.Position, pos1);
				handle1 = Handles.FreeMoveHandle(pos1, Quaternion.identity, HandleUtility.GetHandleSize(pos1) * 0.15f, Vector3.zero, Handles.SphereHandleCap);
			}
			else
			{
				normal = Vector3.Cross(dir0, new Vector3(dir0.z, 0, -dir0.x)).normalized;
				Handles.DrawLine(pos0, node[1]);
				handle0 = Handles.FreeMoveHandle(pos0, Quaternion.identity, HandleUtility.GetHandleSize(pos0) * 0.15f, Vector3.zero, Handles.SphereHandleCap);
				handle1 = Handles.FreeMoveHandle(pos1, Quaternion.identity, HandleUtility.GetHandleSize(pos1) * 0.15f, Vector3.zero, Handles.SphereHandleCap);
			}
			
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(node, "Modify Tangent");
				
				Vector3 handleDir;
				Vector3 projPos = Vector3.zero;
				if (pos0 != handle0)
				{
					handleDir = handle0 - node.Position;
					projPos = (handleDir - Vector3.Project(handleDir, normal)) + node.Position;
					handleDir = projPos - node.Position;
					node.tangentAngleOffset = Mathf.Atan2(handleDir.x, handleDir.z) * Mathf.Rad2Deg - angle0 + node.tangentAngleOffset;
					node.tangentStrength = Mathf.Max(Vector3.Distance(projPos, node.Position) * node.tangentStrength / dist0, 0.01f);
				}
				else if (pos1 != handle1)
				{
					handleDir = handle1 - node.Position;
					projPos = (handleDir - Vector3.Project(handleDir, normal)) + node.Position;
					handleDir = projPos - node.Position;
					node.tangentAngleOffset += Mathf.Atan2(handleDir.x, handleDir.z) * Mathf.Rad2Deg - angle1;
					node.tangentStrength = Mathf.Max(Vector3.Distance(projPos, node.Position) * node.tangentStrength / dist1, 0.01f);
				}
				
				EditorUtility.SetDirty(node);
			}
		}
	}
}