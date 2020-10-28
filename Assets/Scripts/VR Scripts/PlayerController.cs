using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class PlayerController : MonoBehaviour
    {
        public float speed = 1;
        public CharacterController characterController;
        public XRController controller;
        public bool moveIfPressed = true;

        public bool useGravity = true;
        public bool useTriggerToFly = false;

        Vector2 currentState;
        Vector3 direction;

        Camera directionCam;

        private void Start()
        {
            characterController = GetComponent<CharacterController>();
            directionCam = characterController.GetComponentInChildren<Camera>();

        }

        private void Update()
        {
            if (directionCam != null)
                direction = directionCam.transform.TransformDirection(new Vector3(currentState.x, 0, currentState.y)); //Player.instance.hmdTransform.TransformDirection(new Vector3(input.axis.x, 0, input.axis.y));
        }

        void FixedUpdate()
        {
            InputDevice device = controller.inputDevice;
            
            InputFeatureUsage<Vector2> feature = CommonUsages.primary2DAxis;

            InputFeatureUsage<bool> triggerButtonFeature = CommonUsages.triggerButton;
            
            InputFeatureUsage<bool> clicker = CommonUsages.primary2DAxisClick;
            bool clicked;


            bool move = true;
            if (moveIfPressed) // If the setting is enabled
                if (device.TryGetFeatureValue(clicker, out clicked)) // if the output can be read
                    if (!clicked) // if the button is not clicked
                        move = false;
            
            // Apply gravity
            if (useGravity && !useTriggerToFly)
                characterController.Move(new Vector3(0, -9.81f, 0) * Time.deltaTime);

            // Walking
            if (move)
            {
                if (device.TryGetFeatureValue(feature, out currentState))
                    if (currentState.magnitude > 0.1)
                        characterController.Move(speed * Time.deltaTime * Vector3.ProjectOnPlane(direction, Vector3.up));
            }
            else
                currentState = new Vector2(0f, 0f);

            // Trigger Flying
            bool triggerDown;
            if (device.TryGetFeatureValue(triggerButtonFeature, out triggerDown) && useTriggerToFly)
                if (triggerDown)
                    transform.position += speed* Time.deltaTime * controller.transform.forward;//characterController.Move(speed * Time.deltaTime * characterController.GetComponentInChildren<Camera>().transform.forward);

        }
    }

}
