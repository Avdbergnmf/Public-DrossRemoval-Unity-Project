using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditorInternal;

public class _MainMenu_UIMan : MonoBehaviour
{
    #region Properties
    [Header("Parent GOs")]
    public GameObject loggingManager = null; // Manages all the loggers
    public SceneState sceneState = null; // the sceneState script used for controlling/finding the scene state
    public Canvas CanvasMainMenu = null; // This canvas (used to e.g. disable some buttons etc)
    
    CurrState currState = null; // The current state of the scene, used to load/save the presets

    [Header("UI GOs")]
    public Dropdown presetDropDown = null;
    public InputField inputField = null;

    public Canvas playerInfoCanas = null; // shows the survey about basic player info
    public Canvas startInfoCanvas = null; // Shows the connect robot to start screen

    [Header("Tutorial GOs")]
    public GameObject[] tutorialPanels = null;
    public CollisionLoggingManager CLM = null;
    public Text KOText = null;

    #endregion

    #region MainMethods

    private void Update()
    {
        if (CLM.violatedForbidden)
        {
            KOText.canvasRenderer.SetAlpha(1.0f);
            KOText.CrossFadeAlpha(0.0f, 1.5f, false);
        }

        if (CLM.violatedForce)
        {
            KOText.canvasRenderer.SetAlpha(1.0f);
            KOText.CrossFadeAlpha(0.0f, 1.5f, false);
        }
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        KOText.canvasRenderer.SetAlpha(0.0f);

        currState = sceneState.currState;

        // disable the canvasses that should be on enable
        playerInfoCanas.gameObject.SetActive(false);
        startInfoCanvas.gameObject.SetActive(false);

        StartCoroutine(LateStart(0.1f)); // Else the dropdown isn't filled. This prevents having to use the update function
    }

    IEnumerator LateStart(float waitTime) // this function starts a bit after the onEnable, to make sure some update loops have passed
    {
        yield return new WaitForSeconds(waitTime);
        fillDropDown(); // This function can only find the presets after the first update
        
        sceneState.rippleInstanceAtStartUp = GameObject.FindGameObjectsWithTag("RippleInstance"); // these can only be found after the first update
    }

    public void startNewPlayer() // The environment for a new player, that will have to fill in the playerinfo
    {
        setPresetMenu(0);
        lockMainMenu(true);

        // Startup the player info survey canvas
        playerInfoCanas.gameObject.SetActive(true);

        sceneState.setXRUI(false); // disable the XRUI so the info can be filled in

        sceneState.HP.enableGrabbing = false; // disable the haptic functionality
        // DO SOMETHING TO DISABLE ALL HAPTIC EFFECTS
    }

    public void lockMainMenu(bool state) // lock or unlock the main menu
    {
        // Lock the scene settings
        presetDropDown.interactable = !state;
        sceneState.setAllButtonsInteractable(!state); // make sure none of the buttons are interactable so the user cant change the scene state
    }

    public void initStartInfoCanvas()
    {
        startInfoCanvas.gameObject.SetActive(true);
    }

    #endregion

    #region TutorialMethods
    public void enableTutPanel(int i)
    {
        setAllTutPanels();

        if (sceneState.trialOrderManager.tutorialState)
            tutorialPanels[i].SetActive(true);
    }
    public void setAllTutPanels(bool state = false)
    {
        if (sceneState.trialOrderManager.tutorialState)
            foreach (var pan in tutorialPanels)
                pan.SetActive(state);
    }


    #endregion

    #region PresetFunctions
    public void fillDropDown() // Fill the dropdown selector with the found presets
    {
        presetDropDown.options.Clear();

        foreach (string preset in sceneState.presets)
        {
            string[] name = AssetDatabase.GUIDToAssetPath(preset).Split('/');
            presetDropDown.options.Add(new Dropdown.OptionData() { text = name[name.Length-1]});
        }
    }

    public void savePreset() // Save a preset when the save preset button is pressed
    {
        currState = sceneState.currState;
        sceneState.flushAll();
        sceneState.CreatePresetAsset(currState, inputField.text, sceneState.presetsFolder);

        sceneState.updateAssetList();
        fillDropDown();
    }

    public void setPresetMenu(int set) // When a preset is selected, set this canvas accordingly (not really functional, just so it makes sense for the user)
    {
        presetDropDown.value = set;
        inputField.text = presetDropDown.options[set].text.Split('.')[0];
        presetDropDown.enabled = false;
    }

    #endregion
}
