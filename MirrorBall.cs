using UnityEngine;
using System.Collections;

public class MirrorBall : MonoBehaviour {

    public Transform centreMirrorBall;

    Vector3 myRot;
    float x, y, z, deg;

    // Use this for initialization
    void Start () {
        myRot = Vector3.zero;
        x = y = z = 0;
        deg = - 0.2f;
    }
	
	// Update is called once per frame
	void Update () {
        x -= 0.25f;
        y -= 0.22f;
        
        myRot = new Vector3(x, y, z);
        centreMirrorBall.rotation = Quaternion.Euler(myRot);
    }
}
