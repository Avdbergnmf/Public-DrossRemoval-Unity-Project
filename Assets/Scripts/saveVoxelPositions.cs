using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class saveVoxelPositions : MonoBehaviour
{
    // Saving the voxel positions saves me from having to load in an additional particle container, which cuts down on CPU/GPU usage
    // This script object is saved into a preset, and later loaded in before startup. 
    // If new voxel positions need to be loaded up follow this steps:
    // - Add the Voxels prefab (in the flexassets folder) to the scoop
    // - Set the correct flex assets
    // - go into intersection logging manager, enable saving, disable useSaved
    // - On this scriptobject save the preset by clicking the topright icon in the window in the gameobject this is attached to
    [ReadOnly] public Vector3[] voxels;

    //Curently has an issue, but kept here for possible later use (if the flex solid actors start causing CPU spikes again...)
}
