using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This script manages all the loggers in the scene.
Add the loggers to the loggers list.
All loggers in the list get triggered/managed simultaneously.
*/

public class LoggingManager : MonoBehaviour
{
    [SerializeField] public TrialOrderManager trialOrderManager = null;
    [SerializeField] public bool logPractice = true; // Also log practiceruns?

    [ReadOnly] public List<LogToCSV> loggers = new List<LogToCSV>();

    public void startRepetition()
    {
        bool pracState = trialOrderManager.practiceState;
        int currPresetNo = trialOrderManager.getCurrPresetNo();
        int currRep = trialOrderManager.currRep;

        CreatePaths(currPresetNo, pracState);

        if (logPractice || pracState)
        {
            CreateAll(currRep);
            setAllLogging(true); // enable the logging

            SetAllStartTime(Time.time);
        }
        else
            setAllLogging(false); // disable the logging
    }

    public void CreatePaths(int presetNo, bool practiceState = false)
    {
        //StartSurveyScript startSurveyScript = FindObjectOfType<StartSurveyScript>();

        string pName = "";
        if (trialOrderManager)
            if (trialOrderManager.playerInfo.name.Length > 0)
                pName = trialOrderManager.playerInfo.name.Replace(" ","");
            else
                Debug.LogError("Player name empty!");

        foreach (LogToCSV log in loggers)
        {
            log.CreatePath(presetNo, pName, practiceState); // trial number
        }
    }

    public void CreateAll(int trialNo)
    {
        if (loggers[0].reportDirectoryPath.Length > 0)
        {
            foreach (LogToCSV log in loggers)
            {
                log.Create(trialNo); // trial number
            }
        }
        else
        {
            Debug.LogError("Don't call the CreateAll function before the CreatePaths function!");
        }
    }

    public void SetAllStartTime(float startTime)
    {
        foreach (LogToCSV log in loggers)
        {
            log.startingTime = startTime;
        }
    }

    public void setAllLogging(bool state)
    {
        foreach(LogToCSV log in loggers)
        {
            log.writeLogs = state;
        }
    }

    public void AddLogger(LogToCSV logger)
    {
        loggers.Add(logger);
    }

    public void RmLogger(LogToCSV logger)
    {
        loggers.Remove(logger);
    }



    // Shutdown
    private void OnApplicationQuit()
    {
        foreach (LogToCSV log in loggers)
        {
            log.ShutDown(); 
        }
    }
    private void OnDisable()
    {
        foreach (LogToCSV log in loggers)
        {
            log.ShutDown(); 
        }
    }
    private void OnDestroy()
    {
        foreach (LogToCSV log in loggers)
        {
            log.ShutDown();
        }
    }
}
