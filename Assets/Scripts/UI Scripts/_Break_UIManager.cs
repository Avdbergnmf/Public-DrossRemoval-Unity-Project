using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class _Break_UIManager : MonoBehaviour
{
    [Header("Other GOs")]
    public TrialOrderManager trialOrderManager;
    public SceneState sceneState;

    [Header("Progress Elements")]
    public Text EvaluationText = null;
    public Text conditionProgressText = null;
    public Text repetitionProgressText = null;
    public Button continueButton = null;

    public Slider progressSlider = null;
    public Color[] progressColorGradientColors;
    Gradient gradient;
    GradientColorKey[] colorKey;
    GradientAlphaKey[] alphaKey;
    Image progressImage;



    public Text waitText = null;
    private float waitedTime = 0.0f;
    public void OnEnable()
    {
        makeProgressBar();

        var condDone = trialOrderManager.currConditionNo;

        conditionProgressText.text = "You have currently completed " + condDone.ToString() + "/" + trialOrderManager.conditionOrder.Length + " experimental conditions";
        repetitionProgressText.text = "And you have completed " + trialOrderManager.currRep.ToString() + "/" + trialOrderManager.numberOfRepetitions + " repetitions for this condition";

        int repProg = trialOrderManager.currRep + condDone * trialOrderManager.numberOfRepetitions;
        int totReps = trialOrderManager.numberOfRepetitions * trialOrderManager.conditionOrder.Length;
        float progress = (float)repProg / totReps;

        setProgressBar(progress);

        waitedTime = 0.0f;

        // Evaluation:
        if (sceneState.failedGoal)
            EvaluationText.text = "You failed to complete the trial due to a collision with the environment";
        else
            EvaluationText.text = "You completed the trial with " + Math.Round((1-sceneState.removedProgress)*100,3).ToString() + "% of the dross removed";
        
    }

    public void Update()
    {
        if (waitedTime > trialOrderManager.breakTimePerTrial)
            continueButton.interactable = true;
        else
        {
            waitedTime += Time.deltaTime;
            waitText.text = "Please take a quick break for the next: " + Mathf.Round(trialOrderManager.breakTimePerTrial - waitedTime).ToString() + " seconds";
        }

    }

    #region Progress
    public void setProgressBar(float progress)
    {
        float sliderValue = progress;
        //Debug.Log(sliderValue);
        progressSlider.value = sliderValue;
        progressImage.color = gradient.Evaluate(sliderValue);
    }

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
    #endregion
}
