using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    float originalY;
    public float floatStrength = 0.6f;  // Den Bereich der möglichen y-Werte bestimmen
    public bool isActive;
    
    void Start()
    {
        originalY = transform.position.y;
    }

    void Update()
    {
        if (isActive)
        {
            transform.position = new Vector3(transform.position.x,
            originalY + ((float)System.Math.Sin(Time.time) * floatStrength),
            transform.position.z);
        }
    }

}
