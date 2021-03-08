using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncFOV : MonoBehaviour
{
    public Camera cam;

    void LateUpdate()
    {
        GetComponent<Camera>().fieldOfView = cam.fieldOfView;
    }
}
