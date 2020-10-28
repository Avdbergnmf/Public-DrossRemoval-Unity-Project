using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DampingSetter : MonoBehaviour
{
    // Add this to a gameobject to:
    // - set damping values for all hinge joints in the child components
    // - set drag values for all rigibodies in the child components
    // Purpose of this is to have make the tweaking of the values a bit easier.

    [Range(0.0f, 10.0f)] public float hingeJointsDamping = 0.0f;
    [Range(0.0f, 10.0f)] public float rigidBodyDrag = 0.0f;
    HingeJoint[] hJoints;
    Rigidbody[] RBs;
    List<Rigidbody> RBsList;
    public Rigidbody[] exceptionRigidBodies;

    float prevDamp = -10;
    float prevDrag = -10;

    private void Awake()
    {
        hJoints = GetComponentsInChildren<HingeJoint>();
        RBs = GetComponentsInChildren<Rigidbody>();
        RBsList = RBs.ToList();

        foreach (var RB in exceptionRigidBodies)
            RBsList.Remove(RB);

        if (hingeJointsDamping != prevDamp)
            foreach (var hj in hJoints)
            {
                var spring = hj.spring;
                hj.useSpring = true;
                spring.damper = hingeJointsDamping;
                hj.spring = spring;
            }

        if (rigidBodyDrag != prevDrag)
            foreach (var rb in RBsList)
                rb.drag = rigidBodyDrag;
    }
}
