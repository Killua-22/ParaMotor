using UnityEngine;
using System.Collections;

public class PathRealisticSpeed : MonoBehaviour 
{
	//The more it is, the more influence slopes have on the speed
	public float mass = 1;
	public float minSpeed = 90;

	float baseSpeed;
	AirplanePath path;

	void Awake() 
	{
		path = GetComponent<AirplanePath>();
		if (path == null) 
		{
			Debug.LogWarning("Place the PathRealisticSpeed on the same GameObject as the AiplanePath script");
			enabled = false;
			return;
		}
		baseSpeed = path.speed;
		path.PathOver += HandlePathOver;
	}

	//Reset when path over
	void HandlePathOver (object sender, PathOverEventArgs e)
	{
		path.speed = baseSpeed;
	}

	//Asjusts the speed based on a simple physical model
	void Update() 
	{
		if (path.Playing)
		{
			var speed = path.speed;
			var speedRatio = speed / baseSpeed;
			var dragForce = -speedRatio * speedRatio;
			var thrustForce = 1;
			var gravityForce = -10 * path.Velocity.normalized.y * mass;
			var acceleration = dragForce + thrustForce + gravityForce;
			var newSpeed = path.speed + acceleration * Time.deltaTime;
			path.speed = Mathf.Max(newSpeed, minSpeed);
		}
	}
}