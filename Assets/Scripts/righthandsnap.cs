using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;


public class righthandsnap : MonoBehaviour
{
    private GameObject RightHand;
    [SerializeField] public Transform snapPosition; // Position to snap the hand to
    private bool isSnapped = false;

    InputDevice rightController;


    private void Awake()
    {
        RightHand = GameObject.FindGameObjectWithTag("RHand");
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }
    void Update()
    {
        if (rightController.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
        {
            if (gripValue > 0.1f && !isSnapped) // Adjust grip value threshold as needed
            {
                SnapHandToPosition();
            }
            else if (gripValue < 0.1f && isSnapped)
            {
                isSnapped = false;
            }
        }
    }

    void SnapHandToPosition()
    {
        transform.position = snapPosition.position;
        transform.rotation = snapPosition.rotation;
        isSnapped = true;
    }
}
