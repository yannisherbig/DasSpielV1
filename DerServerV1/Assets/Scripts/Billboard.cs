using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour
{
    //Orient the camera after all movement is completed this frame to avoid jittering
    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }

    //void Update()
    //{
    //    transform.LookAt(Camera.main.transform.position);
    //    //transform.Rotate(0, 180, 0);
    //    Vector3 objectNormal = transform.rotation * Vector3.forward;
    //    Vector3 cameraToText = transform.position - Camera.main.transform.position;
    //    float f = Vector3.Dot(objectNormal, cameraToText);
    //    if (f < 0f)
    //    {
    //        transform.Rotate(0f, 180f, 0f);
    //    }
    //}
}