using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;

/// <summary>
/// Airplane spline path controller.
/// </summary>
[AddComponentMenu("Airplane Path/Airplane Path")]
public class AirplanePath : MonoBehaviour 
{
	public enum PathLoopingType {Once, Looping}
	
	/// <summary>
	/// loopingType: Once: plane stops at the end of path, Looping: loops endlessly
	/// playAtStart: Should the plane path start when the scene starts
	/// deactivatePlaneOnStop: Should the GameObject be deactivated when the path ends
	/// showGizmos: Should the path be shown in editor
	/// GizmoColors: Colors of the different gizmos
	/// speed: The speed at which the plane goes along the path
	/// rollIntensity: How much the plane should roll (bank) when turning
	/// plane: The plane Transform
	/// nodes: Array containing the nodes. Should not be touched unless you want to do funky things, like a path going through the same nodes several times
	/// </summary>
	public PathLoopingType loopingType = PathLoopingType.Looping;
	public bool playAtStart = true;
	public bool deactivatePlaneOnStop = true;
	public bool destroyOnDeactivate = false;
	[FormerlySerializedAs("keepUpdatingPositions")]
	public bool useWorldSpaceMovement = false;
	public bool showGizmos = true;
	public GizmoColors gizmoColors = new GizmoColors();
	public float speed = 100;
	public float rollIntensity = 2.5f;
	public Transform plane;
	public AirplaneNode[] nodes = new AirplaneNode[0];
	public AdvancedSettings advancedSettings = new AdvancedSettings();
	

	public event System.EventHandler<PathOverEventArgs> PathOver;
	
	//The roll of the plane is smoothed to filter out jerks. You can change this value to change the behaviour, but is could produce unrealistic results.
	//I judged it was not a good idea to make this value easily changeable in the inspector. Replace the "const" with "public" to make it available from the inspector.
	//A smaller value is smoother but less responsive.
	const float rollSmoothing = 5;
	float smoothRoll = 0;

	//Normalized time [0,1] of the current spline section
	[HideInInspector] public float time = 0;

	//Number of line sections used to draw previews of your path in editor. A larger number will draw more precise previews but will be heavier to render.
	const int gizmoSplinePrecision = 20;
	
	//If the plane is moving, this value is true
	public bool Playing {get; set;}
	
	//Index of the first node of the current spline section
	public int Index {get; set;}

	//Instant Velocity of the plane
	public Vector3 Velocity {get; private set;}

	//Roll offset of the plane
	public float RollOffset {get; set;}

	//Last roll angle due to turn
	public float TurnRollAngle {get; private set;}
	
	//current control points
	Vector3 p0, p1, p2, p3;

	Vector3 lastSystemPosition;
	Vector3 lastSystemVelocity;
	
	[HideInInspector]
	public GameObject nodeParent;
	[HideInInspector]
	public Transform lastCreatedNode;
	
	#region Behaviour
	
	void Start()
	{
		SetControlPoints();
		if (playAtStart)
		{
			StartPath(0);
		}
	}
	
	/// <summary>
	/// Starts the path at index = 0. Automatically called at start if playAtStart is set.
	/// </summary>
	public void StartPath() {StartPath(0);}
	
	/// <summary>
	/// Starts the plane at set index.
	/// </summary>
	/// <param name="firstNodeIndex">First node index.</param>
	public void StartPath(int firstNodeIndex) 
	{
		if (CheckForProblems(firstNodeIndex)) {return;}
		plane.position = nodes[firstNodeIndex].Position;
		plane.gameObject.SetActive(true);
		
		time = 0;
		Index = firstNodeIndex;
		p0 = nodes[firstNodeIndex].Position;
		p1 = nodes[firstNodeIndex][1];
		p2 = nodes[firstNodeIndex + 1][0];
		p3 = nodes[firstNodeIndex + 1].Position;
		Playing = true;
	}
	
	/// <summary>
	/// Stops the plane and makes it disappear if deactivatePlaneOnStop is set.
	/// </summary>
	public void StopPath() {StopPath(deactivatePlaneOnStop);}

	/// <summary>
	/// Stops the plane.
	/// </summary>
	/// <param name="deactivatePlane">If set to <c>true</c> deactivate plane.</param>
	public void StopPath(bool deactivatePlane)
	{
		Playing = false;
		if (deactivatePlane) {plane.gameObject.SetActive(false);}
		if (PathOver != null) 
		{
			PathOver(this, new PathOverEventArgs(this.Velocity, this.TurnRollAngle + this.RollOffset, deactivatePlane));
		}
		if (deactivatePlane && destroyOnDeactivate) {Destroy(this.gameObject);}
	}
	
	/// <summary>
	/// Checking for common problems
	/// </summary>
	/// <returns><c>true</c>, if problems are found, <c>false</c> otherwise.</returns>
	/// <param name="firstNodeIndex">First node index.</param>
	bool CheckForProblems(int firstNodeIndex)
	{
		if (nodes.Length <= 1) {Debug.LogError("Not enough nodes created", this); return true;}
		
		else if (firstNodeIndex + 1 == nodes.Length && loopingType == PathLoopingType.Once) 
		{Debug.Log("Chosen starting node too high for Once looping type, plane starting at the end of path", this); return true;}
		
		else if (firstNodeIndex + 1 > nodes.Length) {Debug.LogError("Chosen starting node too high, node inexistant", this); return true;}
		
		else if (nodes.Length <= 2 && loopingType == PathLoopingType.Looping) 
		{Debug.Log("Too few nodes for Looping mode, which needs at least 3 nodes. Change looping type or add nodes", this); return true;}
		
		else if (plane == null) {Debug.Log("No aircraft GameObject set", this); return true;}
		
		return false;
	}
	
	void LogNanError()
	{
		Debug.LogError(this.ToString() + " disabled: Subsequent Airplane Nodes may be at the same position, which causes unexpected errors. " +
		               "Try moving nodes a little.", this);
		enabled = false;
	}
	
	void Update () 
	{
		if (Playing)
		{
			//Find Bezier speed
			Vector3 instantBezierVelocity = CalculateBezierDerivative(time, p0, p1, p2, p3);
			float instantBezierVelocityMagnitude = instantBezierVelocity.magnitude;

			if (instantBezierVelocityMagnitude == 0)
			{
				LogNanError(); 
				return;
			}
			
			// Gets the right delta time for constant speed
			float speedModifier = speed / instantBezierVelocityMagnitude;
			time += Time.deltaTime * speedModifier;

			if (useWorldSpaceMovement && time < 1)
			{
				var nextIndex = Index == nodes.Length - 1 ? 0 : Index + 1;
				p0 = nodes[Index].Position;
				p1 = nodes[Index][1];
				p2 = nodes[nextIndex][0];
				p3 = nodes[nextIndex].Position;
			}
			else
			{
				while (time >= 1)
				{
					int nextIndex = Index + 2;

					switch (loopingType)
					{
						case PathLoopingType.Looping:
							Index = Index == nodes.Length - 1 ? 0 : Index + 1;
							nextIndex = Index == nodes.Length - 1 ? 0 : Index + 1;
							break;
						case PathLoopingType.Once:
							Index = Index + 1;
							if (Index >= nodes.Length - 1)
							{
								StopPath(); 
								return;
							}
							nextIndex = Index + 1;
							break;
					}

					p0 = nodes[Index].Position;
					p1 = nodes[Index][1];
					p2 = nodes[nextIndex][0];
					p3 = nodes[nextIndex].Position;

					//Adapt delta Time for new section while keeping constant speed
					time -= 1;
					time *= instantBezierVelocityMagnitude;
					instantBezierVelocity = CalculateBezierDerivative(0, p0, p1, p2, p3);
					instantBezierVelocityMagnitude = instantBezierVelocity.magnitude;
					time /= instantBezierVelocityMagnitude;
					speedModifier = speed / instantBezierVelocityMagnitude;

				}
			}
			//Must Change Bezier Section

			Velocity = instantBezierVelocity * speedModifier;

			//Find Position
			plane.position = CalculateBezierPoint(time, p0, p1, p2, p3);
			Vector3 secondDerivative = Vector3.zero;
			Vector3 additionalAcceleration = Vector3.zero;
			if (useWorldSpaceMovement)
			{
				var deltaTime = Time.deltaTime;
				deltaTime = deltaTime == 0 ? float.MaxValue : deltaTime;
				var inverseTime = 1 / deltaTime;

				var systemPosition = transform.position;
				var systemVelocity = (systemPosition - lastSystemPosition) * inverseTime;
				Velocity += systemVelocity;
				lastSystemPosition = systemPosition;

				secondDerivative = (systemVelocity - lastSystemVelocity) * inverseTime;
				lastSystemVelocity = systemVelocity;
			}
			
			secondDerivative += CalculateBezierSecondDerivative(time, p0, p1, p2, p3);
			Quaternion rotation = Quaternion.identity;
			Vector3 right = new Vector3(Velocity.z, 0, -Velocity.x).normalized;
			
			if (Velocity == Vector3.zero) {LogNanError(); return;}

			float rollFactor = Vector3.Dot((secondDerivative * speedModifier + additionalAcceleration) * 0.001f, right);
			float rollAngle = CalculateRollAngle(rollFactor);
			rotation.SetLookRotation(Velocity, Vector3.up);// + rollIntensity * smoothRoll * right);
			rotation *= Quaternion.Euler(new Vector3(0, 0, rollAngle));
			plane.rotation = rotation;
		}
	}

	/// <summary>
	/// Calculates Roll and smoothes it (to compensates for non C2 continuous control points algorithm)	/// </summary>
	/// <returns>The roll angle.</returns>
	/// <param name="rollFactor">Roll factor.</param>
	float CalculateRollAngle(float rollFactor)
	{
		smoothRoll = Mathf.Lerp(smoothRoll, rollFactor, rollSmoothing * Time.deltaTime);
		float angle = Mathf.Atan2(1, smoothRoll * rollIntensity);
		angle *= Mathf.Rad2Deg;
		angle -= 90;

		TurnRollAngle = angle;

		angle += RollOffset;
		return angle;
	}
	
	#endregion
	
	#region Control Points Calculations
	
	/// <summary>
	/// Sets the intermediary control points (p1 and p2 of splines).
	/// </summary>
	public void SetControlPoints()
	{
		for(int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i] == null) {FixNullErrors(); break;}
		}
		if (nodes.Length <= 1) {return;}

		Vector3 tangent;
		switch (loopingType)
		{
		case PathLoopingType.Looping:
			for (int i = 0; i < nodes.Length; i++)
			{
				AirplaneNode previous = (i == 0) ? nodes[nodes.Length - 1] : nodes[i - 1];
				AirplaneNode next = (i == nodes.Length - 1) ? nodes[0] : nodes[i + 1];
				AirplaneNode current = nodes[i];
				tangent = CalculateTangent(previous.Position, next.Position, current.Position);
				tangent = Quaternion.Euler(0, nodes[i].tangentAngleOffset, 0) * tangent;
				current.controlPoints = new Vector3[2] 
				{
					-tangent * current.tangentStrength, 
					tangent * current.tangentStrength
				};
			}
			break;
		case PathLoopingType.Once:
			
			//For a simple line if there are only 2 nodes
			if (nodes.Length == 2) 
			{
				nodes[0].controlPoints = new Vector3[2]
				{
					Vector3.zero, 
					(nodes[1].Position - nodes[0].Position) * 0.5f
				};
				nodes[1].controlPoints = new Vector3[2]
				{
					(nodes[0].Position - nodes[1].Position) * 0.5f, 
					Vector3.zero
				};
				break;
			}

			//For internal nodes of a more complex path
			for (int i = 1; i < nodes.Length - 1; i++)
			{
				AirplaneNode previous = nodes[i - 1];
				AirplaneNode next = nodes[i + 1];
				AirplaneNode current = nodes[i];
				tangent = CalculateTangent(previous.Position, next.Position, current.Position);
				tangent = Quaternion.Euler(0, nodes[i].tangentAngleOffset, 0) * tangent;
				current.controlPoints = new Vector3[2] 
				{
					-tangent * current.tangentStrength,
					tangent * current.tangentStrength
				};
			}
			
			//For boundary nodes, (control point = half the distance between the node and the control point of the next node)
			var firstNode = nodes[0];
			tangent = (nodes[1][0] - firstNode.Position) * 0.5f;
			tangent = Quaternion.Euler(0, firstNode.tangentAngleOffset, 0) * tangent;
			firstNode.controlPoints = new Vector3[2]
			{
				Vector3.zero, 
				tangent * nodes[0].tangentStrength
			};
			
			var lastNode = nodes[nodes.Length - 1];
			tangent = (nodes[nodes.Length - 2][1] - lastNode.Position) * 0.5f;
			tangent = Quaternion.Euler(0, lastNode.tangentAngleOffset, 0) * tangent;
			lastNode.controlPoints = new Vector3[2]
			{
				tangent * lastNode.tangentStrength, 
				Vector3.zero
			};
			break;
			
		}
	}
	
	/// <summary>
	/// Calculates tangents for automatic control points. Mix of Catmull Rom algorithm and custom adjustment for realistic path.
	/// </summary>
	/// <returns>The tangent.</returns>
	/// <param name="previous">Previous.</param>
	/// <param name="next">Next.</param>
	/// <param name="current">Current.</param>
	Vector3 CalculateTangent(Vector3 previous, Vector3 next, Vector3 current)
	{
		Vector3 catmullTangent = (next - previous) * 0.5f;
		Vector3 distance = current - (next + previous) * 0.5f;
		Vector3 perpendicularTangent;

		if (advancedSettings.useLegacyPathAlgorithm)
		{
			perpendicularTangent = new Vector3(-distance.y, 0, distance.x);
		}
		else
		{
			perpendicularTangent = new Vector3(-distance.z, 0, distance.x);
		}
		
		perpendicularTangent = (Vector3.Dot(catmullTangent, perpendicularTangent) >= 0) ? perpendicularTangent : -perpendicularTangent;
		Vector3 result = perpendicularTangent * 0.5f + catmullTangent * 0.5f;
		return new Vector3(result.x, catmullTangent.y, result.z);
	}
	
	#endregion
	
	#region Bezier Calculations
	
	/// <summary>
	/// Calculates the bezier position.
	/// </summary>
	/// <returns>The bezier position.</returns>
	/// <param name="t">Normalized Time.</param>
	/// <param name="po">P0.</param>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	/// <param name="p3">P3.</param>
	static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;
		
		Vector3 p = uuu * p0; 		//first term
		p += 3 * uu * t * p1; 	//second term
		p += 3 * u * tt * p2; 		//third term
		p += ttt * p3; 				//fourth term
		
		return p;
	}
	
	/// <summary>
	/// Calculates the first bezier derivative.
	/// </summary>
	/// <returns>The first bezier derivative.</returns>
	/// <param name="t">Normalized Time.</param>
	/// <param name="po">P0.</param>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	/// <param name="p3">P3.</param>
	static Vector3 CalculateBezierDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		
		Vector3 p = -3 * uu * p0;			//first term
		p += (t * (9 * t - 12) + 3) * p1;	//second term
		p += 3 * t * (2 - 3 * t) * p2;		//third term
		p += 3 * tt * p3;					//fourth term
		
		return p;
	}
	
	/// <summary>
	/// Calculates the bezier second derivative.
	/// </summary>
	/// <returns>The bezier second derivative.</returns>
	/// <param name="t">Normalized Time.</param>
	/// <param name="po">P0.</param>
	/// <param name="p1">P1.</param>
	/// <param name="p2">P2.</param>
	/// <param name="p3">P3.</param>
	static Vector3 CalculateBezierSecondDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		float u = 1 - t;
		
		Vector3 p = 6 * u * p0;		//first term
		p += (18 * t - 12) * p1;	//second term
		p += (6 -18 * t) * p2;	//third term
		p += 6 * t * p3;			//fourth term
		
		return p;
	}
	
	#endregion
	
	#region Editor
	
	/// <summary>
	/// Sets the parent of the nodes.
	/// </summary>
	void SetPathParent()
	{
		if (nodeParent == null) 
		{
			Transform trans = transform.FindChild("Path");
			if (trans != null)
			{
				nodeParent = trans.gameObject;
			}
		}
		//Create the parent of the nodes, named "Path"
		if (nodeParent == null)
		{
			nodeParent = new GameObject("Path");
			var parentTransform = nodeParent.transform;
			parentTransform.parent = transform;
			parentTransform.localPosition = Vector3.zero;
			parentTransform.localRotation = Quaternion.identity;
		}
	}
	
	/// <summary>
	/// Creates a Node, designed to be used in editor.
	/// Not tested to be used at runtime, use at your own risk.
	/// If you do try to use it, you should probably call SetControlPoints() afterwards.
	/// Contact us for more information if you want to try funky things.
	/// </summary>
	public void CreateNode()
	{
		SetPathParent();
		List<AirplaneNode> nodeList = new List<AirplaneNode>(nodes);
		var newNodeGo = new GameObject("Node " + (nodes.Length + 1).ToString());
		#if UNITY_EDITOR
		UnityEditor.Undo.RegisterCreatedObjectUndo(newNodeGo, "Create Airplane Node");
		#endif
		var newNodeTransform = newNodeGo.transform;
		newNodeTransform.parent = nodeParent.transform;
		
		//Places the node near the last one that was created
		if (lastCreatedNode != null)
		{
			//Apply an arbitrary offset to dodge divisions per zero
			newNodeTransform.localPosition = lastCreatedNode.localPosition + Vector3.forward;
			newNodeTransform.localRotation = lastCreatedNode.localRotation;
		}
		else
		{
			newNodeTransform.localPosition = Vector3.zero;
			newNodeTransform.localRotation = Quaternion.identity;
		}
		
		lastCreatedNode = newNodeTransform;
		var newNode = newNodeGo.AddComponent<AirplaneNode>();
		nodeList.Add(newNode);
		newNode.path = this;
		nodes = nodeList.ToArray ();
	}
	
	/// <summary>
	/// Called when a node is destroyed in Editor. Not tested at runtime.
	/// Same warning than CreateNode()
	/// </summary>
	/// <param name="destroyedNode">Destroyed node.</param>
	public void OnDestroyedNode(AirplaneNode destroyedNode)
	{
		List<AirplaneNode> nodeList = new List<AirplaneNode>();
		int index = 1;
		foreach(AirplaneNode node in nodes)
		{
			if (node != destroyedNode)
			{
				node.gameObject.name = "Node " + index.ToString ();
				index += 1;
				nodeList.Add(node);
			}
		}
		nodes = nodeList.ToArray ();
	}
	
	/// <summary>
	/// Gets all objects that could change when destroying a node. Used for Undo.RecordObjects().
	/// </summary>
	/// <returns>The all objects.</returns>
	public Object[] GetAllObjects()
	{
		List<Object> gos = new List<Object>();
		foreach (AirplaneNode node in nodes)
		{
			gos.Add (node.gameObject);
		}
		gos.Add (this);
		//		if (nodeParent != null) {gos.Add(nodeParent);}
		return gos.ToArray ();
	}
	
	/// <summary>
	/// Drawing path of airplane in editor.
	/// </summary>
	void OnDrawGizmos()
	{
		if(showGizmos && enabled && !((loopingType == PathLoopingType.Looping && nodes.Length < 3) || (loopingType == PathLoopingType.Once && nodes.Length < 2)))
		{
			SetControlPoints();
			int maxIndex = nodes.Length;
			
			if (loopingType == PathLoopingType.Once) {maxIndex = nodes.Length - 1;}
			
			for (int i = 0; i < maxIndex; i++)
			{
				int nextIndex = (i == nodes.Length - 1) ? 0 : i + 1;
				
				Gizmos.color = gizmoColors.pathColor;
				
				float increment = 1 / (float)gizmoSplinePrecision;
				
				Vector3 p0 = nodes[i].Position;
				Vector3 p1 = nodes[i][1];
				Vector3 p2 = nodes[nextIndex][0];
				Vector3 p3 = nodes[nextIndex].Position;
				
				Vector3 q0 = CalculateBezierPoint(0, p0, p1, p2, p3);
				Vector3 q1;
				float t=0;
				for (int j = 1; j <= gizmoSplinePrecision; j++)
				{
					t += increment;
					q1=CalculateBezierPoint(t, p0, p1, p2, p3);
					Gizmos.DrawLine(q0, q1);
					q0=q1;
				}
			}
		}
	}
	
	public bool IsLast(AirplaneNode node)
	{
		return node == nodes[nodes.Length - 1] && loopingType == PathLoopingType.Once;
	}
	
	public bool IsFirst(AirplaneNode node)
	{
		return node == nodes[0] && loopingType == PathLoopingType.Once;
	}
	
	public void FixNullErrors()
	{
		List<AirplaneNode> nodeList = new List<AirplaneNode>();
		int index = 1;
		int before = nodes.Length;
		foreach (AirplaneNode node in nodes) 
		{
			if (node != null) 
			{
				node.gameObject.name = "Node " + index.ToString ();
				index += 1;
				nodeList.Add (node);
			}
		}
		nodes = nodeList.ToArray ();
		int after = nodes.Length;
		if (before != after){Debug.Log ("Null References in Nodes array cleared", this);}
	}
	
	
	
	#if UNITY_EDITOR
	[UnityEditor.MenuItem("GameObject/Create Other/Airplane Path")]
	static void CreateNewPath()
	{
		GameObject go = new GameObject("Airplane Path", typeof(AirplanePath));
		go.transform.position = Vector3.zero;
		go.transform.rotation = Quaternion.identity;
		UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Airplane Path");
	}
	#endif
	
	[System.Serializable]
	public class GizmoColors
	{
		public Color pathColor = Color.red;
		public Color nodeTangentsColor = Color.green;
	}
	
	#endregion

	[System.Serializable]
	public class AdvancedSettings
	{
		public bool useLegacyPathAlgorithm = false;
	}
}


