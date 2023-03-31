using UnityEngine;
using game4automation;
using TMPro;



public class ExampleFunctions : MonoBehaviour
{
    [Header("Factory Interfaces")]
    public OPCUA_Interface[] opcuaInterfaces;
    public TMP_Text feedbackText;

    //runs through all of the factory interfaces checking for connection
    public void CheckConnectionWithMachines()
    {
        for (int i = 0; i < opcuaInterfaces.Length; i++)             
        {
            if (opcuaInterfaces[i].IsConnected)               
            {
                feedbackText.text = feedbackText.text + "Good connection to interface " + i + ".";
                Debug.Log("Good connection to interface " + i + ".");
            }
            else                                     
            {
                feedbackText.text = feedbackText.text + "Good connection to interface " + i + ".";
                Debug.Log("No connection to interface " + i + ".");
            }
        }
    }
}
