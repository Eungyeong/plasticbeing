using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the RoomDome along with reflections probes at a specified speed
/// </summary>

public class Rotater : MonoBehaviour {

    public Transform lightShape1;
    public Transform Reflect1;
   // public Transform Reflect2;
   // public Transform Reflect3;
   // public Transform Reflect4;
    Vector3 myRot;
    float x, y, z, deg;

	// Use this for initialization
	void Start () {
        myRot = Vector3.zero;
        x = y = z = 0;
        deg = 0.1f;    
    }
	
	// Update is called once per frame
	void Update () {
        
        x += 0f;
        y += 0.05f;
        myRot = new Vector3(x, y, z);
        lightShape1.rotation = Quaternion.Euler(myRot);
        Reflect1.RotateAround(Vector3.zero, Vector3.up, deg);
        //Reflect2.RotateAround(Vector3.zero, Vector3.up, deg);
        //Reflect3.RotateAround(Vector3.zero, Vector3.up, deg);
        //Reflect4.RotateAround(Vector3.zero, Vector3.up, deg);
        
    }

}
