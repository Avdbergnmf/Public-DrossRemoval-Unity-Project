using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Attach this script to any collider to add Virtual Fixture Haptic Guidance to it.

There is a lot of settings so be sure to go through them and get a grasp for all of them.
*/

public class HapticGuidance : MonoBehaviour
{
    public enum EFFECT_TYPE {CONSTANT};

    public enum CONTACT_MODEL { constant, linearSpring, huntCrossley, zincBath}; // Maybe's: Kelvin–Voigt [Coefficient of Restitution Interpreted as Damping in Vibroimpact] Maxwell models [A Nonlinear ViscoElastoplastic Impact Model and the Coefficient of Restitution,Viscoplastic Analysis for Direct Impact of Sports Balls]

    public bool disableGuidance = false;
    public bool persistent = false; // Makes haptic effects persist after being created, useful as for some reason, every time a haptic effect is enabled, a little force "shock" is given, which is hella annoying, would be nicer to have all effects get created on start but that brings some issues unless going more complex, and not really worth the effort here.

    // General Settings
    //public bool applyForceToRigidBody = false; // This was pretty much useless 
    public float triggerDistance = 0.5f;
    public float gradientDistance = 0.1f;
    public Transform customTransform = null;
    //[Range(1.0f, 1000.0f)]

    // Model properties
    public CONTACT_MODEL contactModel = CONTACT_MODEL.huntCrossley;
    public float magnitude = 1f; // for constant force
    public float stiffness = 1f;
    public float restDistance = 0f;
    // hunt-crossley specific
    public bool insideOut = false;
    public float n = 1.2f;
    public float alpha = 0.16f;
    // Zincbath specific stuff
    public FindIntersectingVolume findIntersectingVolume = null; // This was an addon for the zincbath haptics specifically 
    public float forceScale = 1f;

    // damper settings
    public bool dampRB = false;
    public bool projRBDamping = false;
    public float rbDampingConst = 5f;
    public bool dampHD = false;
    public bool projHDDamping = false;
    public float hdDampingConst = 0.5f;

    public Vector3 force = Vector3.zero; // The force vector
    Vector3 Direction = Vector3.up; // the direction of the force (non-normalized)

    // Keep track of the Haptic Devices
    HapticPlugin[] devices;
    bool[] inTheZone;       //Is the stylus in the effect zone?
    bool[] oldInTheZone;
    Vector3[] devicePoint;  // Current location of stylus
    float[] delta;          // Distance from stylus to zone collider.
    int[] FXID;             // ID of the effect.  (Per device.)
    bool[] effectRunning;

    // These are the user adjustable vectors, converted to world-space. 
    private Vector3 focusPointWorld = Vector3.zero;
    private Vector3 directionWorld = Vector3.up;

    EFFECT_TYPE effectType = EFFECT_TYPE.CONSTANT;

    private void OnEnable()
    {
        if (devices != null) StopAll(); // make sure no effects are running when enabling again...
    }

    // Called upon starting the scene:
    //    It will identify the Haptic devices, initialize variables internal to this script, 
    //    and request an Effect ID from Open Haptics. (One for each device.)
    void Start()
    {
        //Initialize the list of haptic devices.
        devices = (HapticPlugin[])Object.FindObjectsOfType(typeof(HapticPlugin));
        inTheZone = new bool[devices.Length];
        oldInTheZone = new bool[devices.Length];

        devicePoint = new Vector3[devices.Length];
        delta = new float[devices.Length];
        FXID = new int[devices.Length];

        effectRunning = new bool[devices.Length];


        // Generate an OpenHaptics constantForce effect for each of the devices.
        for (int ii = 0; ii < devices.Length; ii++)
        {
            inTheZone[ii] = false;
            oldInTheZone[ii] = false;
            devicePoint[ii] = Vector3.zero;
            delta[ii] = 0.0f;
            FXID[ii] = HapticPlugin.effects_assignEffect(devices[ii].configName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Find the pointer to the collider that defines the "zone". 
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider == null)
        {
            Debug.LogError("This Haptic Effect Zone requires a collider");
            return;
        }

        // Update the effect seperately for each haptic device.
        for (int ii = 0; ii < devices.Length; ii++)
        {
            HapticPlugin device = devices[ii];
            bool olderInTheZone = oldInTheZone[ii];
            oldInTheZone[ii] = inTheZone[ii];
            int ID = FXID[ii];

            // If a haptic effect has not been assigned through Open Haptics, assign one now.
            if (ID == -1)
            {
                FXID[ii] = HapticPlugin.effects_assignEffect(devices[ii].configName);
                ID = FXID[ii];

                if (ID == -1) // Still broken?
                {
                    Debug.LogError("Unable to assign Haptic effect.");
                    continue;
                }
            }

            // Determine if the stylus is in the "zone". 
            Vector3 manipPos;
            if (customTransform)
            {
                manipPos = customTransform.position;
            }
            else
            {
                manipPos = device.hapticManipulator.transform.position; //World Coordinates, device.stylusPositionWorld
            }

            Vector3 CP = collider.ClosestPoint(manipPos);  //World Coordinates

            devicePoint[ii] = CP * triggerDistance;
            Direction = (CP - manipPos);

            delta[ii] = Direction.magnitude;

            focusPointWorld = CP; // transform.TransformPoint(CP);
            directionWorld = Vector3.Normalize(Direction); //// Normalize, else the force scales here... transform.TransformDirection(Direction);

            //If the stylus is within the Zone, The ClosestPoint and the Stylus point will be identical.
            if (delta[ii] <= triggerDistance + gradientDistance && device.hapticManipulator.GetComponent<Joint>() != null) // Check if manip is within the zone, and if a joint exists, meaning a the manipulator is controlling some object
            {
                if (delta[ii] == 0 && contactModel != CONTACT_MODEL.zincBath) // <<<<<<<<<<<<
                    return;

                inTheZone[ii] = true;

                // Convert from the World coordinates to coordinates relative to the haptic device.
                Vector3 focalPointDevLocal = device.transform.InverseTransformPoint(focusPointWorld); // Closest point on collider
                Vector3 rotationDevLocal = device.transform.InverseTransformDirection(directionWorld); // The direction vector
                Vector3 xDevLocal = device.transform.InverseTransformDirection(Direction); //The distance vector from to the closest point on the collider

                // Velocity of rigid body
                Rigidbody body = device.hapticManipulator.GetComponent<Rigidbody>();
                Vector3 bodyVel = body.velocity;
                if (projRBDamping)
                    bodyVel = Vector3.Project(body.velocity, directionWorld);
                Vector3 rbDamping = Vector3.zero;

                // Velocity of stylus
                Vector3 stylusVel = device.hapticManipulator.GetComponent<Rigidbody>().velocity;
                if (projHDDamping)
                    stylusVel = Vector3.Project(stylusVel, directionWorld);
                Vector3 hdDamping = Vector3.zero;

                // Calculate the force vector
                switch (contactModel)
                {
                    case CONTACT_MODEL.constant:
                        force = - magnitude * rotationDevLocal;
                        rbDamping = -rbDampingConst * bodyVel; // simple linear damper
                        hdDamping = -hdDampingConst * stylusVel;
                        break;
                    case CONTACT_MODEL.linearSpring:
                        force = stiffness * (xDevLocal - restDistance * rotationDevLocal); // Subtracted the restlength of the spring (F=k*(x-L))
                        rbDamping = -rbDampingConst * bodyVel; // simple linear damper
                        hdDamping = -hdDampingConst * stylusVel;
                        break;
                    case CONTACT_MODEL.huntCrossley: // This acts weirdddd, in the end decided to just go with the linear spring
                        Vector3 xn;
                        int flipped;
                        if (!insideOut)
                        {
                            //Vector3 border = directionWorld * (triggerDistance + gradientDistance);
                            //Vector3 x = border - Direction;
                            //Vector3 xLocal = device.transform.InverseTransformDirection(x);
                            xn = powVec3(xDevLocal, n, true);
                            flipped = -1;
                        }
                        else
                        {
                            xn = xDevLocal;// powVec3(xDevLocal, n, true);
                            flipped = 1;
                        }

                        force = flipped * stiffness * xn; // spring force

                        Vector3 relVel = Vector3.Project(body.GetRelativePointVelocity(focusPointWorld), directionWorld); // velocity relative to the closestpoint, projected on the direction of the force, if this is not done it causes strange behavior since the point moves along with the tool, but its velocity isnt registered
                        rbDamping = flipped * rbDampingConst * stiffness * 1.5f * alpha * Vector3.Scale(xn,-relVel); // hunt-crossley damping force
                        hdDamping = flipped * hdDampingConst * stiffness * 1.5f * alpha * Vector3.Scale(xn, -stylusVel);
                        break;
                    case CONTACT_MODEL.zincBath:
                        force = findIntersectingVolume.fractionSubmerged * forceScale * new Vector3(0,1,0); // just a force up
                        rbDamping = rbDampingConst * new Vector3(0, 1, 0) * findIntersectingVolume.flux;// * bodyVel * (force.magnitude + 0.1f); // simple linear damper
                        hdDamping = hdDampingConst * new Vector3(0, 1, 0) * findIntersectingVolume.flux;// * stylusVel * (force.magnitude + 0.1f);
                        break;
                }
                // GradientDistance
                if (delta[ii] > triggerDistance)
                    force *= 1 - (delta[ii] - triggerDistance) / gradientDistance;

                // Apply damping
                if (dampRB)// Apply the damping on the rigidbody based on RB velocity <-- this is preferred! The connection between the haptic grabber and the tool isn't very stiff and so only damping the omni force will still lead to instabilities
                    body.AddForce(rbDamping);
                if (dampHD) // Damp HD based on its stylusvelocity
                    force += hdDamping; // Damp the forces HD based on the stylus velocity <----- stylus velocity is not correct yet!!

                if (disableGuidance) // make the force zero if this guidance is set to disabled
                    force = Vector3.zero;

                // Assign the calculated forces
                Vector3 forceDir = Vector3.Normalize(force);
                double forceMagnitude = force.magnitude;
                double[] dir = { forceDir.x, forceDir.y, forceDir.z };

                // Send the current effect settings to OpenHaptics.
                double Gain = 1; // not used
                double[] pos = { 0, 0, 0 }; // pos of the spring --> doesn't matter, not using it
                double Frequency = 0; // not used

                HapticPlugin.effects_settings(
                    device.configName,
                    ID,
                    Gain,
                    forceMagnitude,
                    Frequency,
                    pos,
                    dir);
                HapticPlugin.effects_type(
                    device.configName,
                    ID,
                    (int)effectType);

            }
            else
            {
                inTheZone[ii] = false;
                force = Vector3.zero;
                // Note : If the device is not in the "Zone", there is no need to update the effect settings.
            }


            // If the on/off state has changed since last frame, send a Start or Stop event to OpenHaptics
            if (!oldInTheZone[ii] && inTheZone[ii]) // added an extra inthezone bool to skip 1 loop & filter out some spazzy button signals -> removed again cause it didnt get fixed. The problem is in the hapticplugin script somewhere... it (almost) always takes two attempts to connect it... check out the code, but it works well enough so mehhhh
            {
                if (persistent && effectRunning[ii]) return;

                Debug.Log("Starting Effect for " + device.configName + ", on GO: " + gameObject.name);
                HapticPlugin.effects_startEffect(device.configName, ID);

                effectRunning[ii] = true;
            }
            else if (oldInTheZone[ii] && !inTheZone[ii])
            {
                if (persistent)
                {
                    // make effect magnitude 0
                    double[] pos = { 0, 0, 0 }; // pos of the spring --> doesn't matter, not using it
                    Vector3 forceDir = Vector3.Normalize(force);
                    double[] dir = { forceDir.x, forceDir.y, forceDir.z };
                    HapticPlugin.effects_settings(
                        device.configName,
                        ID,
                        0,
                        0,
                        0,
                        pos,
                        dir);
                    HapticPlugin.effects_type(
                        device.configName,
                        ID,
                        (int)effectType);
                }
                else stopEffect(device.configName, ID, ii);
            }
        }
    }
    void OnDestroy() // This is important as otherwise, the haptic forces will remain
    {
        StopAll();
    }

    void OnApplicationQuit()
    {
        //For every haptic device, send a Stop event to OpenHaptics
        for (int ii = 0; ii < devices.Length; ii++)
        {
            HapticPlugin device = devices[ii];
            if (device == null)
                continue;

            int ID = FXID[ii];

            stopEffect(device.configName, ID, ii);

            inTheZone[ii] = false;
        }
    }

    private void OnDisable()
    {
        StopAll();
    }

    void stopEffect(string configName, int ID, int ii)
    {
        if (effectRunning[ii])
        {
            HapticPlugin.effects_stopEffect(configName, ID);
            effectRunning[ii] = false;
        }
    }

    public void StopAll()
    {
        //For every haptic device, send a Stop event to OpenHaptics
        for (int ii = 0; ii < devices.Length; ii++)
        {
            HapticPlugin device = devices[ii];
            if (device == null)
                continue;
            int ID = FXID[ii];

            stopEffect(device.configName, ID, ii);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.matrix = this.transform.localToWorldMatrix;

        Gizmos.color = Color.white;

        Ray R = new Ray();
        R.direction = Direction;

        if (effectType == EFFECT_TYPE.CONSTANT)
        {
            Gizmos.DrawRay(R);
            Gizmos.DrawIcon(focusPointWorld, "anchor_icon.tiff");
        }

        if (devices == null)
            return;

        // If the device is in the zone, draw a red marker. 
        // And draw a line indicating the spring force, if we're in that mode.
        for (int ii = 0; ii < devices.Length; ii++)
        {
            if (delta[ii] <= Mathf.Epsilon)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(devicePoint[ii], 1.0f);

                Gizmos.DrawLine(focusPointWorld, devicePoint[ii]);
            }
        }

    }

    Vector3 powVec3(Vector3 vec, float pow, bool keepSign = false)
    {
        Vector3 result;

        result.x = Mathf.Pow(Mathf.Abs(vec.x), pow);
        result.y = Mathf.Pow(Mathf.Abs(vec.y), pow);
        result.z = Mathf.Pow(Mathf.Abs(vec.z), pow);

        if (keepSign)
        {
            if (vec.x < 0)
                result.x = -result.x;
            if (vec.y < 0)
                result.y = -result.y;
            if (vec.z < 0)
                result.z = -result.z;
        }

        return result;
    }
}

