using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    TrialOrderManager trialOrderManager = null;
    public _MainMenu_UIMan mainCanvasMan = null;
    // Canvasses
    public Canvas startScreenCanvas = null;
    public Canvas mainMenuCanvas = null;
    public Canvas VRMainCanvas = null;
    public Canvas MonMainCanvas = null;
    public Canvas QuestionnaireCanvas = null;
    public Canvas BreakCanvas = null;
    public Canvas FinalCanvas = null;
    public Canvas FinalQuestionnaire = null;

    public GameObject[] VRPracticeGOs = null;
    public GameObject[] MonPracticeGOs = null;
    public GameObject[] TutorialGOs = null;

    [Tooltip("GameObjects to set to inactive during the main experiment.")]
    public GameObject[] removeMain = null;
    [Tooltip("GameObjects to set to inactive during the survey.")]
    public GameObject[] removeSurvey = null;
    [Tooltip("GameObjects to set to inactive during the tutorial.")]
    public GameObject[] removeTutorial = null;
    [Tooltip("GameObjects to set to inactive only after connecting the to the robot each trial.")]
    public GameObject[] removeOnConnect = null;


    private Dictionary<string, Canvas> canvasDict = new Dictionary<string, Canvas>();
    private Dictionary<string, GameObject[]> GODict = new Dictionary<string, GameObject[]>();

    private void Start()
    {
        trialOrderManager = FindObjectOfType<TrialOrderManager>();
        if (!trialOrderManager) Debug.LogError("No TrialOrderManager found! ZOMFG");

        // Fill a dict containing all the canvasses, so we can easily switch between them
        canvasDict.Add(startScreenCanvas.gameObject.name, startScreenCanvas);
        canvasDict.Add(mainMenuCanvas.gameObject.name, mainMenuCanvas);
        canvasDict.Add(VRMainCanvas.gameObject.name, VRMainCanvas);
        canvasDict.Add(MonMainCanvas.gameObject.name, MonMainCanvas);
        canvasDict.Add(QuestionnaireCanvas.gameObject.name, QuestionnaireCanvas);
        canvasDict.Add(BreakCanvas.gameObject.name, BreakCanvas);
        canvasDict.Add(FinalCanvas.gameObject.name, FinalCanvas);
        canvasDict.Add(FinalQuestionnaire.gameObject.name, FinalQuestionnaire);

        // Fill dict containing all mode-specific GOs
        GODict.Add("VRPractice", VRPracticeGOs);
        GODict.Add("MonPractice", MonPracticeGOs);
        GODict.Add("Tutorial", TutorialGOs);
        GODict.Add("removeMain", removeMain);
        GODict.Add("removeSurvey", removeSurvey);
        GODict.Add("removeTutorial", removeTutorial);
    }

    public void startScreen()
    {
        setCanvas(startScreenCanvas);
        trialOrderManager.tutorialState = false;
    }

    public void startTutorial()
    {
        setCanvas(mainMenuCanvas);
        mainCanvasMan.lockMainMenu(false); // unlock the menu
        setTutorial(true);
        trialOrderManager.tutorialState = true;
    }


    public void startMain(bool newPlayerStart = false)
    {
        setTutorial(false);
        setCanvas(mainMenuCanvas);

        setAllEnabled(removeMain, false);

        if (newPlayerStart) // If the currScene trial order is set by a preset, but the playerinfo still needs to be collected (firstTrial=true in currState)
        {
            setAllEnabled(removeSurvey, false);
            mainCanvasMan.startNewPlayer();
        }
    }


    public void startVR(bool VRenabled, bool practice)
    {
        if (VRenabled)
            setCanvas(VRMainCanvas);
        else
            setCanvas(MonMainCanvas);

        setAllEnabled(removeOnConnect, false);

        setPractice(practice, VRenabled);
    }

    public void setGOs(bool state) // never really used this in the end
    {
        foreach (var GOs in GODict.Values)
        {
            setAllEnabled(GOs, state);
        }
    }

    public void setAllEnabled(GameObject[] GOs, bool enabled = true)
    {
        foreach(GameObject GO in GOs)
            GO.SetActive(enabled);
    }

    public void setCanvas(Canvas currCanvas)
    {
        //Debug.Log(currCanvas.name);
        foreach (var c in canvasDict.Values)
        {
            c.gameObject.SetActive(c == currCanvas);
        }

        setPractice(false);
    }

    private void setTutorial(bool enabled = true)
    {
        setAllEnabled(removeTutorial, !enabled);
        setAllEnabled(TutorialGOs, enabled);
    }

    private void setPractice(bool practice ,bool VR = false)
    {
        if (practice)
        {
            setAllEnabled(MonPracticeGOs, !VR);
            setAllEnabled(VRPracticeGOs, VR);
        }
        else
        {
            setAllEnabled(MonPracticeGOs, false);
            setAllEnabled(VRPracticeGOs, false);
        }

    }

    public void initStartCanvas() 
    {
        startMain();
        mainCanvasMan.fillDropDown();
        mainCanvasMan.setPresetMenu(trialOrderManager.conditionOrder[trialOrderManager.currConditionNo]);
        mainCanvasMan.lockMainMenu(true);
        mainCanvasMan.startInfoCanvas.gameObject.SetActive(true);

        setAllEnabled(removeOnConnect, true);
    }
}
