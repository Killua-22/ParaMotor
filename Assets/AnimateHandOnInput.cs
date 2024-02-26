using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinch;
    public InputActionProperty grab;
    public Animator handAnimator;


    void Start()
    {
        
    }

    
    void Update()
    {
        float triggerValue = pinch.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        float gripValue = grab.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);

    }
}
