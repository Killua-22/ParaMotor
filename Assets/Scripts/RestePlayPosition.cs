using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RestePlayPosition : MonoBehaviour
{
    public InputActionReference ResetReference = null;
    public Transform PlayerSpawn;
    public Transform Player;
    
    private void Awake()
    {
        ResetReference.action.started += PlayerRest;
    }
    private void OnDestroy()
    {
        ResetReference.action.started -= PlayerRest;
    }


    private void PlayerRest(InputAction.CallbackContext context)
    {
        Player.transform.position = PlayerSpawn.transform.position;
    }
}
