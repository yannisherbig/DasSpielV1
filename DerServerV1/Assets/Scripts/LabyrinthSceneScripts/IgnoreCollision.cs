using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(collision.gameObject.tag);
        if(collision.gameObject.tag=="NoHitBoxPlayer")
        {
            Physics.IgnoreCollision( collision.gameObject.GetComponent<Collider>() , GetComponent<Collider>());
        }
        if (collision.gameObject.tag == "Player")
        {
            Physics.IgnoreCollision(collision.gameObject.GetComponent<Collider>(), GetComponent<Collider>());
        }

        if (collision.gameObject.tag == "Trap")
        {
            GetComponent<Health2>().TakeDamage(25);
        }
    }

}
