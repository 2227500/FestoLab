using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class CurrentOrders: MonoBehaviour
{
    public List<string> CurrentOrderData = new List<string>();    

    public CurrentOrderJSON[] currentOrdersObjectArray;      

    public string listInfo;                                  

    public TMP_Text info;

    public string requestURL;

    public void ReceieveData(string CurrentOrderStringPHPMany)
    {
        string newCurrentOrderStringPHPMany = fixJson(CurrentOrderStringPHPMany);     

        Debug.LogWarning(newCurrentOrderStringPHPMany);                                    

        currentOrdersObjectArray = JsonHelper.FromJson<CurrentOrderJSON>(newCurrentOrderStringPHPMany);    

        CurrentOrderData.Clear();                                                
        listInfo = "";                                                              

        for (int i = 0; i < currentOrdersObjectArray.Length; i++)                     
        {
            Debug.LogWarning("ONo:" + currentOrdersObjectArray[i].ONo + ", Company:" + currentOrdersObjectArray[i].Company + ", Planned Start:" + currentOrdersObjectArray[i].PlannedStart + ", Planned End:" + currentOrdersObjectArray[i].PlannedEnd + ", State:" + currentOrdersObjectArray[i].State);

            //CurrentOrderData.Add("Order Number: " + currentOrdersObjectArray[i].ONo + ", Company Name: " + currentOrdersObjectArray[i].Company + ", Planned Start Time: " + currentOrdersObjectArray[i].PlannedStart + ", Planned End Time: " + currentOrdersObjectArray[i].PlannedEnd + ", Build State: " + currentOrdersObjectArray[i].State);
            
            CurrentOrderData.Add("Current Orders" + "\n" + "Current Order Number: " + currentOrdersObjectArray[i].ONo + ", " + "Planned Start Time: " + currentOrdersObjectArray[i].PlannedStart + ", Planned End Time: " + currentOrdersObjectArray[i].PlannedEnd + ", Build State: " + currentOrdersObjectArray[i].State);
        }

        foreach(var listMember in CurrentOrderData)                 
        {
            listInfo += listMember.ToString() + "\n" + "\n";    
        }

        info.text = listInfo;                  
    }

    string fixJson(string value)
    {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    public void GetRequestPublic()
    {
        StartCoroutine(GetRequest(requestURL));     //calls coroutine and sets string
    }

    IEnumerator GetRequest(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = url.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    ReceieveData(webRequest.downloadHandler.text);
                    Debug.LogError("Current Orders Success");

                    break;
            }
        }
    }
}
