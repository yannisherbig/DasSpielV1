using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Druckplatte1Tor : MonoBehaviour {
    public GameObject tur;
    WannTurOffnen referenzscript;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        referenzscript=tur.GetComponent<WannTurOffnen>();
        referenzscript.schalter1 = 1;
    }

    private void OnTriggerExit(Collider other)
    {
        referenzscript = tur.GetComponent<WannTurOffnen>();
        referenzscript.schalter1 = 0;
    }

}
