using UnityEngine;

// Manages what parts of the UI are presented at which times. Called on by SceneState.cs

public class UIManagerScript : MonoBehaviour
{
    public HapticPlugin haptics;
    public Canvas completeCanvas;
    public GameObject Panel;
    public bool logData;
    [ConditionalField(nameof(logData))] public GameObject logManager;

    public bool startWithRobotEnabled;

    public GameObject[] screens;
    public int startingScreenNumber = 0;
    private int currScreen;

    private void Start()
    {
        Panel.SetActive(true); // If the panel has been disabled (for example when it is annoying :P)

        if (startWithRobotEnabled)
            haptics.enableGrabbing = true;
        else
            haptics.enableGrabbing = false;

        currScreen = startingScreenNumber;

        if (currScreen >= screens.Length)
            FinalScreen();
        else if (currScreen >= 0)
        {
            completeCanvas.enabled = true;
            screens[currScreen].SetActive(true);

            for (int i = currScreen + 1; i < screens.Length; i++)
            {
                //Debug.Log("in for loop " + i.ToString());
                screens[i].SetActive(false);
            }
        }
        else // (if currscreen is below 0)
            FinalScreen();
    }

    public void FinalScreen()
    {
        screens[screens.Length-1].SetActive(false);
        EnableGrabbing();
        DisablePanel();

        if (logData)
            logManager.SetActive(true);
    }

    public void ProgressScreen()
    {
        if (currScreen+1 >= screens.Length)
        {
            FinalScreen();
            return;
        }

        screens[currScreen++].SetActive(false);
        screens[currScreen].SetActive(true);
    }

    public void EnableGrabbing()
    {

        haptics.enableGrabbing = true;
    }

    public void DisablePanel()
    {
        Panel.SetActive(false);
    }


    public void DisableCanvas()
    {
        completeCanvas.enabled = false;
    }
}
