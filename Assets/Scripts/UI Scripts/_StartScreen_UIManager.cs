using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Presets;

public class _StartScreen_UIManager : MonoBehaviour
{
    public TrialOrderManager trialOrderManager = null;
    public CanvasManager CM = null;

    [Header("UI Elements")]
    public GameObject buttonsPanel = null;
    public GameObject dropDownPanel = null;

    [Header("TrialOrder Preset Settings")]
    [Tooltip("The dropdown that will contain all the trialOrder presets to select from")]
    public Dropdown presetDropDown = null;
    [Tooltip("The folder that contains the trialOrder presets")]
    public string trialOrderPresetsFolder = "Assets/Settings/TrialOrders"; // that rhymes lel
    string type = "Preset"; // used for searching the presets 
    [HideInInspector] public string[] presets = null; // An array containing all the presets found in the presetsfolder


    public void Start()
    { // Start off with the buttonsPanel active
        goToButtonsPanel();
    }

    public void startSelect()
    {
        presets = FindAssetsByType(type, trialOrderPresetsFolder);
        fillDropDown();
        buttonsPanel.SetActive(false);
        dropDownPanel.SetActive(true);
        setPreset(0);
    }

    public void startSelectedPreset()
    {
        CM.startMain(true);
    }

    public void goToButtonsPanel()
    {
        buttonsPanel.SetActive(true);
        dropDownPanel.SetActive(false);
    }

    public void startCustomTrial()
    {
        CM.startMain(false);
    }


    #region HelperFunctions
    public void fillDropDown() // Fill the dropdown selector with the found presets
    {
        presetDropDown.options.Clear();

        foreach (string preset in presets)
        {
            string[] name = AssetDatabase.GUIDToAssetPath(preset).Split('/');
            presetDropDown.options.Add(new Dropdown.OptionData() { text = name[name.Length - 1] });
        }
    }
    public void setPreset(int set) // Set a preset from the presetlist
    {
        Preset preset = (Preset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(presets[set]), typeof(Preset)); // Locate the preset file
        preset.ApplyTo(trialOrderManager); // load it into trialOrderManager
        trialOrderManager.practiceState = trialOrderManager.doPractice; // Set the correct practicestate
    }

    public static string[] FindAssetsByType(string type, string folder)
    {
        Debug.Log("*** FINDING ASSETS BY TYPE ***");

        string[] guids;

        // search for a ScriptObject called ScriptObj
        guids = AssetDatabase.FindAssets("t:" + type, new[] { folder });
        /*        foreach (string guid in guids)
                {
                    Debug.Log("Asset: " + AssetDatabase.GUIDToAssetPath(guid));
                }*/
        return guids;
    }
    #endregion
}
