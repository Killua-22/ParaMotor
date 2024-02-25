using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Rigidbody))] //adding a rgidbody to the obj the script is on
public class paragliderScript : MonoBehaviour
{
    //---------Variables-------------

    [Header("Game object only ther for debugging. Will set in runtime")]
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject LeftHand;
    
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


    private float forwardThrust = 0f;
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
        
        //Head = GameObject.FindGameObjectWithTag("MainCamera");
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
        float leftRotationZ = Mathf.Lerp(0, minRotation, (leftHandle.yvalue - 1) / 6f);
        float rightRotationZ = Mathf.Lerp(0, maxRotation, (rightHandle.yvalue - 1) / 6f);

        //Combine left and right rotations to tilt the glider
        float combinedRotationZ = rightRotationZ - leftRotationZ;

        // Apply the combined rotation to the glider along the z-axis
        Player.transform.rotation = Quaternion.Euler(0f, 0f, combinedRotationZ);


        if (leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool primaryButtonPressed))
        {
            if (primaryButtonPressed)
            {

                ApplyLiftForce();
            }
            else if (isLifting)
            {
                isLifting = false;
            }
        }
    }

    void FixedUpdate()
    {
        float pullDirection;
        //Arm Distance
        //distance = GetHandDistance(out pullDirection);
        //distance = ClampWingspan(distance);
        //dragForce = GravityCalculation(distance, rb.velocity.y);

        //effectiveWingspan = distance / max_wingspan;

        //head position maybe ?? //might make sick xD

        // Check for trigger input
        CheckForInput();

        
        // Apply forward thrust
        ApplyForwardThrust();

        //RotateGlider(pullDirection);

        // Simulate slow downward glide
        SimulateDownwardGlide();

    }

    void ApplyLiftForce()
    {
        isLifting = true;
        rb.AddForce(Vector3.up * liftForce, ForceMode.Force);
    }

    void CheckForInput()
    {
        // Get the left controller's input device
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        // Check if the trigger button is pressed
        if (leftController.TryGetFeatureValue(CommonUsages.trigger, out float triggerValue))
        {
            // Adjust forward thrust based on the trigger value
            //Debug.Log(triggerValue);
            forwardThrust = triggerValue;
        }
        else
        {
            // If trigger is not pressed, set forward thrust to zero
            forwardThrust = Mathf.Lerp(forwardThrust, 0f, Time.deltaTime * releaseLerpSpeed);
        }

    }

    void ApplyForwardThrust()
    {
        //float angleInRadians = viewDirection * Mathf.Deg2Rad;

        // Calculate the normalized direction vector based on the angle
        //Vector3 forwardVector = new Vector3(Mathf.Sin(angleInRadians), 0f, Mathf.Cos(angleInRadians));

        // Apply constant forward thrust
        Vector3 forwardForce = Player.transform.forward * forwardThrust * 500f;
        rb.AddForce(forwardForce, ForceMode.Force);

    }



    void SimulateDownwardGlide()
    {
        // Simulate slow downward glide
        Vector3 downwardForce = Vector3.down * glideSpeed * 0.001f; // Adjust glide speed as needed
        rb.AddForce(downwardForce, ForceMode.Force);
    }

    #region Hand Distance
    /* public float GetHandDistance(out float pullDirection)
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
    }*/
    #endregion

    void RotateGlider(float pullDirection)
    {
        // You can adjust the rotation speed based on the pull direction
        float rotationSpeed = 5f;

        // Calculate the target rotation based on the pull direction
        Quaternion targetRotation = Quaternion.Euler(0f, pullDirection * 45f, 0f);

        // Smoothly rotate the glider towards the target rotation
        Player.transform.rotation = Quaternion.Slerp(Player.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /*
    #region Gravity
    public float GravityCalculation(float wingspan, float falling_velocity)
    {
        float area_coefficient = 40f;
        float density_Air = 1.225f;
        float drag_coefficient = 2;         // depends on the object causing the drag. The factor 2 is similar to a rectangle

        // formula to calculate drag which is dependent on the velocity
        // this allows to simulate a terminal falling velocity
        float resultingForce = 0.5f * area_coefficient * wingspan * density_Air * drag_coefficient * (falling_velocity * falling_velocity);

        if (falling_velocity > 0)
        {
            resultingForce = -resultingForce;
        }
        return resultingForce;
    }
    #endregion
    */
}

