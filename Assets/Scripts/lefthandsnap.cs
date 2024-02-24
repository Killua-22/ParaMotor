using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class lefthandsnap : MonoBehaviour
{
    private GameObject LeftHand;
    [SerializeField] public Transform snapPosition; // Position to snap the hand to
    private bool isSnapped = false;

    InputDevice leftController;


    private void Awake()
    {
        LeftHand = GameObject.FindGameObjectWithTag("LHand");
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }
    void Update()
    {
        if (leftController.TryGetFeatureValue(CommonUsages.grip, out float gripValue))
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
