using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerController : MonoBehaviour
{
    [SerializeField] private GameObject BlinkingScreenPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(BlinkingScreenPrefab, transform);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("BLINKK!!!!!!!111");
    }
}
