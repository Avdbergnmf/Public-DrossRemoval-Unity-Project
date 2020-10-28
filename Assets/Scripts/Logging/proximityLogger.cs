using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class proximityLogger : MonoBehaviour
{
    // publicasdf
    [Tooltip("The colliders for which the proximity is evaluated.")] public List<Collider> colliderCheckList = null;
    [Tooltip("The GameObjects for which the children will be checked for colliders for each of which the proximity is evaluated.")] public List<GameObject> checkChildrenColliders = null;
    
    [Tooltip("The transforms locations from which the proximity is evaluated. Make sure the GO these transforms are attached to have descriptive names.")] public Transform[] checkTransformPoints = null;

    //[HideInInspector] public List<string> transformNames; // ???????

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
    private string[] reportHeaders = new string[6]
    {
        "Transform Name",
        "Closest collider",
        "Distance X",
        "Distance Y",
        "Distance Z",
        "Magnitude"
    };
    LogToCSV[] loggers;

    void Start()
    {
        loggers = new LogToCSV[checkTransformPoints.Length];

        for (int i = 0; i < checkTransformPoints.Length; i++)
        {
            LogToCSV logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs);

            string reportFileName = CreateName(logger, checkTransformPoints[i].gameObject.name);
            logger.CreateFileName(reportFileName);

            loggers[i] = logger;
        }

        foreach (GameObject GO in checkChildrenColliders)
        {
            if (GO) // check if its assigned
            {
                Collider[] colliders = GO.GetComponentsInChildren<Collider>();

                foreach (Collider coll in colliders)
                {
                    colliderCheckList.Add(coll); // Add all the child colliders
                }
            }
        }

        Debug.Log("Number of colliders checked for proximity: " + colliderCheckList.Count.ToString());
            //Debug.LogError("Not all loggers are created, something went wrong with the proximitylogger, please check");
    }

    void Update()
    {
        for (int i = 0; i < checkTransformPoints.Length; i++)
        {
            if (!loggers[i].madeFile) return; // if no file is made, dont log

            // Logged variables
            Vector3 CP = Vector3.positiveInfinity; // The closest point found (from/rel to the current transform point)
            string closestColl = ""; // the name of the closest collider

            // Current position checked
            Vector3 pos = checkTransformPoints[i].position; // the position vector of the currently checked transform

            foreach (Collider coll in colliderCheckList)
            {
                Vector3 CPCurr = coll.ClosestPoint(pos);
                Vector3 currDistVec = CPCurr - pos;
                if (currDistVec.magnitude < CP.magnitude) // check if the currently checked collider is the closest one
                { // if it is, set the closest point to that one
                    CP = currDistVec;
                    closestColl = coll.gameObject.name;
                }
            }

            // Make the to be logged string
            string[] strings = new string[6]
            {
                checkTransformPoints[i].gameObject.name,
                closestColl,
                CP.x.ToString(),
                CP.y.ToString(),
                CP.z.ToString(),
                CP.magnitude.ToString()
            };

            loggers[i].write(strings); // log it
        }
    }



    public string CreateName(LogToCSV logger, string transName = "") // modified to take some arguments for easy implementation into this list based set-up
    {
        if (custom.Length > 0)
        {
            reportFileName = custom;
        }
        else if (includePlayerName || includeDate || includeObjectName || includeObjectName)
        {
            reportFileName = "proximity_";
            if (includeObjectName)
                reportFileName += transName + "_";
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
