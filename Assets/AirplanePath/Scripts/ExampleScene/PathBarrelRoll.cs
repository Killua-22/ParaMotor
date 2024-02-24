using UnityEngine;
using System.Collections;

public class PathBarrelRoll : MonoBehaviour 
{
	public float rollingSpeed = 0.5f;
	AirplanePath path;
	bool rolling;
	float t;
	
	void Start () 
	{
		//Get the AirplanePath of the aircraft
		path = GetComponent<AirplanePath>();
	}
	
	void Update () 
	{
		if (rolling)
		{
			Roll();
		}
		else if ((Input.GetKeyDown(KeyCode.B)) && path.Playing)
		{
			t = 0;
			rolling = true;
		}
	}

	void Roll()
	{
		t += Time.deltaTime * rollingSpeed;
		//Set RollOffset property over time to execute the barrel roll
		path.RollOffset = Mathf.Lerp (0, 360, t);
		if (t >= 1)
		{
			rolling = false;
			path.RollOffset = 0;
		}
	}
}
