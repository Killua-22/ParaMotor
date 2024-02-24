//-------------------------------------------------------------------------------
//------------------------Isabel Bartelmus 06.07.22------------------------------
//--Calculating the hand distance from the core to get the wingspan for gliding--
//----------------Calculating the Physcial stuff for gliding---------------------
//-------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

[RequireComponent (typeof(Rigidbody))] //adding a rgidbody to the obj the script is on
public class WingsCalculation : MonoBehaviour
{
    //---------Variables-------------

    [Header ("Game object only ther for debugging. Will set in runtime")]
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject Core;
    [SerializeField] private GameObject Head;
    [SerializeField] private GameObject Player;

    private Rigidbody rb;

    

    private float distance;
    private float dragForce = 0;
    private float max_wingspan = 3f;

    private float angleOfAttack = -30f;
    private float flightDirection;
    private float effectiveWingspan;
    private Vector3 rotationOfPlayer;

    private float startTime; 

    public float PlyerWeight;
    void Awake()
    {
        // Get Components in Awake
        RightHand = GameObject.FindGameObjectWithTag("RHand");
        LeftHand  = GameObject.FindGameObjectWithTag("LHand");
        Core      = GameObject.FindGameObjectWithTag("Core");
        Head      = GameObject.FindGameObjectWithTag("MainCamera");
        Player    = GameObject.FindGameObjectWithTag("Player");
        rb = Player.GetComponent<Rigidbody>();

        //rb.AddForce(50f*rb.mass, 0, 0); //Start force (stat speed)
        
        startTime = Time.time;

    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {   
        //Arm Distance
        distance = GetHandDistance();
        distance = ClampWingspan(distance);
        dragForce = GravityCalculation(distance, rb.velocity.y);

        effectiveWingspan = distance/max_wingspan;

        //head position maybe ?? //might make sick xD
        flightDirection = Head.transform.rotation.eulerAngles.y;
        directionOfFlight(flightDirection);
        
        //Hand Rotation
        angleOfAttack = GetHandRotation();

        ReduceSpeed(angleOfAttack, flightDirection,effectiveWingspan);
        IncreaseSpeed(angleOfAttack,dragForce, flightDirection);
        rb.AddForce(0, dragForce, 0);

        // Print Debug
        //Debug.Log("Velocity on X  " + rb.velocity.x + "\n"+  "Velocity on Z  "+rb.velocity.z);
        //Debug.Log("Vlight angel" + angleOfAttack);
        
        #region Debug
        //Debug Movement kalkulation
        // if((Time.time -startTime) > 3)
        // {
        //     angleOfAttack = 0f;
        //     rotationOfPlayer = Player.transform.eulerAngles;
        //     rotationOfPlayer.y = 45f;

        //     Player.transform.eulerAngles = rotationOfPlayer;
        // }
        // if((Time.time -startTime) > 5)
        // {
        //     angleOfAttack = 30f;
        // }
        // if((Time.time -startTime) > 7)
        // {
        //     angleOfAttack = 0f;
        // }
        #endregion


    }
    
    #region Hand Distance
    public float GetHandDistance()
    {
        float fullDist = 0f;

        float rightDistance = Vector3.Distance(RightHand.transform.position , Core.transform.position);
        float leftDistance = Vector3.Distance(LeftHand.transform.position, Core.transform.position);

        fullDist = rightDistance + leftDistance;

        #region Visualisation
        Debug.DrawLine(RightHand.transform.position , Core.transform.position,Color.magenta,rightDistance);
        Debug.DrawLine(LeftHand.transform.position, Core.transform.position, Color.blue ,leftDistance);
        Debug.DrawLine(Head.transform.position, Core.transform.position, Color.red , 2);
        #endregion

        return fullDist;
    }
    #endregion

    #region  Hand Rotation
    public float GetHandRotation()
    {
        float combinedRoation = 0f;

        float rightRotation = RightHand.transform.rotation.eulerAngles.x;
        //Debug.Log("Right                  "+rightRotation);
        float leftRotation = LeftHand.transform.rotation.eulerAngles.x;
        //SDebug.Log("LEFT    "+leftRotation);

        #region Clamp Left Hand
        //Clamp Rotation Left Hand
        if(leftRotation >180 && leftRotation < 360)
        {
            // minus 10 because 10 is your neutral position
            leftRotation = leftRotation - 360 -5; 
        }
        else 
        {
            leftRotation -= 5;
        }
        #endregion

        #region Clamp Right Hand
          //Clamp Rotation Right  Hand
        if(rightRotation >180 && rightRotation < 360)
        {
            // minus 10 because 10 is your neutral position
            rightRotation = rightRotation - 360 -5; 
        }
        else 
        {
            rightRotation -= 5;
        }
        #endregion

        float addedRotation = rightRotation + leftRotation;
        combinedRoation = addedRotation;

        return combinedRoation;
    }
    #endregion

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

    #region Clamp Wing
    public float ClampWingspan(float wingspan)
    {
        if(wingspan > max_wingspan)
        {
            wingspan = max_wingspan;
        }
        if(wingspan < 1.6f)
        {
            wingspan = 0.003f;
        }

        return wingspan/max_wingspan;
    }
    #endregion

    #region Reduce Speed
    public void ReduceSpeed(float climbingAngle, float viewingDirection, float effectiveWingspan)
    {   
        
        float tempVelocity = 0;
        float horizontalVelocity = Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2));
        float degreeToRadian = climbingAngle * Mathf.PI /180;
        float viewingDirectionRadian = viewingDirection * Mathf.PI /180;

        Vector3 tempVector = rb.velocity;
        float Force = 0;
        if (horizontalVelocity > 0 && climbingAngle >= 0)
        {   
            tempVelocity = horizontalVelocity - 9.81f * 0.02f * Mathf.Sin(degreeToRadian);
            Force = 40f * Mathf.Sin(degreeToRadian)* rb.mass;
            Force = Force * effectiveWingspan; 
            rb.AddForce(0,Force,0);
           
            if (tempVelocity < 0) 
            {
                tempVector.x = 0;
                tempVector.z = 0;
                rb.velocity = tempVector;
            }
            else 
            {
                tempVector.x = tempVelocity * Mathf.Sin(viewingDirectionRadian);
                tempVector.z = tempVelocity * Mathf.Cos(viewingDirectionRadian);
                rb.velocity = tempVector;
            }

        }
    }
    #endregion

    #region Increas Speed
    public void IncreaseSpeed(float climbingAngle, float dragForce, float viewingDirection)
    {   
        if(climbingAngle < 0)
        {
            float degreeToRadian = climbingAngle * Mathf.PI /180;
            float directionRadian = viewingDirection * Mathf.PI /180;
            rb.AddForce(-Mathf.Sin(degreeToRadian)*dragForce * Mathf.Sin(directionRadian), Mathf.Sin(degreeToRadian)*dragForce, -Mathf.Sin(degreeToRadian)*dragForce * Mathf.Cos(directionRadian));
        }
        
    }
    #endregion

    #region direction Of Flight
    public void directionOfFlight(float viewingDirection)
    {
        Vector3 tempVector = rb.velocity;
        float horizontalVelocity = Mathf.Sqrt(Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2));
        float degreeToRadian = viewingDirection * Mathf.PI /180;
        tempVector.x = horizontalVelocity * Mathf.Sin(degreeToRadian);
        tempVector.z = horizontalVelocity * Mathf.Cos(degreeToRadian);
        rb.velocity = tempVector;
    }
    #endregion


}
