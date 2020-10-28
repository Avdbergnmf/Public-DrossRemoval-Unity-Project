using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;
using UnityEditor;

public class CameraPositions : MonoBehaviour
{
    public bool linkToVR = false;

    public Camera VRCamera = null;

    public string presetFolder = "Assets/Settings/CameraPositions";

    public string saveName = "default";

    public bool setZRotToZero = true;

    public bool countUp = true;
    int count = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (linkToVR)
        {
            gameObject.transform.position = VRCamera.transform.position;
            gameObject.transform.rotation = VRCamera.transform.rotation;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (countUp)
            {
                saveName += count.ToString();
                count++;
            }

            if (setZRotToZero)
            {
                Vector3 correctZRot = gameObject.transform.rotation.eulerAngles;

                gameObject.transform.rotation = Quaternion.Euler(correctZRot.x, correctZRot.y, 0f);
            }

            CreatePresetAsset(gameObject.GetComponent<Transform>(), saveName, presetFolder);
            Debug.Log("Saved Position!");
        }
        
    }

    // This method creates a Preset from a given Object and save it as an asset in the project.
    public void CreatePresetAsset(Object source, string name, string folder)
    {
        Preset preset = new Preset(source);
        AssetDatabase.CreateAsset(preset, folder + '/' + name + ".preset");
    }
}
