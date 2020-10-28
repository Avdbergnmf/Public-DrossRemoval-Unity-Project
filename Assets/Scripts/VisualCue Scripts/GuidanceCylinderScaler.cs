using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Scales the cylinder that shows the current (positional) height of the scoop
// Is based on a raycast that finds out the distance.

public class GuidanceCylinderScaler : MonoBehaviour
{
    Vector3 startScale;
    public float thickness = 0.01f;

    public DecalRayCast floorDecalRayCast = null; // if this is set, it uses the raycast of the floor decal to set the groundheight, else it will default to 0

    float groundY = 0f;

    public Transform parentLocation = null;

    void Update()
    {
        if (floorDecalRayCast != null)
        {
            groundY = floorDecalRayCast.hit.point.y;
        }

        float halfWay = (parentLocation.position.y - groundY) / 2;

        gameObject.transform.position = parentLocation.position - Vector3.up * halfWay; // place it halfway through between the scoop and the ground (ground is at y=0)

        gameObject.transform.localScale = new Vector3(thickness, halfWay, thickness);
    }
}
