
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Wrote this script to link/fix the movements of gameobjects.

Used for the guidance line to make sure it stays algined in the desired way.

There is probably much cleaner ways to achieve this, but this general purpose script is just a quick and easy way to accomplish these type of things.
*/

public class TransformModifier : MonoBehaviour
{
    // position link
    public bool linkPosition = false;
    [ConditionalField(nameof(linkPosition))] public Transform parentPosition = null;
    public bool positionOffset = false;
    [ConditionalField(nameof(positionOffset))] public Vector3 posOffset = Vector3.zero;

    // rotation link
    public bool linkRotation = false;
    [ConditionalField(nameof(linkRotation))] public Transform parentRotation = null;
    public bool rotationOffset = false; // <<<<<<<<<< not sure how this works
    [ConditionalField(nameof(rotationOffset))] public Quaternion rotOffset = Quaternion.identity;

    // scale link
    public bool linkScale = false;
    [ConditionalField(nameof(linkScale))] public Transform parentScale = null;
    public bool scaleOffset = false;
    [ConditionalField(nameof(scaleOffset))] public Vector3 sOffset = Vector3.zero;


    // Simple Fixposition
    [ConditionalField(nameof(linkPosition), true)] public bool simpleFixPosition = false;
    Vector3 startPosition;
    // Simple Fixrotation
    [ConditionalField(nameof(linkRotation), true)] public bool simpleFixRotation = false;
    Quaternion startRotation;




    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }
    void LateUpdate()
    {
        if (linkPosition)
        {
            transform.position = parentPosition.position;

            if (positionOffset)
                transform.position += posOffset;
        }
        else if (simpleFixPosition)
            transform.position = startPosition;

        if (linkRotation)
        {
            transform.rotation = parentRotation.rotation;

            if (rotationOffset)
                transform.rotation *= rotOffset;
        }
        else if (simpleFixRotation)
            transform.rotation = startRotation;

        if (linkScale)
        {
            transform.localScale = parentScale.localScale;

            if (scaleOffset)
                transform.localScale += sOffset;
        }
        else if (simpleFixRotation)
            transform.rotation = startRotation;

    }
}
