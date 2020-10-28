using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _VRExtra_UIManager : MonoBehaviour
{
    public GameObject breakCanvas = null;
    public GameObject stoppingPanel = null;
    public GameObject questionnaireCanvas = null;

    public GameObject startInfoCanvas = null;
    public GameObject startingPanel = null;

    void Update()
    {// Lazy programming but whatevs
        // Stopping (take off VR)
        if ((breakCanvas.activeInHierarchy || questionnaireCanvas.activeInHierarchy) && !stoppingPanel.activeSelf)
            stoppingPanel.SetActive(true);
        else if (!(breakCanvas.activeInHierarchy || questionnaireCanvas.activeInHierarchy) && stoppingPanel.activeSelf)
            stoppingPanel.SetActive(false);
        // Starting (connect omni to start)
        if (startInfoCanvas.activeInHierarchy && !startingPanel.activeSelf)
            startingPanel.SetActive(true);
        else if (!startInfoCanvas.activeInHierarchy && startingPanel.activeSelf)
            startingPanel.SetActive(false);
    }
}
