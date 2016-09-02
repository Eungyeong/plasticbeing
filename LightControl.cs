using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Script to control the lighting in the scene
/// </summary>
public class LightControl : MonoBehaviour {

    //public float duration = 2.0F;//blinking

    public Light spotlight; // get the spotlight
    public Light pointlight; // get the Point light
    public LensFlare flarelight; 

    // Timer for the lights
    public float timer;
    private Text textTimer;
    public float seconds, minutes;

    public float smoothing;
    private float newIntensity;
    private bool isOnPLight = false;
    private bool isDimOn = false;


    // light on and off test
    
    void Awake() 
    {
        //newIntensity = spotlight.intensity;
        //pointlight.intensity = 20f;

        flarelight.brightness = 40f;
    }

    // Use this for initialization
    void Start () {

        //LIGHTING
        StartCoroutine(pointlightOn());
        // pointlight = GetComponent<Light>();

        // Timer
        textTimer = GetComponent<Text>();
        
    }


    // Begin the timer countdown and when user time is up turn off the lights
    void Update()
    {
        // Timer 
        timer = Time.time;
        minutes = (int)((120 - Time.time) / 60f);
        seconds = (int)(60-Time.time % 60f);
        textTimer.text = minutes.ToString("00") + ":" + seconds.ToString("00");

        //print(timer);
        /*
        if (timer > 0 && timer < 7f && isOnPLight == false)
        {
            isOnPLight = true;
            StartCoroutine(pointlightOn());
        }*/
        if (timer >= 6.0f && timer < 30.0f && isDimOn == false)
          {
            isDimOn = true;
            StartCoroutine(lightDimOn());
          }
        else if (timer >= 120.0f)
          {
            StartCoroutine(lightDimOff());
        }

    }


    // At the beginning, point light lights up.
    IEnumerator pointlightOn()
    {
        /*
        //LIGHTING INTENSITY blinking
        float phi = Time.time / duration * 1 * Mathf.PI;//how many times the light blinks per second
        float amplitude = Mathf.Cos(phi) * 8F + 0.5F;//intensity differnence
        pointlight.intensity = amplitude;
        */

        while (timer < 12f) { 
            pointlight.intensity = Mathf.Lerp(pointlight.intensity, 0.7f, timer * Time.deltaTime * 0.8f);
            flarelight.brightness = Mathf.Lerp(flarelight.brightness, 0f, timer * Time.deltaTime * 0.8f);
            yield return new WaitForSeconds(0.02f);
        }
    }

    // spot light goes up
    IEnumerator lightDimOn()
    {
        newIntensity = 1.8f;
        //spotlight.intensity = Mathf.Lerp(spotlight.intensity, newIntensity, Time.deltaTime); //fade increase
        spotlight.intensity = newIntensity; //non fade light on
        yield return null;
    }

    // lights go off
    IEnumerator lightDimOff()
    {
        spotlight.intensity = Mathf.Lerp(spotlight.intensity, 0f, Time.deltaTime);
        pointlight.intensity = Mathf.Lerp(pointlight.intensity, 0f, Time.deltaTime * smoothing);
        yield return null;
    }
    
}

