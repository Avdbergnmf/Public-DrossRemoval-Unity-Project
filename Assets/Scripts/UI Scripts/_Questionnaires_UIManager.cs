using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class _Questionnaires_UIManager : MonoBehaviour
{
    // Did the logging in here as well, bc i was hella lazy
    [Header("Other GameObjects")]
    public Canvas questionnaireCanvas = null;
    public TrialOrderManager trialOrderManager = null;
    public SceneState sceneState = null;

    // Logging
    [Header("Questionnaire")]
    [SerializeField] public string reportFileName = "Questionnaire";
    [SerializeField] public string reportDirectoryName = "Report";

    private string[] reportHeaders = new string[2] {
            "Question",
            "Value"
    };

    Slider[] sliders;

    void Start()
    {
        sliders = questionnaireCanvas.GetComponentsInChildren<Slider>();
    }


    public void LogQuestionnaire() // Saves the playerinfo to the PlayerInfo object in this class (so that it can be used by other scripts), and logs it to a CSV
    {
        if (trialOrderManager.currConditionNo < trialOrderManager.conditionOrder.Length + 1) // to prevent out of bounds error
        {
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

            logger.CreatePath(trialOrderManager.conditionOrder[trialOrderManager.currConditionNo - 1], pName, false);
            logger.Create(trialOrderManager.currRep);

            foreach (var s in sliders)
            {
                string[] strings = new string[2] { // Put the playerinfo in a list of strings
                    s.transform.parent.gameObject.name, // Question number
                    s.value.ToString()
                };

                logger.write(strings, true);
            }

            resetSliders();
        }

        sceneState.startBreak();
    }


    public void resetSliders(int value = 2)
    {
        foreach (var s in sliders)
            s.value = value;
    }
}
