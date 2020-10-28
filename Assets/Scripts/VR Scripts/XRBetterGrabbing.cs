using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRGrabInteractable))]
/* How to use: 
Add this script to the XR Grab interactable object.
Then add the makeNewTransform function to the "On Select Enter" interactable event.



    */
public class XRBetterGrabbing : MonoBehaviour
{
    public float grabRange = 0.5f;
    public Transform fromTransform = null; // From where should the grabrange be calculated? If this is not set, the objects own transform is used

    XRGrabInteractable grab = null;

    XRBaseInteractor interactor;

    void Start()
    {
        grab = GetComponent<XRGrabInteractable>();

        if (fromTransform == null)
            fromTransform = transform;
    }

    public void makeNewTransform()
    {
        if (grab.hoveringInteractors.Count > 0) // get the last interactor hovering the object
            interactor = grab.hoveringInteractors[grab.hoveringInteractors.Count-1];

        //Debug.Log((interactor.transform.position - fromTransform.position).magnitude);
        if ((interactor.transform.position - fromTransform.position).magnitude < grabRange)
        {
            grab.trackPosition = true;
            grab.trackRotation = true;
            grab.throwOnDetach = true;
            grab.attachTransform = interactor.transform;
        }
        else
        {
            grab.trackPosition = false;
            grab.trackRotation = false;
            grab.throwOnDetach = false;
            //grab.onSelectEnter.Invoke; // zo iets om nicer te maken
        }
    }
}
