using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour {

    private Rigidbody rb;
    public float speed;
    public int schaltermenge;
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletSpeed;
    public string status;
   
    public int brake;
    // Use this for initialization
    void Start() {
        rb = GetComponent<Rigidbody>();
        schaltermenge = 0;
        rb.freezeRotation=true;
       

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown("3"))
        {
            fire();
        }

        if (Input.GetKeyDown("space"))
        { 
            rb.AddForce(-brake * rb.velocity);

        }

    }

    private void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        rb.AddForce(movement * speed);
        
    }

    public void schaltermengeerhohen(){
        schaltermenge++;
        }

    public int getschaltermenge()
    {
        return schaltermenge;
    }


    public void fire()
    {

        Physics.IgnoreCollision(bulletPrefab.GetComponent<Collider>(), GetComponent<Collider>());

        // Create the Bullet from the Bullet Prefab
        var bullet = (GameObject)Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation);

        // Add velocity to the bullet
        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + bullet.transform.right) * bulletSpeed;

        // Destroy the bullet after 2 seconds
        Destroy(bullet, 4.0f);
    }



}
