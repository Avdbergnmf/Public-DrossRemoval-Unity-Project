using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionLoggingManager : MonoBehaviour
{
    [ReadOnly] public string collidingBodyName = "";
    [ReadOnly] public Vector3 collisionForce = Vector3.zero;
    Vector3[] collisionPoints;
    Vector3[] collisionNormals;

    [SerializeField] bool visualizeCollisions = false;
    [ConditionalField(nameof(visualizeCollisions))] public float verticalSize = 0.5f;

    [ConditionalField(nameof(visualizeCollisions))] public GameObject projectorObject = null;

    [ConditionalField(nameof(visualizeCollisions))] public bool scaleWithJointForce = true;
    [ConditionalField(nameof(visualizeCollisions))] public float maxSize = 0.5f;
    [ConditionalField(nameof(visualizeCollisions))] public float minSize = 0.2f;

    [ConditionalField(nameof(scaleWithJointForce))] public float scaleFactor = 1f;
    [ConditionalField(nameof(scaleWithJointForce))] public GameObject grabberObj = null;

    [ConditionalField(nameof(scaleWithJointForce))] public bool smoothForces = false;
    [ConditionalField(nameof(smoothForces))] public int windowSize = 3;

    Vector3 projectorObjectStartPos;

    [Header("Violations")]
    // Violations
    [SerializeField] bool checkViolations = true;
    [ConditionalField(nameof(checkViolations))] public GameObject[] forbiddenCollisionGameObjects = null;
    [ConditionalField(nameof(checkViolations))] public float maxForceMagnitude = 10f;
    [ConditionalField(nameof(checkViolations))] [ReadOnly] public bool violatedForbidden = false;
    [ConditionalField(nameof(checkViolations))] [ReadOnly] public bool violatedForce = false;

    List<float> forceHistory; // Reference API: http://msdn.microsoft.com/en-us/library/6sh2ey19.aspx

    // Start is called before the first frame update
    void Start()
    {
        if (visualizeCollisions)
            projectorObjectStartPos = projectorObject.transform.position;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject collidingBody = collision.collider.gameObject;
        collidingBodyName = collidingBody.name;

        // Check for violation of forbidden colliders (Steel Strip)
        if (checkViolations)
            foreach (GameObject GO in forbiddenCollisionGameObjects)
                if (collidingBody == GO) violatedForbidden = true;

        collisionPoints = new Vector3[collision.contacts.Length];
        collisionNormals = new Vector3[collision.contacts.Length];
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            collisionPoints[i] = collision.contacts[i].point;
            collisionNormals[i] = collision.contacts[i].normal;
        }

        // Get the collisionforce
        Joint grabJoint = grabberObj.GetComponent<Joint>();
        float forceInCollDir = 0f;
        if (grabJoint)
        {
            collisionForce = grabJoint.currentForce;
            forceInCollDir = Vector3.Dot(collisionForce, collisionNormals[0]);

            // Check force violation
            if (checkViolations)
                if (forceInCollDir > maxForceMagnitude)
                    violatedForce = true;
        }

        // Only plot the first collision, but the rest could also be implemented in the same way...
        if (visualizeCollisions) 
        {
            Transform hitProjector = projectorObject.GetComponent<Transform>();
            hitProjector.position = collisionPoints[0];

            float finalScale = maxSize;

            if (scaleWithJointForce)
            {
                if (smoothForces)
                {
                    forceHistory = new List<float>();
                    forceHistory.Add(forceInCollDir);
                }

                finalScale *= scaleFactor * forceInCollDir;
                finalScale = checkRange(finalScale, 0.2f, maxSize);
            }

            Vector3 scale = new Vector3(finalScale, verticalSize, finalScale);
            hitProjector.position -= (verticalSize / 2.05f) * collisionNormals[0];
            hitProjector.localScale = scale;
            hitProjector.rotation = Quaternion.FromToRotation(Vector3.up, collisionNormals[0]);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        collisionPoints = new Vector3[collision.contacts.Length];

        collisionPoints = new Vector3[collision.contacts.Length];
        collisionNormals = new Vector3[collision.contacts.Length];
        for (int i = 0; i < collision.contacts.Length; i++)
        {
            collisionPoints[i] = collision.contacts[i].point;
            collisionNormals[i] = collision.contacts[i].normal;
        }

        // Get the collisionforce
        Joint grabJoint = grabberObj.GetComponent<Joint>();
        float forceInCollDir = 0f;
        if (grabJoint)
        {
            collisionForce = grabJoint.currentForce;
            forceInCollDir = Vector3.Dot(collisionForce, collisionNormals[0]);

            // Check force violation
            if (checkViolations)
                if (forceInCollDir > maxForceMagnitude)
                    violatedForce = true;
        }

        if (visualizeCollisions)
        {
            Transform hitProjector = projectorObject.GetComponent<Transform>();
            hitProjector.position = collisionPoints[0];

            float finalScale = maxSize;

            if (scaleWithJointForce)
            {
                if (grabJoint)
                {
                    if (smoothForces) // this is kinda laggy, needs a better filter (LP would prob work) or just not be used at all...
                    {
                        forceHistory.Add(forceInCollDir);
                        if (forceHistory.Count > windowSize)
                            forceHistory.Add(forceInCollDir);

                        else if (forceHistory.Count == windowSize)
                        {
                            forceHistory.RemoveAt(0); // remove first element
                            forceHistory.Add(forceInCollDir); // add new element
                        }

                        float avgForce = 0;

                        for (int i = 0; i < forceHistory.Count; i++)
                            avgForce += forceHistory[i];

                        avgForce /= forceHistory.Count;
                        forceInCollDir = avgForce;
                    }

                    finalScale *= scaleFactor * forceInCollDir;

                    finalScale = checkRange(finalScale, minSize, maxSize);
                }
                else
                    finalScale = minSize; // else just default to minsize, as the forces will reset anyway.

            }

            Vector3 scale = new Vector3(finalScale, verticalSize, finalScale);
            hitProjector.position -= (verticalSize / 2.05f) * collisionNormals[0];
            hitProjector.localScale = scale;
            hitProjector.rotation = Quaternion.FromToRotation(Vector3.up, -collisionNormals[0]);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        collidingBodyName = "";
        collisionForce = Vector3.zero;
        projectorObject.transform.position = projectorObjectStartPos;

        // ViolationPart
        if (checkViolations)
        {
            violatedForbidden = false;
            violatedForce = false;
        }
    }


    public float checkRange(float input, float min, float max)
    {
        float output;

        if (input < min)
            output = min;
        else if (input > max)
            output = max;
        else
            output = input;

        return output;
    }


    /*private void OnDrawGizmos()
    {
        if (collisionPoints != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < collisionPoints.Length; i++)
            {
                Gizmos.DrawSphere(collisionPoints[i], 0.2f);
            }
        }
    }*/
}
