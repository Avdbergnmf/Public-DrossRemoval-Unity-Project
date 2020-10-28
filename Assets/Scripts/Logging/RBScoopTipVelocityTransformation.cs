using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBScoopTipVelocityTransformation : MonoBehaviour
{
    Rigidbody scoop = null;
    Transform LocalRelativeTransformOffset = null;

    private void Start()
    {
        string[] csvData = null;

        foreach (var line in csvData)
        {
            Vector3 offset = LocalRelativeTransformOffset.position - scoop.transform.position;
            Vector3 LinVelOffsetPoint = GetLinearVelocityAtPoint(scoop, offset);

            // Log this
        }
    }


    public Vector3 GetLinearVelocityAtPoint(Rigidbody rb, Vector3 aLocalPoint)
    {
        var p = aLocalPoint - rb.centerOfMass;
        var v = Vector3.Cross(rb.angularVelocity, p); // 
        v = rb.transform.TransformDirection(v);
        v += rb.velocity;
        return v;
    }
}