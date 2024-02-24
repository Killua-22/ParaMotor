using UnityEngine;
using System.Collections;

public class PathStarter : MonoBehaviour 
{
	AirplanePath path;
	bool hasFreeFall = false;

	void Start () 
	{
		//Get the AirplanePath of the aircraft
		path = GetComponent<AirplanePath>();

		if (GetComponent<PathFreeFall>() != null)
		{
			hasFreeFall = true;
		}
	}

	void Update () 
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (!path.Playing)
			{
				//On spacebar down, starts the aircraft
				path.StartPath ();
			}
			else if (hasFreeFall)
			{
				path.StopPath(false);
			}
		}
	}
}
