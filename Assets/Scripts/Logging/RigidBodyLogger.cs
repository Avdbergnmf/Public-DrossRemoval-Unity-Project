using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class RigidBodyLogger : MonoBehaviour
{
    public GameObject loggedObject = null;

    LogToCSV logger;

    [Header("File Name")]
    [ReadOnly] public string reportFileName = "";
    [SerializeField] public string reportDirectoryName = "Report";
    [SerializeField] public bool includeObjectName = true;
    [SerializeField] public bool includePlayerName = true;
    [SerializeField] public bool includeDate = true;
    [SerializeField] public string custom;

    [SerializeField] public bool allowOverwrite = false;

    [Header("Log settings")]
    [SerializeField] public int writeEveryNLogs = 100;

    private string[] reportHeaders = new string[19] {
        "Object",
        "Position X",
        "Position Y",
        "Position Z",
        "Rotation X",
        "Rotation Y",
        "Rotation Z",
        "Rotation W",
        "LinVelocity X",
        "LinVelocity Y",
        "LinVelocity Z",
        "AngVelocity X",
        "AngVelocity Y",
        "AngVelocity Z",
        "Impact Force X",
        "Impact Force Y",
        "Impact Force Z",
        "Impact Force Magnitude",
        "Colliding Body",
    };

    Rigidbody body;
    CollisionLoggingManager collisions;

    void Start()
    {
        logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs);

        string reportFileName = CreateName();

        logger.CreateFileName(reportFileName);

        // For rigging the rigid bodies
        body = loggedObject.GetComponent<Rigidbody>();

        if (!body)
        {
            Debug.LogError("GameObject has no RigidBody component!");
            logger.madeFile = false; // so that it does not go through update loop 
            return;
        }
        else
        {
            collisions = loggedObject.GetComponent<CollisionLoggingManager>();
            if (!collisions)
            {
                Debug.LogError("No collision logging manager on the rigid body, not logging collisions");
                collisions = new CollisionLoggingManager(); // for now, just fill it with empty values
                collisions.collidingBodyName = "";
                collisions.collisionForce = Vector3.zero;
            }
        }
    }

    void Update()
    {
        if (!logger.madeFile) return;

        string[] strings = new string[19] {
                body.name.ToString(),
                body.transform.position.x.ToString(),
                body.transform.position.y.ToString(),
                body.transform.position.z.ToString(),
                body.transform.rotation.x.ToString(),
                body.transform.rotation.y.ToString(),
                body.transform.rotation.z.ToString(),
                body.transform.rotation.w.ToString(),
                body.velocity.x.ToString(),
                body.velocity.y.ToString(),
                body.velocity.z.ToString(),
                body.angularVelocity.x.ToString(),
                body.angularVelocity.y.ToString(),
                body.angularVelocity.z.ToString(),
                collisions.collisionForce.x.ToString(),
                collisions.collisionForce.y.ToString(),
                collisions.collisionForce.z.ToString(),
                collisions.collisionForce.magnitude.ToString(),
                collisions.collidingBodyName
        };

        logger.write(strings);
    }

    public string CreateName()
    {
        if (custom.Length > 0)
        {
            reportFileName = custom;
        }
        else if (includePlayerName || includeObjectName || includeDate)
        {
            reportFileName = "RB_";
            if (includeObjectName)
                reportFileName += loggedObject.name + "_";
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
