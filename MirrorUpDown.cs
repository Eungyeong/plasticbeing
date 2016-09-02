using UnityEngine;
using System.Collections;

public class MirrorUpDown : MonoBehaviour {

  public float timer;

	// Use this for initialization
	void Start () {
     
    }

    // Update is called once per frame
    void Update () {

        timer = Time.time;

        if(timer < 110) // mirror up
        {
            if (transform.position.y < 1.55f)
            {
                transform.Translate(0f, 0f, 0.02f);
            }
        } else if (timer > 110) // mirror down
        {
            transform.Translate(0f, 0f, -0.03f);
        }
    }
}
