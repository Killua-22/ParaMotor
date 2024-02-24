using UnityEngine;
using System.Collections;

public class PathOverEventArgs : System.EventArgs
{
	public Vector3 Velocity {get; set;}
	public float OffsetRollAngle {get; set;}
	public bool Deactivated {get; set;}

	public PathOverEventArgs(Vector3 velocity, float offsetRollAngle, bool deactivated) 
	{
		Deactivated = deactivated;
		Velocity = velocity;
		OffsetRollAngle = offsetRollAngle;
	}
}
