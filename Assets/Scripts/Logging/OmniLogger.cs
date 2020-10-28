using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OmniLogger : MonoBehaviour
{
    public HapticPlugin hapticDevice;

    LogToCSV logger;

    [Header("File Name")]
    [SerializeField] public string reportDirectoryName = "Report";
    [SerializeField] public bool includePlayerName = false;
    [SerializeField] public bool includeDeviceName = true;
    [SerializeField] public bool includeDate = true;
    [SerializeField] public string custom;

    [ReadOnly] public string reportFileName = "";
    [SerializeField] public bool allowOverwrite = false;

    [Header("Log Settings")]
    //[DisplayOnlyAttribute] public string collidingBody = "";
    //[DisplayOnlyAttribute] public Vector3 collisionForce = Vector3.zero;
    [SerializeField] public int writeEveryNLogs = 500;

    private string[] reportHeaders = new string[24] {
        "ConfigName",
        "Stiffness",
        "Damping",
        "Stylus Force X",
        "Stylus Force Y",
        "Stylus Force Z",
        "Stylus Force Magnitude",
        "anchorPosition3 X",
        "anchorPosition3 Y",
        "anchorPosition3 Z",
        "anchorVelocity3 X",
        "anchorVelocity3 Y",
        "anchorVelocity3 Z",
        "stylusPositionRaw X",
        "stylusPositionRaw Y",
        "stylusPositionRaw Z",
        "stylusVelocityRaw X",
        "stylusVelocityRaw Y",
        "stylusVelocityRaw Z",
        "stylusVelocityRaw Magnitude",
        "stylusRotationWorld X",
        "stylusRotationWorld Y",
        "stylusRotationWorld Z",
        "stylusRotationWorld W",
    };

    void Start()
    {
        logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs);

        string reportFileName = CreateName();

        logger.CreateFileName(reportFileName);
    }

    void Update()
    {
        if (!logger.madeFile) return;

        if (hapticDevice.OmniSpring.anchorPosition3 == null) return; // Only start counting after the omni has been used (connected) at least once

        Vector3 anchorPos = makeVector3(hapticDevice.OmniSpring.anchorPosition3); // this returns an error if the hapticDevice hasnt been used yet.
        Vector3 deltaPos = anchorPos - hapticDevice.OmniSpring.stylusPositionRaw;
        Vector3 force = (float)hapticDevice.OmniSpring.stiffness * deltaPos - (float)hapticDevice.OmniSpring.damping * (hapticDevice.stylusVelocityRaw - makeVector3(hapticDevice.OmniSpring.anchorVelocity3)); // apparently damping isn't used by default, left the code here anyway
        // force = k*x - b*u
        
        string[] strings = new string[24] {
                hapticDevice.OmniSpring.configName,
                hapticDevice.OmniSpring.stiffness.ToString(),
                hapticDevice.OmniSpring.damping.ToString(),
                force.x.ToString(),
                force.y.ToString(),
                force.z.ToString(),
                force.magnitude.ToString(),
                hapticDevice.OmniSpring.anchorPosition3[0].ToString(),
                hapticDevice.OmniSpring.anchorPosition3[1].ToString(),
                hapticDevice.OmniSpring.anchorPosition3[2].ToString(),
                hapticDevice.OmniSpring.anchorVelocity3[0].ToString(),
                hapticDevice.OmniSpring.anchorVelocity3[1].ToString(),
                hapticDevice.OmniSpring.anchorVelocity3[2].ToString(),
                hapticDevice.OmniSpring.stylusPositionRaw.x.ToString(),
                hapticDevice.OmniSpring.stylusPositionRaw.y.ToString(),
                hapticDevice.OmniSpring.stylusPositionRaw.z.ToString(),
                hapticDevice.OmniSpring.stylusVelocityRaw.x.ToString(),
                hapticDevice.OmniSpring.stylusVelocityRaw.y.ToString(),
                hapticDevice.OmniSpring.stylusVelocityRaw.z.ToString(),
                hapticDevice.OmniSpring.stylusVelocityRaw.magnitude.ToString(),
                hapticDevice.OmniSpring.stylusRotationWorld.x.ToString(),
                hapticDevice.OmniSpring.stylusRotationWorld.y.ToString(),
                hapticDevice.OmniSpring.stylusRotationWorld.z.ToString(),
                hapticDevice.OmniSpring.stylusRotationWorld.w.ToString(),
        };

        logger.write(strings);

    }

    Vector3 makeVector3(double[] oldVec)
    {
        Vector3 newVector;
        newVector.x = (float)oldVec[0];
        newVector.y = (float)oldVec[1];
        newVector.z = (float)oldVec[2];
        return newVector;
    }



    public string CreateName()
    {
        if (custom.Length > 0)
        {
            reportFileName = custom;
        }
        else if (includePlayerName || includeDeviceName || includeDate)
        {
            reportFileName = "omni_";
            if (includeDeviceName)
                reportFileName += hapticDevice.configName.Replace(" ", "") + "_";
            if (includePlayerName)
            {
                StartSurveyScript startSurveyScript = FindObjectOfType<StartSurveyScript>();

                if (startSurveyScript)
                    if (startSurveyScript.playerInfo.name.Length > 0)
                        reportFileName += startSurveyScript.playerInfo.name + "_";
                    else
                        Debug.LogError("Player name empty!");
            }
            if (includeDate)
                reportFileName += System.DateTime.Today.ToShortDateString().Replace("/", "_");

        }
        else
        {
            Debug.LogError("Log name empty, cannot log data");
            return null;
        }
        if (File.Exists(logger.GetFilePath(reportFileName)))
        {
            if (allowOverwrite)
            {
                Debug.Log("File already exists, but overwriting");
                File.Delete(logger.GetFilePath(reportFileName));
            }
            else
            {
                Debug.LogError("File already exists, not overwriting");
                reportFileName = "";
                return null;
            }
        }
        return reportFileName;
    }


}
