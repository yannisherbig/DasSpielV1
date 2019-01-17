using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DruckplatteFalleDeaktivieren : MonoBehaviour {
    public GameObject falle0;
    //Falle1 bis falle4 sind die Feuer Traps
    public GameObject falle1;
    public GameObject falle2;
    public GameObject falle3;
    public GameObject falle4;
    public GameObject falle5;
    public GameObject falle6;
    public GameObject falle7;
    public GameObject falle8;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    { //Hier lasse ich die Animation ein letztes mal abspielen, damit sie danach gestoppt und an der ersten Frame stehen bleibt
        falle0.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle1.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle2.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle3.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle4.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle5.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle6.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle7.GetComponent<Animation>().wrapMode = WrapMode.Once;
        falle8.GetComponent<Animation>().wrapMode = WrapMode.Once;
    }
}
