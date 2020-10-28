using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


namespace UnityEngine.XR.Interaction.Toolkit
{
    [RequireComponent(typeof(XRController))]
    public class showTrackBall : MonoBehaviour
    {
        public GameObject trackerBall = null;

        XRController controller;
        // Start is called before the first frame update
        void Start()
        {
            controller = gameObject.GetComponent<XRController>();
        }

        // Update is called once per frame
        void Update()
        {
            InputDevice device = controller.inputDevice;
            InputFeatureUsage<Vector2> feature = CommonUsages.primary2DAxis;
            InputFeatureUsage<bool> touch = CommonUsages.primary2DAxisTouch;

            Vector2 currentState;
            bool touched;

            if (device.TryGetFeatureValue(touch, out touched))
                if (touched)
                {
                    trackerBall.SetActive(true);
                    if (device.TryGetFeatureValue(feature, out currentState))
                    {
                        trackerBall.transform.localPosition = new Vector3(currentState.x, 0, currentState.y);
                    }
                }
                else
                    trackerBall.SetActive(false);

        }
    }

}
