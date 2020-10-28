using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartSurveyScript : MonoBehaviour
{
    //public Canvas playerInfoCanvas = null;
    [SerializeField] public string reportFileName = "PlayerInfo";
    [SerializeField] public string reportDirectoryName = "Report";

    public InputField nameInput;
    public InputField ageInput;
    public ToggleGroup genderToggleGroup;
    public Dropdown gamingExperienceDropDown;
    public Dropdown teleopExperienceDropDown;

    public PlayerInfo playerInfo;

    private string[] reportHeaders = new string[6] {
            "Name",
            "Age",
            "Gender",
            "PriorTeleopExperience",
            "PriorGamingExperience",
            "Condition Order"
    };

    // Start is called before the first frame update
    private void Start()
    {
        playerInfo = new PlayerInfo();
    }

    public void SubmitPlayerInfo() // Saves the playerinfo to the PlayerInfo object in this class (so that it can be used by other scripts), and logs it to a CSV
    {
        playerInfo.name = nameInput.text;
        playerInfo.age = int.Parse(ageInput.text);

        for (int i = 0; i < genderToggleGroup.transform.childCount; i++)
        {
            if (genderToggleGroup.transform.GetChild(i).GetComponent<Toggle>().isOn)
            {
                playerInfo.gender = genderToggleGroup.transform.GetChild(i).Find("Label").GetComponent<Text>().text;
                break;
            }
        }

        playerInfo.PriorTeleopExperience = teleopExperienceDropDown.options[teleopExperienceDropDown.value].text; // <<<<<<<< might do a little mathematics here to get the actual time in hrs based on the slider idunno we'll see
        playerInfo.PriorGamingExperience = gamingExperienceDropDown.options[gamingExperienceDropDown.value].text;

        // Write this to a CSV file
        LogToCSV logger = new LogToCSV(reportDirectoryName, reportHeaders, 0, false); // make a logger with 1 entry
        logger.CreateFileName(reportFileName);

        string conditionOrder = "";

        TrialOrderManager trialOrderManager = FindObjectOfType<TrialOrderManager>();

        foreach (var c in trialOrderManager.conditionOrder)
            conditionOrder += c.ToString();

        string[] strings = new string[6] { // Put the playerinfo in a list of strings
            playerInfo.name,
            playerInfo.age.ToString(),
            playerInfo.gender.ToString(),
            playerInfo.PriorTeleopExperience,
            playerInfo.PriorGamingExperience,
            conditionOrder
        };

        logger.CreateSingleEntry(strings, playerInfo.name); // write to the logger
    }
}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int age;
    public string gender;
    public string PriorTeleopExperience;
    public string PriorGamingExperience;

    public PlayerInfo() // so that it starts out with some values
    {
        name = "Player";
        age = 20;
        gender = "male";
        PriorTeleopExperience = "None";
        PriorGamingExperience = "None";
    }
}
