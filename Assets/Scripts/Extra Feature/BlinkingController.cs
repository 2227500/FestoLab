using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingController : MonoBehaviour
{

    [SerializeField] private Camera EmergencyBlink;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        EmergencyBlink.backgroundColor = Color.Lerp(Color.red, Color.black, Mathf.PingPong(Time.time, 1));
    }
}
