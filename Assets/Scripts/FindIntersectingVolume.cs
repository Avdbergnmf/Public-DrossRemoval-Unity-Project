using UnityEngine;
using System.Collections;

public class FindIntersectingVolume : MonoBehaviour // this would more accurately be called find submerged volume, as it looks at the volume below a certain y-value only (which works in the case of a bath)
{
    /*
    This script uses an array of relative positions to a gameobject as voxels for that gameobject.

    It then tests how many of these voxels are underneath a certain y level, for every frame.

    This value is later used by the haptic guidance in the bath to scale the magnitude of the guidance force.
    */
    
    #region Private
    public bool useSavedVoxels = false;
    [ConditionalField(nameof(useSavedVoxels), true)] public bool saveVoxels = true;
    [ConditionalField(nameof(useSavedVoxels))] public saveVoxelPositions voxels = null;
    [SerializeField] public GameObject skimmerObject = null;
    [Tooltip("This NEEDS to be a cube object with a box collider.")][SerializeField] GameObject zincObject = null;
    [SerializeField] bool drawVoxels = false;

    [ReadOnly] public bool colliding;
    [ReadOnly] public float fractionSubmerged = 0f;
    private float prevFraction = 0f;
    [ReadOnly] public float flux = 0f;


    Vector3[] m_particles = null; // This contains all the positions of the particles relative to the position of the skimmerObject

    int Nparticles = 0;
    
    Quaternion startRot;

    Vector3[] currPos;

    Collider skimColl;
    Collider bathColl;
    [ReadOnly] public float surfaceY;

    #endregion

    #region Properties
    public Vector3[] particles
    {
        get { return m_particles; }
        set { m_particles = value; }
    }

    #endregion

    #region Messages
    private void Start()
    {
        skimColl = skimmerObject.GetComponent<Collider>(); // Collider of the skimmer object, this is used to detect initial intersection (not for intersecting volume)
        bathColl = zincObject.GetComponent<Collider>(); // Best if this is a BoxCollider but could be any collider.

        surfaceY = zincObject.transform.position.y + zincObject.transform.localScale.y / 2; // The height of the bath
    }

    void Update()
    {
        if(m_particles == null) 
        {
            if (useSavedVoxels)
                Nparticles = voxels.voxels.Length;
            else
            {
                NVIDIA.Flex.FlexSolidActor skim_actor = skimmerObject.GetComponent<NVIDIA.Flex.FlexSolidActor>();

                if (skim_actor == null) // check for its children
                    if (skimmerObject.GetComponentsInChildren<NVIDIA.Flex.FlexSolidActor>() == null)
                        Debug.LogError("No FlexSolidActors found!");
                    else
                        m_particles = makeChildrenIntoSingleArray();
                else
                {
                    m_particles = TransformArr(ToVector3(skim_actor.particles), skimmerObject.transform.localToWorldMatrix.inverse);
                    //(ToVector3(skim_actor.particles), -skimmerObject.transform.position); // Get the position relative to the skimmer position
                    skim_actor.enabled = false; // Turn off the flex object so that it doesn't interfere with anything
                    Nparticles = m_particles.Length;
                }
            }
        }

        if (bathColl.bounds.Intersects(skimColl.bounds)) // Check if they are colliding at all to prevent unneccessary calculations 
        {
            colliding = true; // Set a bool to display in Editor

            if (useSavedVoxels)
                currPos = TransformArr(voxels.voxels, skimmerObject.transform.localToWorldMatrix); // Get the actual position of each voxel
            else
                currPos = TransformArr(m_particles, skimmerObject.transform.localToWorldMatrix); // Get the actual position of each voxel
            
            int detected = scanBathForSkimmer(currPos, surfaceY); // Find out how many voxels are in the bath

            fractionSubmerged = (float)detected / (float)Nparticles; // The fraction of the object that is submerged.
        }
        else fractionSubmerged = 0f;

        flux = (fractionSubmerged - prevFraction) / Time.deltaTime; // change of fraction per second
        prevFraction = fractionSubmerged; // previous fraction

        if (saveVoxels && !useSavedVoxels)
            voxels.voxels = m_particles;
    }

    #endregion

    #region Methods
    Vector3[] makeChildrenIntoSingleArray()
    {
        NVIDIA.Flex.FlexSolidActor[] skim_actor_arr = skimmerObject.GetComponentsInChildren<NVIDIA.Flex.FlexSolidActor>();

        Debug.Log("Number of FlexSolidActors found in children for voxel positions: " + skim_actor_arr.Length);

        Vector3[][] m_particles_arr;
        m_particles_arr = new Vector3[skim_actor_arr.Length][]; // make array of arrays

        // Transform all the arrays
        for (int i = 0; i < skim_actor_arr.Length; i++)
        {
            skim_actor_arr[i].enabled = true;

            m_particles_arr[i] = TransformArr(ToVector3(skim_actor_arr[i].particles), skimmerObject.transform.localToWorldMatrix.inverse);
            //AddPos(ToVector3(skim_actor_arr[i].particles), -skimmerObject.transform.position); // Get the position relative to the skimmer position

            Nparticles += skim_actor_arr[i].particles.Length; // Add to the total number of particles
            skim_actor_arr[i].enabled = false; // Turn off the flex object so that it doesn't interfere with anything
        }

        // put all in 1 array
        Vector3[] particles_all = new Vector3[Nparticles]; 
        int currNo = 0;

        for (int i = 0; i < skim_actor_arr.Length; i++)
        {
            System.Array.Copy(m_particles_arr[i], 0, particles_all, currNo, m_particles_arr[i].Length);
            currNo += m_particles_arr[i].Length;
        }

        return particles_all;
    }


    int scanBathForSkimmer(Vector3[] pos, float ylevel) 
    {
        // since we already checked if the colliders are intersecting, and the bath can only be acessed from above
        // we only need to check the yposition for every voxel and see if it is below the surface or not.
        int detected = 0;

        for (int i = 0; i < pos.Length; i++)
        {
            if (pos[i].y < ylevel) detected++;
        }

        return detected;
    }


    Vector3[] TransformArr(Vector3[] parent, Matrix4x4 matrix)
    {
        Vector3[] transformed = new Vector3[parent.Length];

        for (int i=0; i< parent.Length; i++)
        {
            transformed[i] = matrix.MultiplyPoint3x4(parent[i]);
        }
        return transformed;
    }

    Vector3[] ToVector3(Vector4[] parent) 
    {
        // For some reason the particle positions come in Vector4 in which the w is always 1, remove this component here.
        Vector3[] Vec3Arr = new Vector3[parent.Length];

        for (int i = 0; i < parent.Length; i++)
        {
            Vec3Arr[i] = new Vector3(parent[i].x, parent[i].y, parent[i].z);
        }
        return Vec3Arr;
    }

    Vector3 ToVector3(Vector4 parent) 
    {
        // left this in as an overload just in case it comes in handy
        return new Vector3(parent.x, parent.y, parent.z);
    }

    Vector3[] AddPos(Vector3[] pos, Vector3 addPos) // overload without rotation
    {
        // For each element in the array of positions: Add a position vector3 to it
        // (Use this function to find the relative position by making addPos negative)

        Vector3[] newPos = new Vector3[pos.Length];

        for (int i = 0; i < pos.Length; i++)
        {
            newPos[i] = pos[i] + addPos;
        }
        return newPos;
    }

    #endregion

    private void OnDrawGizmos()
    {
        if (drawVoxels)
        {
            Gizmos.color = Color.green;

            if (currPos != null)
            {
                for (int i = 0; i < currPos.Length; i++)
                {
                    Gizmos.DrawSphere(currPos[i], 0.01f);
                }
            }
        }
    }
}
