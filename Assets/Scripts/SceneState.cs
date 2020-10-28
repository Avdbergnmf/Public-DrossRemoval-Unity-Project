using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.Presets;
using UnityEngine.EventSystems;
using Obi;
/*
This script manages the state of the scene all throughout the experiment
It handles which cues get turned on/off, which settings to load when, timers etc.

The trial order though it managed by trialOrderManager.cs
*/
public class Cue // This is a special class to contain the information about all the different cues, by putting this in lists it is easier to run a certain function for all cues in that type of list etc.
{ // Might be a bit overkill but turned out to work quite well in the end

    public string name; // The name of this cue (not really used for much other then to easily keep track)
    public GameObject[] GOs; // The gameobjects that should be toggled with the state of this cue
    public bool state; // state of the cue (on/off)

    public Button button; // Which button is paired to this cue?
    Image icon; // The icon image is extracted so that it can be manipulated

    float enabledAlpha = 1f; // what should the icon alpha be when the cue is enabled?
    float disabledAlpha = 0.3f; // and disabled?

    public Cue(string newName, GameObject[] newGOs, bool newState, Button newButton , bool setListener = true) //  The class constructor
    {
        state = newState;
        name = newName;
        GOs = newGOs;
        button = newButton;
        Image[] images = button.GetComponentsInChildren<Image>();
        icon = images[images.Length-1]; // For some reason it also finds its own image... The last one (i think) is always the latest one in the hierarchy
        
        setActive(); // update the state of the gameobjects and button alpha according to the state

        // Set button functionality
        if(setListener) // For some cues this listener got in the way, so I added the option to disable it.
            button.onClick.AddListener(() => buttonClick()); // Add the listener to the buttonclick to give it the functionality
    }
    

    public void setActive() // update the state of the gameobjects and button alpha according to the state
    {
        for (int i = 0; i < GOs.Length; i++)
        {
            GOs[i].SetActive(state);
            // Set button color
            if (state)
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, enabledAlpha);
            else
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, disabledAlpha);
        }
    }

    public void buttonClick()
    {
        state = !state;
        setActive();
    }

    public void DestroyListener() // Destroy the listener manually, else memory issues can occur
    {
        button.onClick.RemoveListener(() => buttonClick());
    }

    public void setButtonInteractable(bool state) // Set the interactibility of the attached button
    {
        button.interactable = state;
    }

    public void setExtra(bool newState) // this only makes sense to use for the display cues (for now), does some stuff to make sure the buttons toggle correcly
    {
        state = newState;

        setButtonInteractable(!newState);

        setActive();
    }

}

public class SceneState : MonoBehaviour
{
    #region Properties

    public CurrState currState = null; // The currState script holds the current state of the scene, so that it can be loaded in and saved
    public CanvasManager CM = null; // The CanvasManager (mostly) manages which canvasses will be turned on/off
    public LoggingManager loggingManager = null; // The LoggingManager manages how and when the loggers will log to the CSV files
    public ObiActor obiActor = null; // the obi actor containing the dross particles, used to reset their position
    public TrialOrderManager trialOrderManager = null;

    [Tooltip("The ResetRB script attached to the root of the body that will be reset at the start of each trial (robot)")] public ResetRB resetRB = null;

    [Header("Visual cues")] 
    // All the gameobjects that should be toggled for the visual cues
    [SerializeField] GameObject[] visualFRVF_GO = null;
    [SerializeField] GameObject[] visualWR_GO = null;
    [SerializeField] GameObject[] visualGVF_GO = null;
    [SerializeField] GameObject[] visualOmniCue_GO = null;
    [SerializeField] GameObject[] visualProjCue_GO = null;
    [SerializeField] GameObject[] visualHitCue_GO = null;
    // All the Buttons that should be linked to these visual cues
    [SerializeField] Button visualFRVF_Button = null;
    [SerializeField] Button visualWR_Button = null;
    [SerializeField] Button visualGVF_Button = null;
    [SerializeField] Button visualOmniCue_Button = null;
    [SerializeField] Button visualProjCue_Button = null;
    [SerializeField] Button visualHitCue_Button = null;
    // The list containing all the visualcues
    [HideInInspector] public List<Cue> visualCues = new List<Cue>();

    [Header("Haptic Cues")]
    // All the gameobjects that should be toggled for the haptic cues
    [SerializeField] GameObject[] hapticFRVF_GO = null;
    [SerializeField] GameObject[] hapticGVF_GO = null;
    // All the Buttons that should be linked to these haptic cues
    [SerializeField] Button hapticFRVF_Button = null;
    [SerializeField] Button hapticGVF_Button = null;
    // The list containing all the hapticcues
    [HideInInspector] public List<Cue> hapticCues = new List<Cue>();

    public HapticPlugin HP = null; // the haptic plugin, so that we can release on initiating etc.

    [Header("Display 'Cues'")] // (which display is it shown on)
    // All the gameobjects that should be toggled for the display cues
    [SerializeField] GameObject[] VR_GO = null;
    [SerializeField] GameObject[] Monitor_GO = null;
    // All the Buttons that should be linked to these display cues
    [SerializeField] Button VRButton = null;
    [SerializeField] Button MonitorButton = null;
    // The list containing all the hapticcues
    [HideInInspector] public List<Cue> displayCues = new List<Cue>();

    [Header("Presets")]
    // Presets
    public string presetsFolder = "Assets/Settings/CuePresets"; // where the presets are saved/found
    string type = "Preset"; // used for searching the presets 
    [HideInInspector] public string[] presets = null; // An array containing all the presets found in the presetsfolder


    // Boundary Conditions
    public bool failedGoal = false;
    public bool reachedGoal = false;
    public float removedProgress = 0.0f;

    // Hidden
    [HideInInspector] public GameObject[] rippleInstanceAtStartUp; // ripple VFX objects that are found at startup so they can be removed if needed (for example the WR object is always present at startup, but should not always be)
    [HideInInspector] public float startTime = 0;// used in UIManagers


    #endregion

    #region MainMethods

    public void Start()
    {
        setXRUI(false);

        // Make the lists with the cues
        MakeLists();

        // Update
        updateAllLists();
        updateAssetList();

        trialOrderManager.practiceState = trialOrderManager.doPractice; // Set the correct practicestate

        // Check the trialOrderManager (do we want to continue with another trial order)
        if (trialOrderManager.NewPlayer) // Is this a new player?
        {
            HP.release();
            CM.startScreen(); // show the startscreen first
        }
        else
            if (trialOrderManager.playerInfo == null) // Is the playerinfo filled in yet?
                CM.startMain(true); // Lets fill out the playerinfo survey...?
            else
                startCurrTrial();

    }

    // Shutdown
    private void OnDisable()
    {
        destroyListeners();
    }

    private void OnApplicationQuit()
    {
        destroyListeners();
    }

    private void OnDestroy()
    {
        destroyListeners();
    }

    // Destroy all listeners
    public void destroyListeners()
    {
        foreach (Cue cue in visualCues)
        {
            cue.DestroyListener();
        }
        foreach (Cue cue in hapticCues)
        {
            cue.DestroyListener();
        }
        foreach (Cue cue in displayCues)
        {
            cue.DestroyListener();
        }
    }

    #endregion

    #region StartFunctions
    public void startNextTrial() // Set up the next scene
    {
        loggingManager.setAllLogging(false); // Stops the loggers
        HP.release();
        resetRB.doReset();
        trialOrderManager.nextTrial();

        if (trialOrderManager.currRep == 0) 
        {
            if (trialOrderManager.doPractice && !trialOrderManager.practiceState) // We just switched the practice state, not the condition
                CM.setCanvas(CM.BreakCanvas);
            else if (trialOrderManager.currConditionNo != 0)
                CM.setCanvas(CM.QuestionnaireCanvas);
            else // its the first condition, so no questionnaire needed!
                initScene(); // set up the scene
        }
        else // Take a break
            CM.setCanvas(CM.BreakCanvas);

        trialOrderManager.Save(); // Save the trialOrderManager preset
    }

    public void startCurrTrial()
    {
        resetRB.doReset();
        setPreset(trialOrderManager.getCurrPresetNo()); // go to next and set the preset

        initScene(); // set up the scene

        if (trialOrderManager.currConditionNo != 0 && trialOrderManager.currRep != 0)
            trialOrderManager.Save(); // Save the trialOrderManager preset --> Can only do this once the paths have been assigned, so can't do this the first trial... which is fine
    }

    public void startTrial() // start the current trial, this is ran by the startInfoCanvas after the button on the omni is pressed
    {
        resetRB.resetKinState();

        startTime = Time.time;

        loggingManager.startRepetition(); // start all loggers

        CM.startVR(currState.VR, trialOrderManager.practiceState);
        setXRUI(currState.VR);

        // DrossReset
        resetObiActor(); // reset the particle positions 
    }

    public void startTutorialTrial()
    {
        HP.release();

        flushAll();

        startTime = Time.time;

        CM.startVR(currState.VR, trialOrderManager.practiceState);
        //setXRUI(currState.VR); // it could be enabled here to have VR UI, but decided not to.

        // DrossReset
        resetObiActor(); // reset the particle positions 
    }

    public void startBreak()
    {
        if (trialOrderManager.currConditionNo < trialOrderManager.conditionOrder.Length) //Have we had all conditions?
            CM.setCanvas(CM.BreakCanvas);
        else
            CM.setCanvas(CM.FinalQuestionnaire); // Do the Final Questionnaire
    }

    #endregion

    #region SetFunctions

    public void initScene() // (Re-)Initialize the scene objects
    {
        // Init the canvas
        CM.initStartCanvas();
        // Robot Reset
        HP.release(); // Release the controlled object

        // Reset the robot pose
        // Maybe force operator to touch object on other side in order to continue to the next trial

        setXRUI(false); // disable the XRUI functionality so the forms can be filled etc

        // delete the already created ripple instances so that they dont remain if not intended
        rippleInstanceAtStartUp = GameObject.FindGameObjectsWithTag("RippleInstance");
        foreach (var GO in rippleInstanceAtStartUp) 
        {
            var source = GO.GetComponent<AnimateSphereMask>().sourceGO;

            if (source != null)
            {
                if (!source.activeSelf)
                    Destroy(GO);
            }
        }

        HP.enableGrabbing = true; // enable the haptic plugin

        resetObiActor();

        setPreset(trialOrderManager.getCurrPresetNo()); // set the preset <<<<<<<<<<<<<<
    }

    public void resetObiActor() // just toggling it real quick resets all the positions
    {
        obiActor.enabled = false;
        obiActor.enabled = true;
    }

    public void setPreset(int set) // Set a preset from the presetlist
    {
        Preset preset = (Preset)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(presets[set]), typeof(Preset)); // Locate the preset file
        preset.ApplyTo(currState); // load it into currstate

        MakeLists(); // re-make the list objects, as they are now all incorrect relative to the currState
        setVR(currState.VR);
    }

    public void MakeLists() // Just repopulate all the lists so that 
    {// In the end resorted to this BS solution, works but not very clean, but whatever.
        destroyListeners(); // Make sure all listeners are removed first, so we dont get multiple listeners

        visualCues.Clear(); // NOTE: the new Cue instances are automatically collected by the garbage collector, so no need to delete them ourselves
        hapticCues.Clear();
        
        // make visualcues list
        visualCues.Add(new Cue("FRVF", visualFRVF_GO, currState.FRVF, visualFRVF_Button));
        visualCues.Add(new Cue("WR", visualWR_GO, currState.WR, visualWR_Button));
        visualCues.Add(new Cue("GVF", visualGVF_GO, currState.GVF, visualGVF_Button));
        visualCues.Add(new Cue("omni", visualOmniCue_GO, currState.omni, visualOmniCue_Button));
        visualCues.Add(new Cue("proj", visualProjCue_GO, currState.proj, visualProjCue_Button));
        visualCues.Add(new Cue("hit", visualHitCue_GO, currState.hit, visualHitCue_Button));

        // make haptic cues list
        hapticCues.Add(new Cue("FRVF", hapticFRVF_GO, currState.h_FRVF, hapticFRVF_Button));
        hapticCues.Add(new Cue("GVF", hapticGVF_GO, currState.h_GVF, hapticGVF_Button));

        // Make display cues
        displayCues.Add(new Cue("VR", VR_GO, currState.VR, VRButton, false));
        displayCues.Add(new Cue("Monitor", Monitor_GO, currState.Monitor, MonitorButton, false));
    }

    public void flushAll() // Flush all states to the currState script, so that it can be saved & viewed from the editor for reference
    { // (this is done according to the state of the gameobjects they are attached to, unfortunately, there is no way to make this cleaner code, that I know of?)
        currState.FRVF = visualFRVF_GO[0].activeSelf;
        currState.WR = visualWR_GO[0].activeSelf;
        currState.GVF = visualGVF_GO[0].activeSelf;
        currState.omni = visualOmniCue_GO[0].activeSelf;
        currState.proj = visualProjCue_GO[0].activeSelf;
        currState.hit = visualHitCue_GO[0].activeSelf;
        currState.h_FRVF = hapticFRVF_GO[0].activeSelf;
        currState.h_GVF = hapticGVF_GO[0].activeSelf;

        currState.VR = VR_GO[0].activeSelf;
        currState.Monitor = Monitor_GO[0].activeSelf;
    }

    public void setAllButtonsInteractable(bool state)
    {
        foreach (Cue cue in visualCues)
        {
            cue.setButtonInteractable(state);
        }
        foreach (Cue cue in hapticCues)
        {
            cue.setButtonInteractable(state);
        }
        foreach (Cue cue in displayCues)
        {
            cue.setButtonInteractable(state);
        }
    }

    public void setVR(bool state)
    {
        if (state) // some additional functionalities to make these buttons work the way I want
        {
            displayCues[0].setExtra(true);
            displayCues[1].setExtra(false);
        }
        else
        {
            displayCues[0].setExtra(false);
            displayCues[1].setExtra(true);
        }
    }

    public void setXRUI(bool state) // Make this less stupid, just turn on after the first 
    {
        EventSystem.current.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>().enabled = state;
    }

    #endregion

    #region UpdateFunctions

    // Update the cue by name!
    public void updateByName(List<Cue> cues, string name)
    {
        foreach (Cue cue in cues)
        {
            if (cue.name == name)
            {
                cue.state = !cue.state;
                cue.setActive();
                break;
            }
        }
    }

    public void updateAssetList()
    {
        presets = FindAssetsByType(type, presetsFolder);
    }

    void updateList(List<Cue> cues) // Re-set the active state of the GO's of all the cues in this list to match their state
    {
        foreach (Cue cue in cues)
        {
            cue.setActive();
        }
    }
    public void updateAllLists() // Update all the lists in this scene
    {
        updateList(visualCues);
        updateList(hapticCues);
        
        setVR(!currState.VR);
    }

    #endregion


    #region SettingsPresets
    // Preset stuff
    // This method uses a Preset to copy the serialized values from the source to the target and return true if the copy succeed.
    public static bool CopyObjectSerialization(Object source, Object target)
    {
        Preset preset = new Preset(source);
        return preset.ApplyTo(target);
    }

    // This method creates a Preset from a given Object and save it as an asset in the project.
    public void CreatePresetAsset(Object source, string name, string folder)
    {
        Preset preset = new Preset(source);
        AssetDatabase.CreateAsset(preset, folder + '/' + name + ".preset");
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