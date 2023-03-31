using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    public GameObject camera1;
    public GameObject arcamera;
    public bool armode; public void modeSwitch()
    {
        
        if (armode == true)
        {
            Debug.Log("Switch Camera");
            camera1.SetActive(false);
            arcamera.SetActive(true);
            armode = false;
        }
        else
        {
            arcamera.SetActive(false);
            camera1.SetActive(true);
            armode = true;
        }
    }
}
