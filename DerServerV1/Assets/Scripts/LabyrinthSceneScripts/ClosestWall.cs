using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClosestWall : MonoBehaviour {

    public float distance;
    public float angle;
    public Collider coll;
    public GameObject player;
    public float RotateSpeed = 30f;

    // Use this for initialization
    void Start () {
        coll = GetComponent<Collider>();

    }
	
	// Update is called once per frame
	void Update () {
        //Hier beginnt ein Test ob alles Funktioniert ohne Server nötig ist
        /*
        if (Distance() < 2)
        {   if(Angle()>0)
                if (Angle()<80)
                {
                    transform.rotation = Quaternion.Euler(0, -10 + transform.eulerAngles.y, 0);
                }
                 else if(Angle()>120)
            {
                        transform.rotation = Quaternion.Euler(0, 10 + transform.eulerAngles.y, 0);
            }
            
            if(Angle()<0)
                if (Angle() < -80)
                {
                    transform.rotation = Quaternion.Euler(0, 10 + transform.eulerAngles.y, 0);
                }
                  else  if (Angle() > -120)
                    {
                        transform.rotation = Quaternion.Euler(0, -10 + transform.eulerAngles.y, 0);
                    }
        }

        */

        //Hier endet es
        if (Input.GetKeyDown("l"))
        {
            transform.rotation = Quaternion.Euler(0,10+transform.eulerAngles.y,0);
        }

    }


    public GameObject FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Wall");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }


    public float Angle()
    {

        Vector3 targetDir = FindClosestEnemy().transform.position - player.transform.position;
        Vector3 forward = transform.forward;
        angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
        return angle;
    }

    public float Distance()
    {
        distance = Vector3.Distance(FindClosestEnemy().transform.position, player.transform.position);
        return distance;
    }


}
