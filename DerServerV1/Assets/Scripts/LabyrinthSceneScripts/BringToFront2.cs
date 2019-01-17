using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BringToFront2 : MonoBehaviour {

    private void OnEnable()
    {
        transform.SetAsLastSibling();
    }
}
