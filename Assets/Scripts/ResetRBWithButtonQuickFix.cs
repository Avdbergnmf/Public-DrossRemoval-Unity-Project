using System.Collections.Generic;
using UnityEngine;

// When resetting the scene, sometimes one of the joints of the virtual omni model would spazz out.
// The reason this happened is because it has joint limits and got pushed past them somehow.
// This quick and dirty fix consists of pressing the F4 button to quickly fix these jointlimits.

public class ResetRBWithButtonQuickFix : MonoBehaviour
{
    public HingeJoint fuckeyJoint = null;
    private bool defLimitsOn = true;

    JointLimits deflims;
    JointLimits altlims;

    private void Start()
    {
        deflims = fuckeyJoint.limits;

        altlims = deflims;
        altlims.min = 0f;
        altlims.max = 125f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            if (defLimitsOn)
            {
                fuckeyJoint.limits = altlims;
                defLimitsOn = false;
            }
            else
            {
                fuckeyJoint.limits = deflims;
                defLimitsOn = true;
            }
        }
    }
}
