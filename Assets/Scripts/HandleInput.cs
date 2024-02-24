using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleInput : MonoBehaviour
{
    public Transform pivot;
    public GameObject Handle;

    public float Handleoutput = 0f;
    public float ydistance;

    private void Start()
    {
        ydistance = pivot.position.y - Handle.transform.position.y;
    }

    public void Update()
    {

        float new_yDistance = pivot.position.y - Handle.transform.position.y;

        float yvalue = new_yDistance - ydistance;

        yvalue *= 22.72f; //initial distance should be 0 but it is 0.044 for some reason after 1 frame. So to tackle that issue, I multiplied it by 22.72 to get ~1 output. The final output ranges from 1 to ~7.

        //recommeneded to use yvalue from 1 to 5 for input of direction.

        //Debug.Log(yvalue);

        
    }
}
