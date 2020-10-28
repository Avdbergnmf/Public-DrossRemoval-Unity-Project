using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// General purpose script that logs all the data to CSV, so that they can be plotted later with external software
// Opening/closing files is quite resource intensive, so make sure to write the data in batches (to prevent missed frames)

public class LogToCSV
{
    // public 
    public string fileName = "";
    public string finalFileName = "";
    public bool madeFile = false;
    public bool writeLogs = false;


    // Settings
    //bool placeInSeperateFolder;
    string reportDirectoryName;
    string[] reportHeaders;

    string reportSeparator = ",";
    string timeStampHeader = "Time Stamp";
    int writeEveryNLogs;

    //public bool writeOnQuit;

    public float startingTime = 0f;


    // Stuff that is used to keep track
    private string uberFinalString = "";
    private int unsavedLogNo = 0;

    public string reportDirectoryPath; // set to public for easy access, better to only use get function...

    public LogToCSV(string newReportDirectoryName, string[] newReportHeaders, int newWriteEveryNLogs, bool addToLoggingManager = true)
    {
        reportHeaders = newReportHeaders;
        writeEveryNLogs = newWriteEveryNLogs;
        reportDirectoryName = newReportDirectoryName;

        LoggingManager[] LM = GameObject.FindObjectsOfType<LoggingManager>();

        if (LM.Length == 1 && addToLoggingManager)
            LM[0].AddLogger(this); // place this logtocsv instance in a list in the logging manager so that we can easily work with multiple different loggers
    }

    public bool write(string[] strings, bool forceWrite = false)
    {
        if (writeLogs || forceWrite)
        {
            appendToUberString(strings, forceWrite);
            return true;
        }
        else
            return false;
    }


    #region Interactions
    private void AppendToReport()
    {
        using (StreamWriter sw = File.AppendText(GetFilePath(finalFileName)))
        {
            sw.Write(uberFinalString);
            uberFinalString = "";
            unsavedLogNo = 0;
        }
    }

    private void appendToUberString(string[] strings, bool forceWrite = false)
    {
        string finalString = "";
        for (int i = 0; i < strings.Length; i++)
        {
            if (finalString != "")
            {
                finalString += reportSeparator;
            }
            finalString += strings[i];
        }
        finalString += reportSeparator + getTimeStamp();
        finalString += "\n";
        uberFinalString += finalString;
        unsavedLogNo++;

        if (unsavedLogNo >= writeEveryNLogs || forceWrite)
            AppendToReport();
    }

    public void ShutDown()
    {
        if (madeFile)
            AppendToReport();
    }


    #endregion


    #region Create

    public void CreateFileName(string reportFileName) // this NEEDS to be called before the create function
    {
        fileName = reportFileName;
    }

    public void CreatePath(int currPresetNo, string playerName = "", bool practiceState = false) // this NEEDS to be called before the createreport function
    {
        string path = "Assets/" + reportDirectoryName;
            
        if (playerName.Length > 0)
        {
            path += "/" + playerName;
        }

        if (currPresetNo >= 0) // In this way, a negative number can be used to make logs appear in the players "root" folder
            path += "/" + currPresetNo.ToString(); // Log all the trials for this condition in the folder related to the loaded preset

        if (practiceState)
            path += "/practice";

        reportDirectoryPath = path;
        //Debug.Log(path);
    }

    public void Create(int trialNo = -1) // this is called by the logging manager once everything is set up
    {
        if (fileName.Length > 0)
        {
            finalFileName = fileName;
            if (trialNo >= 0) // only append if a number of at least 0 is given
                finalFileName += "_" + trialNo.ToString();

            CreateReport(finalFileName);
            madeFile = true;
        }
        else
        {
            Debug.LogError("FILE NAME NOT SET BEFORE CREATING LOG");
            madeFile = false;
        }
    }

    public void VerifyDirectory()
    {
        string dir = reportDirectoryPath;

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    void CreateReport(string name)
    {
        VerifyDirectory();
        if (File.Exists(GetFilePath(name)))
        {
            Debug.Log("<color=red>WARNING: </color>overwriting previous report! With name: " + name);
            File.Delete(GetFilePath(name));
        }
        using (StreamWriter sw = File.CreateText(GetFilePath(name)))
        {
            string finalString = "";
            for (int i = 0; i < reportHeaders.Length; i++)
            {
                if (finalString != "")
                {
                    finalString += reportSeparator;
                }
                finalString += reportHeaders[i];
            }
            finalString += reportSeparator + timeStampHeader;
            sw.WriteLine(finalString);
        }
    }

    public void CreateSingleEntry(string[] strings, string playerName) // special function to force write a single entry (used for the playerInfoLog)
    {
        CreatePath(-1, playerName.Replace(" ", "")); // log it to the players root folder (using negative number for currPresetNo
        Create();
        appendToUberString(strings, true);
    }

    #endregion

    #region Queries

    public string GetFilePath(string finalFileName)
    {
        return reportDirectoryPath + "/" + finalFileName + ".csv";
    }

    public string getTimeStamp() // time since the log file was created
    {
        return (Time.time - startingTime).ToString();
    }

    #endregion
}

