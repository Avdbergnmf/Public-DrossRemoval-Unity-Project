using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Add this to the collider you want to disable collisions for, increase the ignored objects size and add the gameobject who's colliders you want to disable collisions with.
// Enable include children to disable collisions for all colliders in all child objects of the selected gameobjects.

public class DisableCollisions : MonoBehaviour
{
    [SerializeField] private GameObject[] ignoredObjects = null;

    [SerializeField] private bool includeChildren = false;


    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i< ignoredObjects.Length; i++)
        {
            Collider[] ignoredColliders = ignoredObjects[i].GetComponents<Collider>();
            Collider[] theseColliders = GetComponents<Collider>();

            for (int j = 0; j < ignoredColliders.Length; j++)
            {
                for (int l = 0; l < theseColliders.Length; l++)
                    Physics.IgnoreCollision(theseColliders[l], ignoredColliders[j]);
            }

            if (includeChildren)
            {
                Collider[] ignoredChildColliders = ignoredObjects[i].GetComponentsInChildren<Collider>();

                for (int k = 0; k < ignoredChildColliders.Length; k++)
                {
                    for (int l = 0; l < theseColliders.Length; l++)
                        Physics.IgnoreCollision(theseColliders[l], ignoredChildColliders[k]);
                }
            }
        }
    }

}
