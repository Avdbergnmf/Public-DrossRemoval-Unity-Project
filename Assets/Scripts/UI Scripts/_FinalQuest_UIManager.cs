using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class _FinalQuest_UIManager : MonoBehaviour
{
    public SceneState ss = null;
    // Did the logging in here as well, bc i was hella lazy
    [Header("Other GameObjects")]
    public TrialOrderManager trialOrderManager = null;
    public CanvasManager canvasManager = null;
    public InputField Q1 = null;
    public ToggleGroup Q2 = null;
    public InputField Q3 = null;
    public InputField Q4 = null;

    // Logging
    [Header("Logging")]
    [SerializeField] public string reportFileName = "FinalQuestionnaire";
    [SerializeField] public string reportDirectoryName = "Report";

    private Toggle[] toggles = null;

    private string[] reportHeaders = new string[2] {
            "Question",
            "Value"
    };

    private void OnEnable()
    {
        if (ss)
            ss.setXRUI(false);
    }


    private void Awake()
    {
        // Set the correct order of the buttons
        toggles = Q2.GetComponentsInChildren<Toggle>();

        toggles[trialOrderManager.conditionOrder[0]].transform.SetSiblingIndex(0);
        toggles[trialOrderManager.conditionOrder[1]].transform.SetSiblingIndex(1);
        toggles[trialOrderManager.conditionOrder[2]].transform.SetSiblingIndex(2);
        toggles[trialOrderManager.conditionOrder[3]].transform.SetSiblingIndex(3);
    }


    public void LogQuestionnaire() // Saves the playerinfo to the PlayerInfo object in this class (so that it can be used by other scripts), and logs it to a CSV
    {
        Dictionary<int, string> Questions = new Dictionary<int, string>();

        // Write this to a CSV file
        LogToCSV logger = new LogToCSV(reportDirectoryName, reportHeaders, 0, false);
        logger.CreateFileName(reportFileName);

        StartSurveyScript startSurveyScript = FindObjectOfType<StartSurveyScript>();
        string pName = "";
        if (startSurveyScript)
            if (startSurveyScript.playerInfo.name.Length > 0)
                pName = startSurveyScript.playerInfo.name.Replace(" ", "");
            else
                Debug.LogError("Player name empty!");

        logger.CreatePath(-1, pName, false);
        logger.Create();

        // Fill the Dict
        Questions.Add(1, '"' + Q1.text + '"'); // Put quotes to prevent additional cells
        Questions.Add(2, ActiveToggle().ToString());
        Questions.Add(3, '"' + Q3.text + '"');
        Questions.Add(4, '"' + Q4.text + '"');

        // Write to logger
        foreach (var q in Questions)
        {
            string[] strings = new string[2] { // Put the playerinfo in a list of strings
                q.Key.ToString(), // Question number
                q.Value
            };

            logger.write(strings, true);
        }

        canvasManager.setCanvas(canvasManager.FinalCanvas);
    }

    public int ActiveToggle() // Returns the preffered experimental condition
    {
        int activeToggle = -1;

        for (int i = 0; i < toggles.Length; i++)
            if (toggles[i].isOn)
                activeToggle = i;

        return activeToggle;
    }
}
