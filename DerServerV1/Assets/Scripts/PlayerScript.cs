using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {

    //Create a public reference to the ServerScript game object
    public GameObject serverScript;

    //Reference the script attached to the "ServerScript" game object
    public ServerScript theScript;

    public float raycastDistance = 400;

    GameObject collidingPlayerObject;

    public GameObject slightForwardPoint;

    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed;
	
	public float distToWallAhead;
    
    void Start () { 
        theScript = serverScript.GetComponent<ServerScript>();
        collidingPlayerObject = GetComponent<Collider>().gameObject;
        //Debug.Log("collidingPlayerObject: " + collidingPlayerObject);
        //slightForwardPoint = GetComponent<GameObject>().gameObject;
    }
	

	void Update () {

        //Vector3 direction = new Vector3(0, transform.rotation.y, 0);
        //Debug.Log("Vector3.Normalize(slightForwardPoint.transform.position - transform.position): " + Vector3.Normalize(transform.GetChild(). - transform.position));
        RaycastHit hit;
        Ray landingRay = new Ray(transform.position, Vector3.Normalize(slightForwardPoint.transform.position - transform.position));
        //Ray landingRay = new Ray(transform.position, transform.position + new Vector3(1, 0, 1));
        //Debug.Log(slightForwardPoint.transform.position);
        //Debug.DrawRay(transform.position, Vector3.Normalize(slightForwardPoint.transform.position - transform.position) * raycastDistance);

        if(Physics.Raycast(landingRay, out hit, raycastDistance))
        {
            //Debug.Log("raycast hit");
            // Player collidingPlayer = null;
            // Debug.Log(theScript.players.Count);
            // foreach (KeyValuePair<string, Player> entry in theScript.players)
            // {
                // Debug.Log("for loop entered; entry hashcode: " + theScript.players.GetHashCode());
                // Debug.Log("entry.Value.PlayerObject" + entry.Value.PlayerObject);
                // if (GameObject.ReferenceEquals(entry.Value.PlayerObject, collidingPlayerObject))
                // {
                   // Debug.Log("CollidingPlayer found in dictionary");
                    // collidingPlayer = entry.Value;
                    // Debug.Log("collidingPlayerObject found in dictionary");

                // }
            // }

            // if(collidingPlayer == null)
            // {
               // Debug.Log("CollidingPLayer not found in dictionary");
                // return;
            // }

        
            //Debug.Log(distance);
            //Debug.Log(hit.collider.gameObject.tag + " " + distance);
            //if (hit.collider.gameObject.tag.Equals("PickUp"))
            //{
            //    //Debug.Log("Tag==PickUp");
            //    collidingPlayer.ClosestDistToPickUp = distance;
            //}
            if (hit.collider.gameObject.tag.Equals("Wall"))
            {
                //Debug.Log("Tag==Wall");
                distToWallAhead = hit.distance;
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
       // theScript = serverScript.GetComponent<ServerScript>();
        //Debug.Log("OnTriggerEnter() called");
        if (other.gameObject.CompareTag("PickUp"))
        {
            //Debug.Log("if-clause entered");
            //GameObject collidingPlayerObject = GetComponent<Collider>().gameObject;
            //Debug.Log("collidingPlayerObject = " + collidingPlayerObject);
            //Debug.Log("theScript.players.count = " + theScript.players.Count);

            foreach (KeyValuePair<string, Player> entry in theScript.players)
            {
                //Debug.Log("for loop entered; entry hashcode: " + theScript.players.GetHashCode());
                if (GameObject.ReferenceEquals(entry.Value.PlayerObject, collidingPlayerObject))
                {
                    //Debug.Log("collidingPlayerObject found in dictionary");
                    entry.Value.Score++;
                }
            }
            other.gameObject.SetActive(false);
        }
    }

    public void Fire()
    {
        //Vector3 forwardVel = GetComponent<Transform>().forward * speed;
        //Vector3 horizontalVel = GetComponent<Transform>().right * speed;

        //GetComponent<Rigidbody>().velocity = forwardVel + horizontalVel;

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        //var bullet = (GameObject)Instantiate(
        //    bulletPrefab,
        //    bulletSpawn.position,
        //    GetComponent<Transform>().rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + bullet.transform.right) * bulletSpeed;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 4.0f);
    }
}
