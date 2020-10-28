using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Obi;
using System.Net.Http.Headers;
using System.Linq;

public class ObiActorLogger : MonoBehaviour
{
    #region DetectionProperties

    LogToCSV logger;

    [Header("Detection Properties")]
    [SerializeField] public ObiActor m_actor = null;
    [SerializeField]
    float refreshTime = 0.10f; // -> 20 Hz, instead of 50+Hz, so it doesn't cause as much cpu load. This used to be much more of a problem for the flex actor, the obi actors are a lot lighter, left it in to keep this script flexible and the refresh rate isnt that big of a deal for this logger

    [SerializeField] Collider detectionRangeZinc = null;
    [SerializeField] Collider[] detectionRangeBin = null;
    [SerializeField] bool requireBinDeposit = true;

    [ReadOnly] 
    public int inBath = 0;
    [ReadOnly]
    public int[] inBin;
    [ReadOnly]
    public float fraction = 1.0f;
    [ReadOnly]
    public int numParticles = 0;

    private CustomFixedUpdate FU_instance;

    #endregion

    #region LoggingProperties
    [Header("File Name")]
    [ReadOnly] public string reportFileName = "";
    [SerializeField] public string reportDirectoryName = "Report";
    [SerializeField] public bool includePlayerName = false;
    [SerializeField] public bool includeObjectName = true;
    [SerializeField] public bool includeDate = true;
    [SerializeField] public string custom;

    [SerializeField] public bool allowOverwrite = false;

    [Header("Log settings")]
    [SerializeField] public int writeEveryNLogs = 10;

    private string[] reportHeaders = new string[4] {
    "Particles in Bath",
    "Particles in Bin 1",
    "Particles in Bin 2",
    "Total particles"
    };

    #endregion

    #region Messages

    void OnEnable()
    {
        if (m_actor != null && Application.isPlaying)
        {
            logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs);

            // Set the name of the file to be logged to in the class
            string reportFileName = CreateName(); 
            logger.CreateFileName(reportFileName); // only creates the name inside the class
        }
        else if (!Application.isPlaying)
            return; // for somereason the OnEnable runs at shutdown ???
        else
            Debug.Log("No Obi Array object selected.");

        if (refreshTime>0) // this allows easy toggling of by making refreshtime -1
            FU_instance = new CustomFixedUpdate(refreshTime, OnFixedUpdate); //so that the updating rate can be changed as it is quite cpu intensive (a LOT of data to process)
        else
        {
            // Do nothing?
        }
    }

    private void Update()
    {
        if (FU_instance != null && refreshTime > 0)
            FU_instance.Update();
    }

    #endregion

    #region DrossDetection

    private void OnFixedUpdate(float dt)
    {
        fixedUpdateFunc();
    }

    public void fixedUpdateFunc() // run this function to make one log (only do this at the important times, running it at regular intervals using the fixedupdate is really intensive
    {
        if (!Application.isPlaying) return;

        if (detectionRangeZinc) inBath = scanBathForDross(); // These are quite resource intensive, might consider doing them at larger intervals
        else inBath = -1; // make it -1 to show that this data is missing
        if (detectionRangeBin.Length>0) inBin = scanBinsForDross();

        if (m_actor.isLoaded)
        {
            numParticles = m_actor.particleCount;
            if (requireBinDeposit)
            {
                fraction = ((numParticles - inBin.Sum())/ (float)numParticles);
            }
            else
            {
                fraction = (inBath / (float)numParticles);
            }
                
            if (!logger.madeFile) return;

            string[] strings = new string[4] 
            {
                inBath.ToString(),
                inBin[0].ToString(),
                inBin[1].ToString(),
                numParticles.ToString(),
            };

            logger.write(strings);
        }
    }


    int scanBathForDross()
    {
        int detected = 0;
        if (detectionRangeZinc)
            for (int i = 0; i < m_actor.particleCount; i++)
            {
                int solverIndex = m_actor.solverIndices[i];

                Vector3 CP = detectionRangeZinc.ClosestPoint(m_actor.GetParticlePosition(solverIndex));

                if ((CP - m_actor.GetParticlePosition(solverIndex)).magnitude == 0) detected++;
            }
        else
            Debug.LogError("Can't find detectionrangeZinc");

        return detected;
    }
    int[] scanBinsForDross()
    {
        var Nbins = detectionRangeBin.Length;
        int[] detected = new int[Nbins]; 

        if (detectionRangeBin.Length > 0)
        {
            for (int i = 0; i < m_actor.particleCount; i++)
            {
                int solverIndex = m_actor.solverIndices[i];

                // could put all this in a loop, but this is a bit cleaner, as theres only 2 bins anyway...
                Vector3[] CP = new Vector3[Nbins];
                CP[0] = detectionRangeBin[0].ClosestPoint(m_actor.GetParticlePosition(solverIndex));
                CP[1] = detectionRangeBin[1].ClosestPoint(m_actor.GetParticlePosition(solverIndex));

                if ((CP[0] - m_actor.GetParticlePosition(solverIndex)).magnitude == 0) detected[0]++;
                if ((CP[1] - m_actor.GetParticlePosition(solverIndex)).magnitude == 0) detected[1]++;
            }
        }
        else
            Debug.LogError("Can't find detectionRangeBin");

        return detected;
    }

    Vector3 ToVector3(Vector4 parent)
    {
        return new Vector3(parent.x, parent.y, parent.z);
    }
    #endregion

    public string CreateName()
    {
        if (custom.Length > 0)
        {
            reportFileName = custom;
        }
        else if (includePlayerName || includeObjectName || includeDate)
        {
            reportFileName = "obi_";
            if (includeObjectName)
                reportFileName += m_actor.name + "_";
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