using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script saves the entire pose of the robot at startup, and resets it to that pose using the doReset function
// So that the robot has the same starting pose every round.

public class TransformHolder
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public bool isKinematic;

    public TransformHolder(Transform t, bool kinState = false)
    {
        position = t.position;
        rotation = t.rotation;
        localScale = t.localScale;
        isKinematic = kinState;
        //Debug.Log(isKinematic);
    }
    public void matchTransform(Transform t)
    {
        t.position = position;
        t.rotation = rotation;
        t.localScale = localScale;
    }
}

public class ResetRB : MonoBehaviour
{
    public GameObject rootObject = null;
    Rigidbody[] RBs = null;
    Dictionary<Rigidbody, bool> kinState = new Dictionary<Rigidbody, bool>();
    Dictionary<Rigidbody, TransformHolder> startTrans = new Dictionary<Rigidbody, TransformHolder>();

    Joint[] joints;

    void Start()
    {
        RBs = rootObject.GetComponentsInChildren<Rigidbody>();
        Debug.Log("Found this many RBs: " + RBs.Length);
        //joints = rootObject.GetComponentsInChildren<Joint>();

        foreach (Rigidbody RB in RBs)
        {
            startTrans.Add(RB, new TransformHolder(RB.transform, RB.isKinematic)); // Saves the starting transforms
        }
    }

    public void doReset()
    {
        Debug.Log("Resetting RB...");

        foreach (var RB in RBs)
            RB.isKinematic = true;

        foreach (var RB in RBs)
            startTrans[RB].matchTransform(RB.transform);
    }

    public void resetKinState()
    {
        foreach (var RB in RBs)
            RB.isKinematic = startTrans[RB].isKinematic;
    }
}
