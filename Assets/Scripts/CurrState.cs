using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simply holds the current state of the experiment so that it can be saved/loaded later
// Doesnt change any settings, but other scripts (sceneState.cs, trialOrderManager.cs) use this information 
public class CurrState : MonoBehaviour
{
    [Header("Visual states")]
    // visual states
    public bool FRVF;
    public bool WR;
    public bool GVF;
    public bool omni;
    public bool proj;
    public bool hit;

    [Header("Haptic states")]
    // haptic states
    public bool h_FRVF;
    public bool h_GVF;

    [Header("Display on")]
    // display state
    public bool VR;
    public bool Monitor;

}
