using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Spawns an object that contains the visual cue for the virtual fixture that pops up when getting close to the collider.

public class SpawnRipple : MonoBehaviour
{
    [Header("Basic Settings")]
    [Tooltip("When enabled the Ripple color is overwritten with the color set here.")] public bool useCustomColor = false;
    [ColorUsageAttribute(true, true)] public Color rippleColor = new Vector4(1.988f, 0.438f, 0.438f, 1.0f); 

    [EnumFlagsAttribute]
    public RippleSource.RippleType triggerRippleTypes;

    public string rippleVFXName;

    [Header("Sphere mask (start) settings")]
    public float radius = 1f;
    public float innerRadius = 0f;
    public float alpha = 1f;

    [Header("ZincBath Specific")]
    [Tooltip("Only set this for a ZincBath rippleType")] public FindIntersectingVolume findIntersectingVolume = null;
    [Tooltip("Only set this for a ZincBath rippleType")] public Transform useXZLocationOf = null;


    public List<int> ReturnSelectedElements() // use this function to get the selected elements in the enum list
    {

        List<int> selectedElements = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(RippleSource.RippleType)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)triggerRippleTypes & layer) != 0)
            {
                selectedElements.Add(i);
            }
        }

        return selectedElements;
    }

    public List<int> ReturnElements() // use this function to get all elements where when they are enabled, a 1 is returned, else a 0 is returned.
    {

        List<int> selectedElements = new List<int>();
        for (int i = 0; i < System.Enum.GetValues(typeof(RippleSource.RippleType)).Length; i++)
        {
            int layer = 1 << i;
            if (((int)triggerRippleTypes & layer) != 0)
            {
                selectedElements.Add(1);
            }
            else
            {
                selectedElements.Add(0);
            }
        }

        return selectedElements;
    }

}
