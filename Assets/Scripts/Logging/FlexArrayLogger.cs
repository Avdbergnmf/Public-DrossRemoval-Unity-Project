using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace NVIDIA.Flex
{
    [ExecuteInEditMode]
    [AddComponentMenu("NVIDIA/Flex/Flex Array Logger")]

    public class FlexArrayLogger: MonoBehaviour
    {
        #region DetectionProperties

        LogToCSV logger;

        [Header("Detection Properties")]
        [SerializeField]
        FlexArrayActor m_actor = null;
        [SerializeField]
        float refreshTime = 0.2f; // -> 5 Hz, instead of 50+Hz, so it doesn't cause as much cpu load.

        [SerializeField]
        Collider detectionRange = null;
        [ReadOnly] 
        public int inRange = 0;
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
        [SerializeField] public bool includeTestNumber = true;
        [SerializeField] public string custom;

        [SerializeField] public bool allowOverwrite = false;

        [Header("Log settings")]
        [SerializeField] public int writeEveryNLogs = 10;

        private string[] reportHeaders = new string[3] {
        "Actor Name",
        "Particles in Range",
        "Total particles"
        };

        #endregion

        #region Messages

        void OnEnable()
        {
            if (m_actor != null && Application.isPlaying)
            {
                logger = new LogToCSV(reportDirectoryName, reportHeaders, writeEveryNLogs, false);

                string reportFileName = CreateName();

                logger.CreateFileName(reportFileName);
            }
            else if (!Application.isPlaying)
                return; // for somereason the OnEnable runs at shutdown ???
            else
                Debug.Log("No Flex Array object selected.");

            if (refreshTime>0)
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

            if (detectionRange) inRange = scanBathForDross();

            if (m_actor == null)
                return;

            fraction = (inRange / (float)m_actor.particles.Length);

            //m_velocities = m_actor.velocities; // If i happen to want the velocities for something.

            if (!logger.madeFile) return;

            

            string[] strings = new string[3] {
                gameObject.name,
                inRange.ToString(),
                m_actor.particles.Length.ToString(),
            };

            logger.write(strings);
        }


        int scanBathForDross()
        {
            int detected = 0;
            for (int i = 0; i < m_actor.particles.Length; i++)
            {
                Vector3 CP = detectionRange.ClosestPoint(ToVector3(m_actor.particles[i]));

                if ((CP - ToVector3(m_actor.particles[i])).magnitude == 0) detected++;
            }
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
            else if (includePlayerName || includeObjectName || includeDate || includeTestNumber)
            {
                reportFileName = "";
                if (includePlayerName)
                {
                    StartSurveyScript startSurveyScript = FindObjectOfType<StartSurveyScript>();

                    if (startSurveyScript)
                        if (startSurveyScript.playerInfo.name.Length > 0)
                            reportFileName += startSurveyScript.playerInfo.name;
                        else
                            Debug.LogError("Player name empty!");
                }
                if (includeObjectName)
                    reportFileName += m_actor.name;
                if (includeDate)
                    reportFileName += System.DateTime.Today.ToShortDateString().Replace("/", "_");
                if (includeTestNumber)
                {
                    int i = 0;
                    reportFileName += "_";
                    if (File.Exists(logger.GetFilePath(reportFileName + i.ToString())))
                    {
                        //bool foundName = false;
                        while ((File.Exists(logger.GetFilePath(reportFileName + i.ToString()))))
                        {
                            i++;
                        }
                    }
                    reportFileName += i.ToString();
                }

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
}