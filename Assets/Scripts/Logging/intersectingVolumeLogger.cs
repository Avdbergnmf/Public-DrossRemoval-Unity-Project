using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class intersectingVolumeLogger : MonoBehaviour
{
    // Simple script to handle the logging for the FindIntersectionVolume script.
    public FindIntersectingVolume FIV = null;

    [Header("File Name")]
    [ReadOnly] public string reportFileName = "";
    [SerializeField] public string reportDirectoryName = "Report";
    [SerializeField] public bool includeObjectName = true;
    [SerializeField] public bool includePlayerName = true;
    [SerializeField] public bool includeDate = true;
    [SerializeField] public string custom;

    [SerializeField] public bool allowOverwrite = false;

    [Header("Log settings")]
    [SerializeField] public int writeEveryNLogs = 250;

    // private
    private string[] reportHeaders = new string[2]
    {
        "Intersecting Volume",
        "Volume Flux"
    };
    LogToCSV logger;

    // Start is called before the first frame update
    void Start()
    {
        logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs);

        string reportFileName = CreateName();

        logger.CreateFileName(reportFileName);
    }

    // Update is called once per frame
    void Update()
    {
        if (!FIV) return;

        string[] strings = new string[2] 
        {
            FIV.fractionSubmerged.ToString(),
            FIV.flux.ToString()
        };

        logger.write(strings);
    }

    public string CreateName() // modified to take some arguments for easy implementation into this list based set-up
    {
        if (custom.Length > 0)
        {
            reportFileName = custom;
        }
        else if (includePlayerName || includeDate || includeObjectName)
        {
            reportFileName = "ivolume_";
            if (includeObjectName)
                reportFileName += FIV.skimmerObject.name + "_";
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
