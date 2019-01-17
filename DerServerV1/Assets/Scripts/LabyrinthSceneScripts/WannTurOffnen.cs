using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WannTurOffnen : MonoBehaviour {
    public int schalter1;
    public int schalter2;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (schalter1 == 1)
        {
            if (schalter2 == 1)
            {
                GetComponent<Animator>().Play("Turoffen");
            }
        }
	}
}
