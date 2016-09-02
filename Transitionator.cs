using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the distortion effect on the avatar and also the general transitioning from one mesh to another mesh
/// </summary>

public class Transitionator : MonoBehaviour
{

    public Transform HeadBone, LArmBone, RArmBone, LLegBone, RLegBone; // get the bones from the characters skeleton

    // containers for finding and assigning the meshes automatically
    public List<Transform> _BaseMeshes, _LeftArms, _RightArms, _LeftLegs, _RightLegs, _Heads, _Bodies; // references to the mesh body parts
    public List<Renderer> _LeftArmsRenderer, _RightArmsRenderer, _LeftLegsRenderer, _RightLegsRenderer, _HeadsRenderer, _BodiesRenderer; // shader refs

    // used to control body part transitions
    public Transform CurrentH, CurrentLA, CurrentRA, CurrentLL, CurrentRL, CurrentBody;
    public Renderer CurrentRendH, CurrentRendLA, CurrentRendRA, CurrentRendLL, CurrentRendRL, CurrentRendBody;
    public Renderer NewRendH, NewRendLA, NewRendRA, NewRendLL, NewRendRL, NewRendBody;

    public string noiseControlName = "_amount"; // shaders distortion control reference
    public float changeSpeed = 0.01f; // speed of transition change
    public float noiseMod = 0.04f; 
    public float noiseMax = 0.15f; // maximum distortion before transition to next body part is triggered
    public float noiseThreshold = 0.1f;
    public float afterWobble = 0.001f;
    public Vector3 scaleSpeed = new Vector3(0.1f, 0.1f, 0.1f); // add some scaling to make transition nicer

    private Transform CurrentBone; // used for current bone checking

    public Vector3 previousH, previousRA, previousLA, previousRL, previousLL; // get speed of bones calculations
    public float speed, speedH, speedLA, speedRA, speedLL, speedRL;
    public float old_speedH, old_speedLA, old_speedRA, old_speedLL, old_speedRL;
    public float speedChangePoint = 2f; // speed at which transition to new mesh is triggered
    public float speedMinPoint = 1f;
    public int RNG = 0; // pick a new body part randomly

    //states for readiness of various effects
    bool isReady, isReadyH, isReadyLA, isReadyRA, isReadyLL, isReadyRL;
    bool isReadyWobble = false;
    bool isWobblingH, isWobblingLA, isWobblingRA, isWobblingLL, isWobblingRL;

    public Text EffectsCountDown, speedLAtext, percentChangeText, counterText;

    //sfx array
    public List<AudioSource> audioArray;


    void Start ()
    {
        // at start, get all necessary meshes and material renderers into these arrays
        foreach (Transform baseMesh in _BaseMeshes)
        {
            for (int j = 0; j < baseMesh.childCount; j++)
            {
                Transform newchild = baseMesh.GetChild(j);
                if (newchild.name.Contains("Head"))
                {
                    _Heads.Add(newchild);
                    _HeadsRenderer.Add(newchild.GetComponent<Renderer>());
                }
                if (newchild.name.Contains("Body"))
                {
                    _Bodies.Add(newchild);
                    _BodiesRenderer.Add(newchild.GetComponent<Renderer>());
                }
                if (newchild.name.Contains("LeftArm"))
                {
                    _LeftArms.Add(newchild);
                    _LeftArmsRenderer.Add(newchild.GetComponent<Renderer>());
                }
                if (newchild.name.Contains("RightArm"))
                {
                    _RightArms.Add(newchild);
                    _RightArmsRenderer.Add(newchild.GetComponent<Renderer>());
                }
                if (newchild.name.Contains("LeftLeg"))
                {
                    _LeftLegs.Add(newchild);
                    _LeftLegsRenderer.Add(newchild.GetComponent<Renderer>());
                }
                if (newchild.name.Contains("RightLeg"))
                {
                    _RightLegs.Add(newchild);
                    _RightLegsRenderer.Add(newchild.GetComponent<Renderer>());
                }
            }
        }
        int i = 0;
        foreach(Transform head in _Heads){
            if (!head.name.Contains("Hum"))
            {
                head.gameObject.SetActive(false);
            }
            else {
                CurrentH = head;
                CurrentRendH = _HeadsRenderer[i];
            }
            i++;
        }
        i = 0;
        foreach (Transform body in _Bodies){
            if (!body.name.Contains("Hum"))
            {
                body.gameObject.SetActive(false);
            }
            else {
                CurrentBody = body;
                CurrentRendBody = _BodiesRenderer[i];
            }
            i++;
        }
        i = 0;
        foreach (Transform la in _LeftArms){
            if (!la.name.Contains("Hum"))
            {
                la.gameObject.SetActive(false);
            }
            else {
                CurrentLA = la;
                CurrentRendLA = _LeftArmsRenderer[i];
            }
            i++;
        }
        i = 0;
        foreach (Transform ra in _RightArms){
            if (!ra.name.Contains("Hum"))
            {
                ra.gameObject.SetActive(false);
            }
            else {
                CurrentRA = ra;
                CurrentRendRA = _RightArmsRenderer[i];
            }
            i++;
        }
        i = 0;
        foreach (Transform ll in _LeftLegs){
            if (!ll.name.Contains("Hum"))
            {
                ll.gameObject.SetActive(false);
            }
            else {
                CurrentLL = ll;
                CurrentRendLL = _LeftLegsRenderer[i];
            }
            i++;
        }
        i = 0;
        foreach (Transform rl in _RightLegs){
            if (!rl.name.Contains("Hum"))
            {
                rl.gameObject.SetActive(false);
            }
            else {
                CurrentRL = rl;
                CurrentRendRL = _RightLegsRenderer[i];
            }
            i++;
        }
        i = 0;

        // set states
        AvgSpeedH = AvgSpeedLA = AvgSpeedRA = AvgSpeedLL = AvgSpeedRL = new List<float>();
        AvgSpeeds.Add(AvgSpeedH); AvgSpeeds.Add(AvgSpeedLA); AvgSpeeds.Add(AvgSpeedRA); AvgSpeeds.Add(AvgSpeedLL); AvgSpeeds.Add(AvgSpeedRL);

        old_speedH = old_speedLA = old_speedRA = old_speedLL = old_speedRL = 0;

        isReady = isReadyH = isReadyLA = isReadyRA = isReadyLL = isReadyRL = false;

        isWobblingH = isWobblingLA = isWobblingRA = isWobblingLL = isWobblingRL = false; // Only want to test this one for now.               

        audioArray.AddRange( transform.FindChild("_SoundFiles").GetComponents<AudioSource>()); // get audio effect files

        StartCoroutine(StartCountdown(5));
        StartCoroutine(RNG_Timer());
    }


    IEnumerator StartCountdown(int time) // countdown for getting in position to test with the kinect
    {
        while (time > 0)
        {
            time--;
            EffectsCountDown.text = time.ToString();
            yield return new WaitForSeconds(1);
        }
        EffectsCountDown.gameObject.SetActive(false);
        isReady = isReadyH = isReadyLA = isReadyRA = isReadyLL = isReadyRL = true;
        isReadyWobble = true;

    }


    IEnumerator RNG_Timer () // random number generator for which mesh is new mesh
    {
        while (true)
        {
            RNG = Random.Range(0, _BaseMeshes.Count);
            yield return new WaitForSeconds(0.25f);
        }
    }


    void Update() // check the speed of the bones each frame
    {
        previousH = SpeedCheck(HeadBone, previousH); // ref _doFiveTimes, ref AvgSpeeds);
        previousLA = SpeedCheck(LArmBone, previousLA); //ref _doFiveTimes, ref AvgSpeeds);
        previousRA = SpeedCheck(RArmBone, previousRA); //ref _doFiveTimes, ref AvgSpeeds);
        previousLL = SpeedCheck(LLegBone, previousLL); //ref _doFiveTimes, ref AvgSpeeds);
        previousRL = SpeedCheck(RLegBone, previousRL); //ref _doFiveTimes, ref AvgSpeeds);                
    }


    // Experimental section to get a smoothed median of a set of speeds due to irregular readings
    //float currentAvgSpeed = 0;
    // float newspeed

    List<float> AvgSpeedH, AvgSpeedLA, AvgSpeedRA, AvgSpeedLL, AvgSpeedRL;
    List<List< float >> AvgSpeeds = new List<List<float>>();
    
    int[] _doFiveTimes = new int[5] {0,0,0,0,0};

    float UpdateCumulativeMovingAverageSpeed(float theNewSpeed, int qty, ref List<List <float>> currentAvgSpeed, int part)
    {
        

        //++qty;
        //currentAvgSpeed += (theNewSpeed - currentAvgSpeed) / qty;

        currentAvgSpeed[part].Add(theNewSpeed);

        if (qty <= 5)
            return 0;

        else
        {
            qty = 0; // reset after 5

            currentAvgSpeed[part].Sort();

            float medianSpeed = currentAvgSpeed[part][2];
            currentAvgSpeed[part].Clear();
            return medianSpeed;
        }        
    }

    enum part { h = 0, la = 1, ra = 2, ll = 3, rl = 4 }


    // Function to check the speed for each part and then begin either a wobbling effect or
    // the transition to a different body part
    /*
    Vector3 SpeedCheck(Transform MovingPoint, Vector3 previous, ref int[] _doFiveTimes, ref List<List<float>> _AvgSpeed)
    {
             

        int thePart = 0;
        CurrentBone = MovingPoint;

        if (CurrentBone.name == HeadBone.name) thePart = (int) part.h;
        else if (CurrentBone.name == LArmBone.name) thePart = (int)part.la;
        else if (CurrentBone.name == RArmBone.name) thePart = (int)part.ra;
        else if (CurrentBone.name == LLegBone.name) thePart = (int)part.ll;
        else if (CurrentBone.name == RLegBone.name) thePart = (int)part.rl;

        if (_doFiveTimes[thePart] <= 5)
            {
                float newspeed = ((MovingPoint.position - previous).magnitude);
                speed = UpdateCumulativeMovingAverageSpeed(newspeed, _doFiveTimes[thePart], ref _AvgSpeed, thePart) / Time.deltaTime * 10f;

                _doFiveTimes[thePart]++;
                return MovingPoint.position;
            }
        else
            _doFiveTimes[thePart] = 0; */

    Vector3 SpeedCheck(Transform MovingPoint, Vector3 previous)
    { 
        speed = ((MovingPoint.position - previous).magnitude) / Time.deltaTime;   

        // check if speed is at the wobble threshold
        if (speed > speedMinPoint && speed < speedChangePoint && isReadyWobble == true)
        {
            CurrentBone = MovingPoint;

            if (CurrentBone.name == HeadBone.name)
            {
                speedH = speed;
                if (isWobblingH == false)
                {
                    isWobblingH = true;
                    StartCoroutine(WobblerH()); 
                }
            }
            else if (CurrentBone.name == LArmBone.name)
            {
                speedLA = speed;
                if (isWobblingLA == false)
                {
                    isWobblingLA = true;
                    StartCoroutine(WobblerLA()); 
                }
            }
            else if (CurrentBone.name == RArmBone.name)
            {
                speedRA = speed;
                if (isWobblingRA == false)
                {
                    isWobblingRA = true;
                    StartCoroutine(WobblerRA()); 
                }
            }
            else if (CurrentBone.name == LLegBone.name)
            {
                speedLL = speed;
                if (isWobblingLL == false)
                {
                    isWobblingLL = true;
                    StartCoroutine(WobblerLL()); 
                }
            }
            else if (CurrentBone.name == RLegBone.name)
            {
                speedRL = speed;
                if (isWobblingRL == false)
                {
                    isWobblingRL = true;
                    StartCoroutine(WobblerRL()); 
                }
            } 
        }
        else
        {
            speedH = speedLA = speedRA = speedLL = speedRL = speed;
        }

        // check if speed is a the transition threshold
        if (speed > speedChangePoint && isReady == true)
        {          
            if (CurrentBone.name == HeadBone.name && isReadyH == true)
            {
                StartCoroutine(Transition(MovingPoint));
            }
            else if (CurrentBone.name == LArmBone.name && isReadyLA == true)
            {
                StartCoroutine(Transition(MovingPoint));
            }
            else if (CurrentBone.name == RArmBone.name && isReadyRA == true)
            {
                StartCoroutine(Transition(MovingPoint));
            }
            else if (CurrentBone.name == LLegBone.name && isReadyLL == true)
            {
                StartCoroutine(Transition(MovingPoint));
            }
            else if (CurrentBone.name == RLegBone.name && isReadyRL == true)
            {
                StartCoroutine(Transition(MovingPoint));
            }
        }

        return MovingPoint.position;
    }

    // play sfx and fade volume down
    IEnumerator PlaySFX(AudioSource playingSound, float vol)
    {
        playingSound.volume = vol;

        while (playingSound.volume > 0)
        {
            playingSound.volume -= 0.01f;
            yield return null;
        }

        playingSound.Stop();
    }

    // functions to create a wobble effect for the body part mentioned based on speed of movement
    IEnumerator WobblerH()
    {
        while (speedH < speedChangePoint && speedH > speedMinPoint) // wobble if the speed is between a certain amount
        {
            float percent = Mathf.Lerp(speedH, speedChangePoint, speedH / speedChangePoint); // amount between current and max numbers
            float modifier = (percent - speedMinPoint) / (speedChangePoint - speedMinPoint); // (0 - 1) normalizing formula

            if(CurrentH.gameObject.activeSelf == true)
            {          

                CurrentRendH.material.SetFloat(noiseControlName, modifier * noiseMod); // apply wobble
                CurrentRendBody.material.SetFloat(noiseControlName, modifier * noiseMod);

                if (audioArray[0].isPlaying == false) audioArray[1].Play(); // play audio
                audioArray[0].volume = modifier;
            }
            
            yield return null;
        }

        StartCoroutine(PlaySFX(audioArray[0], audioArray[0].volume)); // finish playing sfx

        // After Wobble

        if (isReadyH != false) // fade down wobble effect
        {
            for (float i = CurrentRendH.material.GetFloat(noiseControlName); i > 0; i -= afterWobble)
            {
                CurrentRendH.material.SetFloat(noiseControlName, i);
                CurrentRendBody.material.SetFloat(noiseControlName, i);
                yield return null;
            } 
        }

        CurrentRendH.material.SetFloat(noiseControlName, 0f);
        CurrentRendBody.material.SetFloat(noiseControlName, 0f);
        
        isWobblingH = false;
    }
    IEnumerator WobblerLA()
    {
        //float newNum = 0;
        //float aNum;
        
        while (speedLA < speedChangePoint && speedLA > speedMinPoint)
        {
            float percent = Mathf.Lerp( speedLA, speedChangePoint, speedLA/speedChangePoint);
            float modifier = (percent - speedMinPoint) / (speedChangePoint - speedMinPoint);

            float mod2 = (speedLA - speedMinPoint) / (speedChangePoint - speedMinPoint);

            //aNum =  Mathf.SmoothDamp(speedLA, speedChangePoint, ref newNum, 0.1f);

            //float oldval = CurrentRendLA.material.GetFloat(noiseControlName);

            if (CurrentLA.gameObject.activeSelf == true)
            {
                //LeanTween.value(this.gameObject, oldval, mod2 * 0.3f, 0.1f).setOnUpdate(
                //    (x) => { CurrentRendLA.material.SetFloat(noiseControlName, x); }).setEase(LeanTweenType.easeInOutSine);

                //CurrentRendLA.material.SetFloat(noiseControlName, modifier * noiseMod);

                //CurrentRendLA.material.SetFloat(noiseControlName, mod2 * 0.2f);
                float oldval = CurrentRendLA.material.GetFloat(noiseControlName);

                if (speedLA > old_speedLA)
                    CurrentRendLA.material.SetFloat(noiseControlName, oldval += 0.005f);
                else if (speedLA < old_speedLA)
                        CurrentRendLA.material.SetFloat(noiseControlName, oldval -= 0.005f);
                else
                    CurrentRendLA.material.SetFloat(noiseControlName, oldval);

                if (audioArray[1].isPlaying == false) audioArray[1].Play();
                audioArray[1].volume = modifier;
            }

            old_speedLA = speedLA;

            yield return null; // new WaitForSeconds(0.1f);
        }

        StartCoroutine( PlaySFX(audioArray[1], audioArray[1].volume));

        // After Wobble
        if (isReadyLA != false)
        {/*
            for (float i = CurrentRendLA.material.GetFloat(noiseControlName); i > 0; i -= afterWobble)
            {
                CurrentRendLA.material.SetFloat(noiseControlName, i);
                yield return null;
            }*/

            //float oldval = CurrentRendLA.material.GetFloat(noiseControlName);
            //LeanTween.value(this.gameObject, oldval, 0f, 0.2f).setOnUpdate(
            //       (x) => { CurrentRendLA.material.SetFloat(noiseControlName, x); }).setEase(LeanTweenType.easeInOutSine);

            if (CurrentRendLA.material.GetFloat(noiseControlName) < 0) CurrentRendLA.material.SetFloat(noiseControlName, 0f);
        }

        // Debug.Log("exiting wobbler LA " + speed );
        
        isWobblingLA = false;
    }
    IEnumerator WobblerRA()
    {
        while (speedRA < speedChangePoint && speedRA > speedMinPoint)
        {
            float percent = Mathf.Lerp(speedRA, speedChangePoint, speedRA / speedChangePoint);
            float modifier = (percent - speedMinPoint) / (speedChangePoint - speedMinPoint);

            if (CurrentRA.gameObject.activeSelf == true)
            {
                CurrentRendRA.material.SetFloat(noiseControlName, modifier * noiseMod);

                if (audioArray[2].isPlaying == false) audioArray[2].Play();
                audioArray[2].volume = modifier;
            }

            yield return null;
        }

        StartCoroutine(PlaySFX(audioArray[2], audioArray[2].volume));

        // After Wobble
        if (isReadyRA != false)
        {
            for (float i = CurrentRendRA.material.GetFloat(noiseControlName); i > 0; i -= afterWobble)
            {
                CurrentRendRA.material.SetFloat(noiseControlName, i);
                yield return null;
            } 
        }

        CurrentRendRA.material.SetFloat(noiseControlName, 0f);

        isWobblingRA = false;
    }
    IEnumerator WobblerLL()
    {
        while (speedLL < speedChangePoint && speedLL > speedMinPoint)
        {
            float percent = Mathf.Lerp(speedLL, speedChangePoint, speedLL / speedChangePoint);
            float modifier = (percent - speedMinPoint) / (speedChangePoint - speedMinPoint);

            if (CurrentLL.gameObject.activeSelf == true)
            {
                CurrentRendLL.material.SetFloat(noiseControlName, modifier * noiseMod);

                if (audioArray[3].isPlaying == false) audioArray[3].Play();
                audioArray[3].volume = modifier;
            }

            yield return null;
        }

        StartCoroutine(PlaySFX(audioArray[3], audioArray[3].volume));

        // After Wobble
        if (isReadyLL != false)
        {
            for (float i = CurrentRendLL.material.GetFloat(noiseControlName); i > 0; i -= afterWobble)
            {
                CurrentRendLL.material.SetFloat(noiseControlName, i);
                yield return null;
            } 
        }

        CurrentRendLL.material.SetFloat(noiseControlName, 0f);
        
        isWobblingLL = false;
    }
    IEnumerator WobblerRL()
    {
        while (speedRL < speedChangePoint && speedRL > speedMinPoint)
        {
            float percent = Mathf.Lerp(speedRL, speedChangePoint, speedRL / speedChangePoint);
            float modifier = (percent - speedMinPoint) / (speedChangePoint - speedMinPoint);

            if (CurrentRL.gameObject.activeSelf == true)
            {
                CurrentRendRL.material.SetFloat(noiseControlName, modifier * noiseMod);

                if (audioArray[4].isPlaying == false) audioArray[4].Play();
                audioArray[4].volume = modifier;
            }

            yield return null;
        }

        StartCoroutine(PlaySFX(audioArray[4], audioArray[4].volume));

        // After Wobble
        if (isReadyRL != false)
        {
            for (float i = CurrentRendRL.material.GetFloat(noiseControlName); i > 0; i -= afterWobble)
            {
                CurrentRendRL.material.SetFloat(noiseControlName, i);
                yield return null;
            } 
        }

        CurrentRendRL.material.SetFloat(noiseControlName, 0f);
        
        isWobblingRL = false;
    }


    // Function to produce the actual transition effect from one mesh to the next
    IEnumerator TheChangeOver(Renderer activeRend, Renderer changeToRend)
    {
        float noiseAmount = activeRend.material.GetFloat(noiseControlName);
        

        while (noiseAmount < noiseMax) // begin distortion effect
        {
            noiseAmount += changeSpeed;
            activeRend.material.SetFloat(noiseControlName, noiseAmount);
            yield return null;
        }

        changeToRend.gameObject.SetActive(true);

        Vector3 preScale = activeRend.transform.localScale;        

        while (noiseAmount > 0) // begin changeover transitions
        {
            noiseAmount -= changeSpeed;
            changeToRend.material.SetFloat(noiseControlName, noiseAmount);

            
            if (noiseAmount > noiseThreshold)
                activeRend.transform.localScale -= scaleSpeed;

            if ( noiseAmount < noiseThreshold)
            {
                Vector3 addToScale = (preScale - activeRend.transform.localScale);
                activeRend.transform.localScale += addToScale;
                activeRend.material.SetFloat(noiseControlName, 0f);
                activeRend.gameObject.SetActive(false);
            }

            yield return null;
        }

        changeToRend.material.SetFloat(noiseControlName, 0f);

        // update new object into the appropriate variable 
        if (activeRend.gameObject.activeSelf == false)
        {
            if (activeRend.name == CurrentRendH.name)
            {
                CurrentRendH = changeToRend;
                CurrentH = changeToRend.transform;
            }
            if (activeRend.name == CurrentRendBody.name)
            {
                CurrentRendBody = changeToRend;
                CurrentBody = changeToRend.transform;
            }
            if (activeRend.name == CurrentRendLA.name)
            {
                CurrentRendLA = changeToRend;
                CurrentLA = changeToRend.transform;
            }
            if (activeRend.name == CurrentRendRA.name)
            {
                CurrentRendRA = changeToRend;
                CurrentRA = changeToRend.transform;
            }
            if (activeRend.name == CurrentRendLL.name)
            {
                CurrentRendLL = changeToRend;
                CurrentLL = changeToRend.transform;
            }
            if (activeRend.name == CurrentRendRL.name)
            {
                CurrentRendRL = changeToRend;
                CurrentRL = changeToRend.transform;
            } 
        }
    }

    
    // Funtion to check the RNG isn't the same mesh and is always a new mesh
    int testRNG(Transform active, int temp)
    {
        if (active.parent.name == _BaseMeshes[temp].name)
        {
            if(temp < (_BaseMeshes.Count - 1))
                return (temp + 1);
            else
                return (temp - 1);
        }
        return temp;
    }
    

    // Function to check which body part is transitioning and pick a new part to replace it
    IEnumerator Transition(Transform WhichBone)
    {
        if (WhichBone.name == HeadBone.name && isReadyH == true)
        {
            isReadyH = false;                       

            int tempResult1 = RNG;
            tempResult1 = testRNG(CurrentH, tempResult1); // pick the new part

            NewRendH = _HeadsRenderer[tempResult1];
            NewRendBody = _BodiesRenderer[tempResult1];

            StartCoroutine( TheChangeOver(CurrentRendH, NewRendH)); // start the transition
            yield return StartCoroutine(TheChangeOver(CurrentRendBody, NewRendBody));

            isReadyH = true;
        }


        if (WhichBone.name == LArmBone.name && isReadyLA == true)
        {
            isReadyLA = false;


            int tempResult2 = RNG;
            tempResult2 = testRNG(CurrentLA, tempResult2);

            NewRendLA = _LeftArmsRenderer[tempResult2];

            yield return StartCoroutine( TheChangeOver(CurrentRendLA, NewRendLA));
            
            isReadyLA = true;
        }


        if (WhichBone.name == RArmBone.name && isReadyRA == true)
        {
            isReadyRA = false;


            int tempResult3 = RNG;
            tempResult3 = testRNG(CurrentRA, tempResult3);

            NewRendRA = _RightArmsRenderer[tempResult3];

            yield return StartCoroutine(TheChangeOver(CurrentRendRA, NewRendRA));
            
            isReadyRA = true;
        }


        if (WhichBone.name == LLegBone.name && isReadyLL == true)
        {
            isReadyLL = false;
            
            int tempResult4 = RNG;
            tempResult4 = testRNG(CurrentLL, tempResult4);

            NewRendLL = _LeftLegsRenderer[tempResult4];

            yield return StartCoroutine(TheChangeOver(CurrentRendLL, NewRendLL));
            
            isReadyLL = true;
        }


        if (WhichBone.name == RLegBone.name && isReadyRL == true)
        {
            isReadyRL = false;
            
            int tempResult5 = RNG;
            tempResult5 = testRNG(CurrentRL, tempResult5);

            NewRendRL = _RightLegsRenderer[tempResult5];

            yield return StartCoroutine(TheChangeOver(CurrentRendRL, NewRendRL));

            isReadyRL = true;
        }

        yield return new WaitForSeconds(0.25f);
    }

}
