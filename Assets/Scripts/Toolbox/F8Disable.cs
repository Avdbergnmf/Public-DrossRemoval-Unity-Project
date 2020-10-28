using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F8Disable : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            gameObject.SetActive(false);
        }
    }
}
