using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkingScreen : MonoBehaviour
{
    public float blinkInterval = 0.5f;
    public float blinkDuration = 0.1f;

    private float timer;
    private bool isBlinking = false;

    // Start is called before the first frame update
    void Start()
    {
        if (!isBlinking)
        {
            StartCoroutine(Blink());
        }
    }

    private IEnumerator Blink()
    {
        isBlinking = true;

        while (true)
        {
            timer += Time.deltaTime;

            if (timer >= blinkInterval)
            {
                timer = 0f;

                GetComponent<Camera>().backgroundColor = Color.red;

                yield return new WaitForSeconds(blinkDuration);

                GetComponent<Camera>().backgroundColor = Color.black;
            }
        }
    }


}
