using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class OmniPosCalibration : MonoBehaviour
{
    public XRController leftController;
    public XRController rightController;
    public GameObject CalibrationControllerModels;
    Canvas canvasL;
    Canvas canvasR;
    Canvas canvasOmni;

    bool calibrationActive;

    void Start()
    {
        canvasL = leftController.GetComponentInChildren<Canvas>();
        canvasL.enabled = false;
        canvasR = rightController.GetComponentInChildren<Canvas>();
        canvasR.enabled = false;
        CalibrationControllerModels.SetActive(false);

        canvasOmni = GetComponentInChildren<Canvas>();
        canvasOmni.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (calibrationActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, rightController.transform.rotation.eulerAngles.y + 90, gameObject.transform.rotation.eulerAngles.z);

                gameObject.transform.position = rightController.transform.position;

                Vector3 localOffset = new Vector3(0.141f, 0.136f, 0.004f);

                gameObject.transform.localPosition -= localOffset;

                /*float avgAngle = (leftController.transform.rotation.eulerAngles.y + rightController.transform.rotation.eulerAngles.y) / 2;
                gameObject.transform.rotation = Quaternion.Euler(gameObject.transform.rotation.eulerAngles.x, avgAngle - 90f - 2.532f, gameObject.transform.rotation.eulerAngles.z);
                gameObject.transform.position = (leftController.transform.position + rightController.transform.position) / 2;
                
                Vector3 additionalOffset = new Vector3(0.0155605f, -0.0636968f, 0.053346f);
                gameObject.transform.localPosition += additionalOffset; // accidentally took these offsets in local frame, but whatever, it works.*/
            }
            else if (Input.GetKeyDown(KeyCode.F12))
            {
                canvasL.enabled = false;
                canvasR.enabled = false;
                CalibrationControllerModels.SetActive(false);
                canvasOmni.enabled = false;
                calibrationActive = false;
            }
        }

        if (!calibrationActive && Input.GetKeyDown(KeyCode.F11))
        {
            
            canvasL.enabled = true;
            canvasR.enabled = true;
            CalibrationControllerModels.SetActive(true);
            canvasOmni.enabled = true;
            calibrationActive = true;
        }
    }
}
