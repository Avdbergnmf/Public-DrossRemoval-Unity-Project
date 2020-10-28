using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class ColliderFollowHeadset : MonoBehaviour
{
    private CharacterController charController;
    public Transform centerEye;

    private void Start()
    {
        charController = GetComponent<CharacterController>();
    }

    private void LateUpdate()
    {
        float groundHeight = transform.position.y;

        Vector3 newCenter = transform.InverseTransformVector(centerEye.position - transform.position);
        charController.center = new Vector3(newCenter.x, charController.center.y, newCenter.z);

        charController.height = centerEye.position.y - groundHeight;
        charController.center = new Vector3(charController.center.x, charController.height / 2, charController.center.z);
    }
}
