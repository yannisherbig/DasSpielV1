using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet2 : MonoBehaviour {

    public ParticleSystem explosionEffect;
    public bool detectedBefore = false;
    public string shotCameFromPlayerIP;

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        Destroy(Instantiate(explosionEffect.gameObject, gameObject.transform.position, Quaternion.identity) as GameObject, explosionEffect.main.startLifetime.constant);
        var hit = collision.gameObject;
        //Debug.Log("Coll detected, hit = " + hit.name);

        if (hit.tag.Equals("Player") && !detectedBefore)
        {
            detectedBefore = true;
            //Debug.Log("player Coll detected");
            var health = hit.GetComponent<Health2>();
            if (health != null)
            {
                health.TakeDamage(10, shotCameFromPlayerIP);
            }
        }
    }

    //void OnCollisionExit(Collision collisionInfo)
    //{
    //    Debug.Log("Collision Out: " + gameObject.name);
    //    Debug.Log("Collision Out: " + collisionInfo.gameObject.tag);
    //}
}
