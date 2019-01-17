using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer2 : MonoBehaviour {

    public Text timerText;

    public float seconds, minutes, hours;

    //Create a public reference to the ServerScript game object
    public GameObject serverScript;

    //Reference the script attached to the "ServerScript" game object
    public ServerScript2 theScript;

    private float startButtonClickTime;

    IEnumerator Start()
    {
        theScript = serverScript.GetComponent<ServerScript2>();
       
        while (!theScript.startButtonClicked)
        {
            yield return null;
        }
        startButtonClickTime = Time.timeSinceLevelLoad;
        timerText = GetComponent<Text>() as Text;
    }

    void Update () {

        if (theScript.startButtonClicked)
        {
            float timeSinceStartButtonClicked = Time.timeSinceLevelLoad - startButtonClickTime;
            minutes = (int)(timeSinceStartButtonClicked / 60f);
            seconds = (int)(timeSinceStartButtonClicked % 60f);
            hours = (int)((timeSinceStartButtonClicked / 3600) % 24f);
            timerText.text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
        else
        {
            startButtonClickTime = Time.timeSinceLevelLoad;
        }
    }
}
