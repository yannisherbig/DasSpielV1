using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {

    void OnCollisionEnter(Collision collision)
    {
        
        var hit = collision.gameObject;
        if (hit.tag.Equals("Player"))
        {
            var health = hit.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(10);
            }
        }

        Destroy(gameObject);
    }
}
