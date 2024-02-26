using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
public class Trigger : MonoBehaviour
{
    public PathCreation.Examples.PathFollower pf;

    public InputActionProperty trigger;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(trigger.action.IsPressed())
        {
            pf.enabled = true;
        }
    }
}
