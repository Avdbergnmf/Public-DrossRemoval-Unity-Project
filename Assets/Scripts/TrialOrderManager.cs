using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;

// This script is here to manage the an array containing the order in which the presets will be given to the participant
// In this way we can keep track of which trial we are currenly at and which one we need to proceed to
// Whats more, this information can be saved, so that if a crash were to happen, we can pick up where we left off!

public class TrialOrderManager : MonoBehaviour
{
    #region Properties
    [Header("Main Settings")]
    [Tooltip("Do you want to enable practice runs?")] [SerializeField] public bool doPractice = true;
    [Tooltip("The number of trials per condition")] [SerializeField] public int numberOfRepetitions = 3;
    [Tooltip("How many secs should each trial take?")] [SerializeField] public float secPerTrial = 300;
    [Tooltip("How many secs should the participant take a break for each trial?")] [SerializeField] public float breakTimePerTrial = 60;
    [Tooltip("Should the collision violations be checked for?")] [SerializeField] public bool checkViolations = true;


    [Header("Settings")]
    [Tooltip("Set this to false to use the order that is set in trialOrder in this script, this should be false when loading in a preset")]
    public bool NewPlayer = true; 
    [Tooltip("Enable this if the playerinfo still needs to be filled in (the first trial isnt done yet).")] 
    public bool firstTrial = true; 
    [Tooltip("Do you want to save the state of this component as a preset? This is useful so it can be loaded in later upon crashes etc.")]
    public bool saveCurrOrder = true;

    [Header("current trial order state")]
    public int[] conditionOrder = new int[0]; // an array containing the order in which the conditions are presented to the operator
    public int currConditionNo = 0; // which condition no are we at?
    public int currRep = 0;
    public bool practiceState = true;
    [ReadOnly] public bool tutorialState = false;

    [Header("The Saved PlayerInfo")]
    // Lets also save the player info here, so the form doesn't have to be filled in twice
    public PlayerInfo playerInfo = null;

    // private
    StartSurveyScript startSurveyScript;
    SceneState sceneState;
    LoggingManager LM;

    #endregion

    #region MainMethods

    void Start() // This is not very optimized, but convenient cause no other scripts will depend on this one and it will require minimum setup to use!
    {
        // Do we want to include practice trials?
        practiceState = doPractice;

        // Find the player info
        startSurveyScript = FindObjectOfType<StartSurveyScript>();
        if (!startSurveyScript) Debug.LogError("No startsurveyscript found! Can't get the player info");

        // find the scene state
        sceneState = FindObjectOfType<SceneState>();
        if (!sceneState) Debug.LogError("No SceneState found! Can't get scene state info");

        // find the logging manager (to get the folder where to save the presets)
        LM = FindObjectOfType<LoggingManager>();
        if (!LM) Debug.LogError("No LoggingManager found! Can't get the folder in which to store the preset");

        if (!NewPlayer) // Use the one currently assigned, so make sure it is assigned!
            if (conditionOrder.Length == 0) Debug.LogError("NewPlayer set to false but no preset loaded yet!");
    }



    #endregion

    #region trialManagement
    public void nextTrial()
    {
        if (practiceState) // is the current run a testrun?
            practiceState = false; // make it a good run, keep the rest the same.
        else
        {
            if (++currRep < numberOfRepetitions) // Do we need to do more repetitions?
            { // Progress to next repetition
                //currRep++;
                // Do something
                Debug.Log("Next Trial: " + currRep.ToString());
            }
            else
            {
                if (currConditionNo + 1 <= conditionOrder.Length)
                {
                    currRep = 0; // Reset to 0
                    currConditionNo++;

                    if (doPractice)
                        practiceState = true;
                }
                else
                {
                    Debug.Log("EXPERIMENT IS FINISHED!");
                    currConditionNo++;
                    currRep = 0; // So that scenestate can progress to the questionnaire
                }
            }
        }
    }

    public int getCurrPresetNo()
    {
        //Debug.Log(currConditionNo);
        int presetNo = conditionOrder[currConditionNo];
        return presetNo;
    }


    #endregion

    #region Updating

    public void Save()
    {
        UpdatePlayerInfo();
        if (saveCurrOrder)
            SavePreset();
    }

    public void UpdatePlayerInfo() // <<<<<<When should this be run?
    {
        if (startSurveyScript) playerInfo = startSurveyScript.playerInfo; // update the playerinfo
    }
    #endregion

    #region PresetSaving
    public void SavePreset()
    {
        if (startSurveyScript && sceneState && LM)
        {
            LM.loggers[0].VerifyDirectory(); // Make sure the directory exists, else create it

            bool tmp = NewPlayer;
            NewPlayer = false; // so that it when you load it you dont need to set this by hand
            CreatePresetAsset(this, "trialOrderManager", LM.loggers[0].reportDirectoryPath.Replace("/practice", "") + "/.."); // be sure that its not placed in the practicefolder, if we're logging runs!

            NewPlayer = tmp; // Set it back to whatever it was
        }
    }

    // This method creates a Preset from a given Object and save it as an asset in the project.
    public void CreatePresetAsset(Object source, string name, string folder)
    {
        Preset preset = new Preset(source);
        AssetDatabase.CreateAsset(preset, folder + '/' + name + ".preset");
    }
    #endregion

    #region Randomized
    public void createRandomizedOrder()
    {
        // Make a randomized list and assign the first preset
        conditionOrder = new int[sceneState.presets.Length];
        for (int i = 0; i < conditionOrder.Length; i++)
            conditionOrder[i] = i;

        ShuffleArray(conditionOrder);
    }
    public static void ShuffleArray<T>(T[] arr) // Helper function to shuffle an array of the given type
    {
        for (int i = arr.Length - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            T tmp = arr[i];
            arr[i] = arr[r];
            arr[r] = tmp;
        }
    }
    #endregion
}
