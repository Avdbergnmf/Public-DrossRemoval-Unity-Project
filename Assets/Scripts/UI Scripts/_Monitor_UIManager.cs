using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class _Monitor_UIManager : MonoBehaviour
{
    //public RawImage KOImage = null;
    [Header("Parent GOs")]
    public TrialOrderManager trialOrderManager = null;
    public CollisionLoggingManager CLM = null;
    public SceneState sceneState = null;
    float lastTimeStamp = 0f;

    [Header("UI GOs")]
    public Text trialText = null;
    public Text timeText = null;
    public Slider progressSlider = null;
    public ObiActorLogger obiActorLogger = null;

    public Color[] progressColorGradientColors;
    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;

    // private
    Image progressImage;
    float timeNow;

    private void Start()
    {
        makeProgressBar();
        //KOImage.canvasRenderer.SetAlpha(0.0f);
    }

    private void OnEnable()
    {
        sceneState.failedGoal = false;
        sceneState.reachedGoal = false;
        sceneState.removedProgress = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if any violations have occured!
        if (trialOrderManager.checkViolations)
        {
            if (CLM.violatedForbidden)
            {
                //KOImage.canvasRenderer.SetAlpha(1.0f);
                //KOImage.CrossFadeAlpha(0.0f, 1.5f, false);
                sceneState.failedGoal = true;
                sceneState.startNextTrial(); // Check if tutorial is running
            }

            if (CLM.violatedForce)
            {
                //KOImage.canvasRenderer.SetAlpha(1.0f);
                //KOImage.CrossFadeAlpha(0.0f, 1.5f, false);
                sceneState.failedGoal = true;
                sceneState.startNextTrial();
            }
        }

        trialText.text = "Current Repetition: " + (trialOrderManager.currRep+1).ToString() + "/" + trialOrderManager.numberOfRepetitions; // 

        timeNow = Mathf.Round(100 * (lastTimeStamp + Time.time - sceneState.startTime)) / 100;

        timeText.text = "Time: " + timeNow.ToString();

        //checkRemovalProgress();
        checkTimeProgress();
    }

    public void RestartTrial()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        sceneState.initScene(); 

        lastTimeStamp = 0f;
        sceneState.startTime = Time.time;

        //sceneState.CM.initStartCanvas();
    }

    #region Progress
    public void makeProgressBar()
    {
        var progressImages = progressSlider.GetComponentsInChildren<Image>();
        progressImage = progressImages[progressImages.Length - 1];


        // Set up the gradient
        gradient = new Gradient();

        int lenColor = progressColorGradientColors.Length;
        colorKey = new GradientColorKey[lenColor];
        for (int i = 0; i < lenColor; i++)
        {
            colorKey[i].color = progressColorGradientColors[i];
            colorKey[i].time = (float)((float)i / ((float)lenColor - 1.0f));
        }

        alphaKey = new GradientAlphaKey[1];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
    }

    public float checkTimeProgress()
    {
        float progress = timeNow / trialOrderManager.secPerTrial;

        setProgressBar(progress);

        if (progress >= 1)
        {
            sceneState.reachedGoal = true;

            sceneState.removedProgress = checkRemovalProgress();

            sceneState.startNextTrial();
        }

        return progress;
    }


    public float checkRemovalProgress()
    {
        float progress = obiActorLogger.fraction;
        return progress;
    }

    public void setProgressBar(float progress)
    {
        float sliderValue = 1 - progress;
        progressSlider.value = sliderValue;
        progressImage.color = gradient.Evaluate(sliderValue);
    }

    #endregion

    /*public void PauseGame() // this is old code, but kept here as it might still be useful later on
    {
        Time.timeScale = 0;
        pauseButton.SetActive(false);
        continueButton.SetActive(true);
        lastTimeStamp = Time.time - sceneState.startTime;
    }

    public void ContinueGame()
    {
        Time.timeScale = 1;
        pauseButton.SetActive(true);
        continueButton.SetActive(false);
        //enable the scripts again
        sceneState.startTime = Time.time;
    }*/
}
