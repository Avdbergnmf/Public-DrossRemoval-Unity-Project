using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BasicSurveyScript : MonoBehaviour
{
    public GameObject[] questionGroupArr;
    public QAClass[] qaClassArr;
    public GameObject AnswerPanel;


    // Start is called before the first frame update
    void Start()
    {
        qaClassArr = new QAClass[questionGroupArr.Length];
    }

    public void SubmitAnswer()
    {
        for (int i = 0; i < qaClassArr.Length; i++)
        {
            qaClassArr[i] = ReadQAs(questionGroupArr[i]);
        }
    }

    QAClass ReadQAs(GameObject questionGroup)
    {
        QAClass result = new QAClass();

        GameObject q = questionGroup.transform.Find("Question").gameObject;
        GameObject a = questionGroup.transform.Find("Answer").gameObject;

        result.question = q.GetComponent<Text>().text;

        if (a.GetComponent<ToggleGroup>() != null)
        {
            for (int i = 0; i < a.transform.childCount; i++)
            {
                if (a.transform.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    result.answer = a.transform.GetChild(i).
                        Find("Label").GetComponent<Text>().text;
                    break;
                }
            }
        }
        else if (a.GetComponent<InputField>() != null)
        {
            result.answer = a.transform.Find("Text").GetComponent<Text>().text;
        }
        else if (a.GetComponent<ToggleGroup>() == null && a.GetComponent<InputField>() == null)
        {
            string s = "";
            int counter = 0;

            for (int i = 0; i < a.transform.childCount; i++)
            {
                if (a.transform.GetChild(i).GetComponent<Toggle>().isOn)
                {
                    if (counter != 0)
                    {
                        s += ", ";
                    }
                    s += a.transform.GetChild(i).Find("Label").GetComponent<Text>().text;
                    counter++;
                }

                if (i == a.transform.childCount - 1)
                {
                    s += ".";
                }
            }
            result.answer = s;
        }

        return result;

    }

}

[System.Serializable]
public class QAClass
{
    //public string playerName;

    public string question;
    public string answer;

    //public int age;
    //public string gender;
}

