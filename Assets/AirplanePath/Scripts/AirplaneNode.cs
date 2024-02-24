using UnityEngine;
using System.Collections;
using System.Runtime.CompilerServices;

[ExecuteInEditMode]
public class AirplaneNode : MonoBehaviour 
{
	public Vector3 Position {get{return transform.position;}}
	public float tangentStrength = 1;
	public float tangentAngleOffset = 0;
	[HideInInspector]
	public Vector3[] controlPoints;
	[HideInInspector]
	public AirplanePath path;

	public Vector3 this[int i]
	{
		get
		{
			return controlPoints[i] + Position;
		}
		
	}
	
	void OnDestroy()
	{
		if(path != null)
		{
			path.OnDestroyedNode(this);
		}
	}
}

