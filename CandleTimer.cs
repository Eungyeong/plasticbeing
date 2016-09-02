using UnityEngine;
using System.Collections;

/// <summary>
/// Class to controll a candle used as a timer to signal how much time the user has left before their session ends
/// </summary>

public class CandleTimer : MonoBehaviour
{
    public Transform Candle; // get candle object
    public float time = 120;
    float startTime;
    bool hasStarted = false;

	void Start () // start timer
    {
        startTime = time;   
        StartCoroutine(CTimer());
	}


    IEnumerator CTimer()
    {      

        while (time > 0)
        {
            time--;            
            yield return new WaitForSeconds(1); // reduce time by one every whole second
        }
    }

    void Update()
    {
        if(hasStarted == false)
        {
            LeanTween.moveY(Candle.gameObject, 0f, time); // use tweener to smoothly move candle downwards
            hasStarted = true;
        }
    }
}
