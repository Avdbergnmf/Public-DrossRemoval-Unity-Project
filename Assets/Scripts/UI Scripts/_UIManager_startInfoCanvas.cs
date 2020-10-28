using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _UIManager_startInfoCanvas : MonoBehaviour
{
    public SceneState SS = null;
    public HapticPlugin HP = null;
    public GameObject practiceInfo = null;

    private void OnEnable()
    {
        Debug.Log(SS.trialOrderManager.practiceState);
        if (SS.trialOrderManager.practiceState)
            practiceInfo.SetActive(true);
        else
            practiceInfo.SetActive(false);
    }

    private void Update()
    { 
        if (HP.grabbing)
        {
            SS.startTrial();
        }
    }
}
