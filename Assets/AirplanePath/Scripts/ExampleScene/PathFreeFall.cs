using UnityEngine;
using System.Collections;

public class PathFreeFall : MonoBehaviour 
{
	public float gravity = 10;
	public float drag = 0.05f;
	public float rollWhenFalling = 100;

	AirplanePath path;
	bool falling;
	Vector3 velocity;
	float offsetAngle;
	float rollingSpeed = 0;
	
	void Start () 
	{
		//Get the AirplanePath of the aircraft
		path = GetComponent<AirplanePath>();

		//Wiring up BeginFall method to PathOver event
		path.PathOver += BeginFall;
	}

	/// <summary>
	/// Begins the free fall of the plane. Called when PathOver Event is raised.
	/// This method will get the physical state of the plane when the path stopped.
	/// </summary>
	/// <param name="sender">Sender.</param>
	/// <param name="args">Arguments.</param>
	void BeginFall(object sender, PathOverEventArgs args)
	{
		if (!args.Deactivated)
		{
			offsetAngle = args.OffsetRollAngle;
			velocity = args.Velocity;
			falling = true;
			rollingSpeed = 0;
		}
	}
	
	void Update () 
	{
		if (falling)
		{
			if (path.Playing) 
			{ 
				falling = false;
				return;
			}
			Fall();
		}
		else if ((Input.GetKeyDown(KeyCode.Space)) && path.Playing)
		{
			falling = true;
		}
	}

	/// <summary>
	/// Update the position and rotation of the plane when the plane falls. 
	/// Something similar could be done using a rigidbody and physic.
	/// </summary>
	void Fall()
	{
		//Makes the plane falls without using physics
		velocity -= (Vector3.up * gravity + velocity * drag) * Time.deltaTime;
		path.plane.position += velocity * Time.deltaTime;

		//Accelerates the rate at which the plane rolls when falling
		rollingSpeed += Time.deltaTime * rollWhenFalling;
		rollingSpeed = Mathf.Clamp(rollingSpeed, -360, 360);
		offsetAngle += Time.deltaTime * rollingSpeed;

		//Adjust the angle depending on velocity direction and roll
		Quaternion rotation = Quaternion.identity;
		rotation.SetLookRotation(velocity, Vector3.up);
		rotation *= Quaternion.Euler(new Vector3(0, 0, offsetAngle));

		path.plane.rotation = rotation;
	}
}
