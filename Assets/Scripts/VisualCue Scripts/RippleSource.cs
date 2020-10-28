using UnityEngine;

public class RippleSource : MonoBehaviour
{
    /// <summary>
    /// Place this script on the collider of the fixture, which which the SpawnRipple (operator controlled object) will collide.
    /// This script will spawn a ripple object. From there the rest of the behavior of that ripple object is managed within the spawned object by the AnimateSphereMask script.
    /// (The reason for this is so that the amount of Find/GetComponent callbacks can be severely limited).
    /// </summary>
    public GameObject ripplesVFX;

    public enum RippleType {GuidanceVirtualFixture, ForbiddenRegionVirtualFixture, Workrange, ZincBath};

    public RippleType rippleType = RippleType.ForbiddenRegionVirtualFixture;

    #region MainMethods

    public void OnTriggerEnter(Collider co)
    {
        var ripple = co.gameObject.GetComponent<SpawnRipple>();

        if (ripple != null) // Is it a ripple object?
        {
            var triggers = ripple.ReturnElements();

            if (triggers[(int)rippleType] == 1) // Should this type of ripple be triggered
            {
                Debug.Log("Detected ripple collision, instantiating ripple object");

                // Find the name for the object
                string rippleName = ripple.rippleVFXName;
                if (string.IsNullOrEmpty(rippleName))
                    rippleName = co.name;

                // Spawn the ripple
                var rippleGO = spawnRipple(ripple, rippleName);
            }
        }
    }

    #endregion

    #region HelperMethods

    private GameObject spawnRipple(SpawnRipple ripple, string name)
    {
        GameObject rippleGO = GameObject.Find("_rippleGO" + name);

        if (!rippleGO) // if it does not exist and is perpetual OR if it is not perpetual, spawn it //(!rippleGO && ripple.perpetual || !ripple.perpetual)
        {
            // Instantiate GO
            rippleGO = Instantiate(ripplesVFX, transform.position, transform.rotation) as GameObject;
            rippleGO.tag = "RippleInstance";
            rippleGO.name = "_rippleGO_" + name + "_" + gameObject.name;
            rippleGO.layer = LayerMask.NameToLayer("VisualCue");

            initAnimate(rippleGO, ripple);
        }

        return rippleGO;
    }

    private void initAnimate(GameObject rippleGO, SpawnRipple ripple) // Set sphere animation variables
    {
        var animation = rippleGO.GetComponent<AnimateSphereMask>();

        if (animation != null) // If it does exist (prevent errors)
        {
            // Which GO is the source?
            animation.sourceGO = gameObject;
            // Set the linked GameObject (the object that created it)
            animation.linkedGO = ripple.gameObject;
            // Tell it which type of ripple it is
            animation.type = rippleType;

            // Set start properties
            animation.startRadius = ripple.radius;
            animation.innerStartRadius = ripple.innerRadius;
            animation.startAlpha = ripple.alpha;

            /*// Set animation properties  // Removed the animation as it was unnecessary for now
            animation.alphaFadeSpeed = ripple.alphaFadeSpeed;
            animation.radiusGrowSpeed = ripple.radiusGrowSpeed;
            animation.innerRadiusGrowspeed = ripple.innerRadiusGrowspeed;

            // Disable/Enable animation
            if (ripple.animate)
                animation.startAnimation = true;
            else
                animation.startAnimation = false;
                
            // perpetual?
            if (ripple.perpetual)
                animation.perpetual = true;
             */

            // Set the color
            if (ripple.useCustomColor)
                animation.color = ripple.rippleColor; // use the set color
            else
                animation.color = rippleGO.GetComponent<MeshRenderer>().material.color; // use the default color set in the rippleVFX object

            animation.init();
        }
    }
    #endregion
}
