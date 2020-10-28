using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// There was a bug in which the omni cue would not automatically turn off, this is a quick and dirty fix for that.

public class OmniCueQuickFix : MonoBehaviour
{
    public Canvas omniCueCanvas = null;

    private void OnEnable()
    {
        CurrState currState = FindObjectOfType<CurrState>();

        if (currState && omniCueCanvas)
        {
            if (!currState.omni) // should the omni cue be enabled?
                omniCueCanvas.gameObject.SetActive(false);
        }
        else
            Debug.LogError("Currstate not found, not sure if omnicue is correctly en/dis-abled");
    }
}
