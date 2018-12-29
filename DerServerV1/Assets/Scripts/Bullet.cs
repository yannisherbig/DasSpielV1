using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
        // Hit effect here: ...

        var hit = collision.gameObject;
        Debug.Log("Coll detected, hit = " + hit.name);
        if (hit.tag.Equals("Player"))
        {
            Debug.Log("player Coll detected");
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(10);
            }
        }
    }
}
