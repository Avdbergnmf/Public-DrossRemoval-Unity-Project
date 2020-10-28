using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script handles the visual cue that occurs when getting close to the virtual fixtures.
// This is achieved by setting the material properties of the ForceField VF material.

[RequireComponent(typeof(Collider))]
public class AnimateSphereMask : MonoBehaviour
{
    [Tooltip("The border circle activates at circleActivationScaler*currentRadius.")] public float circleActivationScaler = 1f;
    [Tooltip("The blinking effect activates at blinkActivationScaler*currentRadius.")] public float blinkActivationScaler = 0.1f;
    [ReadOnly] public float currDistance = 0;

    [HideInInspector] public float startAlpha = 1f;

    [HideInInspector] public float startRadius = -.9f;

    [HideInInspector] public float innerStartRadius = -0.2f;

    [ReadOnly] public float currAlpha = 1f;
    [ReadOnly] public float currRadius;
    [ReadOnly] public float currInnerRadius;

    [Tooltip("This color is set through either the spawnripple script or by the default color set in the rippleVFX object")][ReadOnly]
    [ColorUsage(true, true)] public Color color;

    //[ReadOnly] public bool perpetual = false;

    [ReadOnly] public GameObject linkedGO; // The gameobject causing the ripple object to instantiate (the controlled one)
    [HideInInspector] public RippleSource.RippleType type;

    // (global) Private vars
    SpawnRipple ripple;
    Material mat;
    Collider thisCo;

    [ReadOnly] public GameObject sourceGO = null;

    private void OnTriggerStay(Collider co)
    {
        if (co.gameObject == linkedGO)
        {
            Vector3 spherePosition;
            Vector3 closestPoint;

            if (type == RippleSource.RippleType.ZincBath)
            {
                var spawner = linkedGO.GetComponent<SpawnRipple>();

                Vector3 XZpos = spawner.useXZLocationOf.position;

                closestPoint = new Vector3(XZpos.x, spawner.findIntersectingVolume.surfaceY, XZpos.z);

                Vector3 dir = Vector3.up; // normal direction is always up! that saves me some headache (if I thought of that 1 hr ago :')

                Vector3 currDistanceVec = dir * (1 - spawner.findIntersectingVolume.fractionSubmerged) * currRadius;
                currDistance = currDistanceVec.magnitude;

                spherePosition = closestPoint + currDistanceVec;
            }
            else
            {
                Vector3 collPosition = co.transform.position;

                closestPoint = GetClosestPoint(collPosition);

                Vector3 dir = collPosition - closestPoint;
                currDistance = dir.magnitude;

                spherePosition = co.transform.position;
            }

            // Set the Sphere mask position
            mat.SetVector("_SphereCenter", spherePosition);

            if (currDistance <= currRadius*circleActivationScaler)
            {
                mat.SetFloat("_BoundaryBrightness", 1 - currDistance / (currRadius * circleActivationScaler));
                mat.SetInt("_EnableCircle", 1);
                mat.SetVector("_BoundaryCenter", closestPoint);

                if (currDistance <= currRadius * blinkActivationScaler)
                    mat.SetInt("_EnableBlink", 1);
                else
                    mat.SetInt("_EnableBlink", 0);
            }
            else
            {
                mat.SetInt("_EnableCircle", 0);
            }
            
        }
    }

    private void OnTriggerExit(Collider co) // <<< Maybe better to just keep the gameobjects, depending on how many triggers & ripple sources exist.
    {
        var ripple = co.gameObject.GetComponent<SpawnRipple>();

        if (co.gameObject == linkedGO)
        {
            Destroy(gameObject); // SelfDestruct
        }
    }

    #region Helper Methods
    public void init()
    {
        if (!mat)
            SetPrivates();

        currAlpha = startAlpha;
        currRadius = startRadius;
        currInnerRadius = innerStartRadius;

        set();
    }

    private void SetPrivates() // So that all these calls have to be done only once.
    {
        ripple = linkedGO.GetComponent<SpawnRipple>();
        mat = GetComponent<MeshRenderer>().material;
        thisCo = GetComponent<Collider>();
    }

    private void set() // Set the current properties
    {
        mat.SetFloat("_AlphaMult", currAlpha);
        mat.SetFloat("_SphereRadius", currRadius);
        mat.SetFloat("_InnerSphereRadius", currInnerRadius);

        mat.SetColor("_Color", color);
    }

    public Vector3 GetClosestPoint(Vector3 target)
    {
        Vector3 CP = Vector3.zero;

        if (type == RippleSource.RippleType.Workrange) // If we are dealing with a workrange (only works correcly if this workrange is completely spherical)
        {
            SphereCollider sphere = gameObject.GetComponent<SphereCollider>();
            if (sphere)
            {
                CP = target - transform.position;
                CP.Normalize();
                CP *= sphere.radius;
                CP = Vector3.Scale(CP, transform.lossyScale);
                //Debug.Log(transform.lossyScale);
                CP += transform.position;
            }
            else
            {
                Debug.Log("WARNING: Workrange is not sphere and so the closest point on the outside boundary can not be found. Visual cue will be limited to circle only.");
            }

        }

        if (type == RippleSource.RippleType.ForbiddenRegionVirtualFixture || type == RippleSource.RippleType.ZincBath)
        {
            CP = thisCo.ClosestPoint(target);
        }

        return CP;
    }

    #endregion
}