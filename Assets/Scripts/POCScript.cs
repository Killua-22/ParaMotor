using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))] //adding a rgidbody to the obj the script is on
public class POCScript : MonoBehaviour
{
    //---------Variables-------------

    [Header("Game object only ther for debugging. Will set in runtime")]
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject Core;
    [SerializeField] private GameObject Head;
    [SerializeField] private GameObject Player;
    
    

    private Rigidbody rb;

    //[SerializeField] private float liftForceMagnitude = 5f;

    private float distance;
    private float dragForce = 0;
    private float max_wingspan = 3f;

    //private float angleOfAttack = -30f;
    private float flightDirection;
    private float effectiveWingspan;
    private Vector3 rotationOfPlayer;

    private float startTime;

    public float liftForce = 1000f;  // Adjust as needed
    //public float liftDuration = 10f;  // Adjust as needed
    private bool isLifting = false;


    private float forwardThrust = 10f;
    public float upwardThrustThreshold = 80f;
    private bool isFlying = false;
    private float glideSpeed = 0.4f;

    private float releaseLerpSpeed = 5f;

    InputDevice leftController;

    public float PlyerWeight;

    public HandleInput leftHandle;
    public HandleInput rightHandle;
    public float minRotation = -45f;  // Minimum rotation angle to the left
    public float maxRotation = 45f;   // Maximum rotation angle to the right
    void Awake()
    {
        // Get Components in Awake
        RightHand = GameObject.FindGameObjectWithTag("RHand");
        LeftHand = GameObject.FindGameObjectWithTag("LHand");
        Core = GameObject.FindGameObjectWithTag("Core");
        Head = GameObject.FindGameObjectWithTag("MainCamera");
        Player = GameObject.FindGameObjectWithTag("Player");
        rb = Player.GetComponent<Rigidbody>();
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        //rb.AddForce(50f*rb.mass, 0, 0); //Start force (stat speed)

        leftHandle = leftHandle.GetComponent<HandleInput>();
        rightHandle = rightHandle.GetComponent<HandleInput>();

        startTime = Time.time;


    }

    void Update()
    {

        //float leftRotationZ = Mathf.Lerp(0, minRotation, (leftHandle.yvalue - 1) / 6f);
        //float rightRotationZ = Mathf.Lerp(0, maxRotation, (rightHandle.yvalue - 1) / 6f);

        // Combine left and right rotations to tilt the glider
        //float combinedRotationZ = rightRotationZ - leftRotationZ;

        // Apply the combined rotation to the glider along the z-axis
        //gliderTransform.rotation = Quaternion.Euler(0f, 0f, combinedRotationZ);
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonPressed))
        {
            Debug.Log("Started");
            if (primaryButtonPressed)
            {
                
                StartParamotor();
            }
            
        }

        if(isFlying)
        {

            if (rb.velocity.magnitude < upwardThrustThreshold)
            {
                ApplyForwardThrust();
            }
            else
            {
                ApplyUpwardThrust();
            }
        }

    }

    void StartParamotor()
    {
        isFlying = true;
    }

    void FixedUpdate()
    {
        float pullDirection;
        //Arm Distance
        distance = GetHandDistance(out pullDirection);

        //CheckForInput();


        //flightDirection = Head.transform.rotation.eulerAngles.y;

        SimulateDownwardGlide();

    }

    void ApplyLiftForce()
    {
        isLifting = true;
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
    }

    /*void CheckForInput()
    {
        // Get the left controller's input device
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Check if the trigger button is pressed
        if (leftController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            // Adjust forward thrust based on the trigger value
            //forwardThrust = triggerValue;
            //isFlying = true;
        }

    }*/

    void ApplyForwardThrust()
    {
        // Apply forward thrust by adding force in the forward direction
        rb.AddForce(transform.forward * forwardThrust * Time.deltaTime, ForceMode.Force);
    }

    void ApplyUpwardThrust()
    {
        // Apply upward thrust logic here (e.g., change movement direction)
        rb.AddForce(Vector3.up * forwardThrust * Time.deltaTime, ForceMode.Force);
    }



    void SimulateDownwardGlide()
    {
        // Simulate slow downward glide
        Vector3 downwardForce = Vector3.down * glideSpeed * 0.001f; // Adjust glide speed as needed
        rb.AddForce(downwardForce, ForceMode.Force);
    }

    #region Hand Distance
    public float GetHandDistance(out float pullDirection)
    {
        float fullDist = 0f;

        float rightDistance = Vector3.Distance(RightHand.transform.position, Core.transform.position);
        float leftDistance = Vector3.Distance(LeftHand.transform.position, Core.transform.position);

        fullDist = rightDistance + leftDistance;

        float threshold = 0.1f;

        if (Mathf.Abs(rightDistance - leftDistance) < threshold)
        {
            pullDirection = 0f;
        }
        else
        {
            pullDirection = (rightDistance > leftDistance) ? 1f : -1f;
        }

        #region Visualisation
        Debug.DrawLine(RightHand.transform.position, Core.transform.position, Color.magenta, rightDistance);
        Debug.DrawLine(LeftHand.transform.position, Core.transform.position, Color.blue, leftDistance);
        Debug.DrawLine(Head.transform.position, Core.transform.position, Color.red, 2);
        #endregion

        return fullDist;
    }
    #endregion



}
