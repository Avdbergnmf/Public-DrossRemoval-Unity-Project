using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Tie this to the virtual omni model pen to have it follow the motions of the actual omni.
Might take some fiddling with the scaling and the transform point it is connected to. 
*/

public class FollowOmniMotion : MonoBehaviour
{
    public float positionScale = 0.00128f;

    HapticPlugin haptic;

    public Transform additionalTransform = null;

    private void Start()
    {
        haptic = FindObjectOfType<HapticPlugin>();
    }

    private void Update()
    {
        Matrix4x4 matrixWorld =  haptic.stylusTransformRaw;
        Vector3 inkwellPos = new Vector3(0.0f, -65.5f, 88.1f);

        gameObject.transform.localPosition = positionScale * (Vec4To3(matrixWorld.GetColumn(3)) - inkwellPos);
        gameObject.transform.localRotation = QuaternionFromMatrix(matrixWorld);

        if (additionalTransform != null)
        {
            gameObject.transform.localPosition += additionalTransform.localPosition;
            gameObject.transform.localRotation *= additionalTransform.localRotation;
        }
    }


    #region HelperFunctions

    private static Quaternion QuaternionFromMatrix(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
    }

    Vector3 Vec4To3(Vector4 vec4)
    {
        Vector4 vec3 = new Vector3(vec4.x, vec4.y, vec4.z);
        return vec3;
    }
    #endregion
}
