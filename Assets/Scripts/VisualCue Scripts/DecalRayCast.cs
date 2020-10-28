using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Do a raycast and place the decal at the first location it hits. 

// Used for the circle on the floor, underneath the tip of the scoop

public class DecalRayCast : MonoBehaviour
{
    public Transform castFrom;
    public LayerMask m_RayCastLayerMask = -1; // Set the layermask for the portal camera
    public Vector3 globalCastDirection = Vector3.down;

    [HideInInspector] public RaycastHit hit;

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(castFrom.position, globalCastDirection, out hit, 10, m_RayCastLayerMask.value))
        {
            transform.position = hit.point;
            //Debug.Log(hit.collider.name);
        }
        else
        {
            //Debug.Log("Couldn't find a location to project decal, placing at 0,0,0");
            transform.position = Vector3.zero;
        }

        transform.up = -globalCastDirection;
    }
}
