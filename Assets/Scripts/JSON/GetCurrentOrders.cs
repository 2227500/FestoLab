using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;


public class GetCurrentOrders : MonoBehaviour
{
    public List<string> GetOrderData = new List<string>();    

    public GetOrdersJSON[] getOrdersObjectArray;      

    public string listInfo;                                  

    public TMP_Text info;

    public string requestURL;

    public void ReceieveData(string CurrentOrderStringPHPMany)
    {
        string newCurrentOrderStringPHPMany = fixJson(CurrentOrderStringPHPMany);     

        Debug.LogWarning(newCurrentOrderStringPHPMany);                                    

        getOrdersObjectArray = JsonHelper.FromJson<GetOrdersJSON>(newCurrentOrderStringPHPMany);    

        GetOrderData.Clear();                                                
        listInfo = "";                                                              

        for (int i = 0; i < getOrdersObjectArray.Length; i++)                     
        {
            Debug.LogWarning("ONo:" + getOrdersObjectArray[i].PNo + ", Description:" + getOrdersObjectArray[i].Description);

            //CurrentOrderData.Add("Order Number: " + currentOrdersObjectArray[i].ONo + ", Company Name: " + currentOrdersObjectArray[i].Company + ", Planned Start Time: " + currentOrdersObjectArray[i].PlannedStart + ", Planned End Time: " + currentOrdersObjectArray[i].PlannedEnd + ", Build State: " + currentOrdersObjectArray[i].State);
            
            GetOrderData.Add("Get CurrentOrders" + "\n" + "CurrentOrder Number: " + getOrdersObjectArray[i].PNo + "Description: " + getOrdersObjectArray[i].Description);
        }

        foreach(var listMember in GetOrderData)                 
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
                    Debug.LogError("GetCurrent Orders Success");

                    break;
            }
        }
    }
}
