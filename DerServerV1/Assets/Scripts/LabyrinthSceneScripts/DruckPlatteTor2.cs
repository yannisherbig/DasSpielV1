using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruckPlatteTor2 : MonoBehaviour {
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
        referenzscript = tur.GetComponent<WannTurOffnen>();
        referenzscript.schalter2 = 1;
    }

    private void OnTriggerExit(Collider other)
    {
        referenzscript = tur.GetComponent<WannTurOffnen>();
        referenzscript.schalter2 = 0;
    }
}
