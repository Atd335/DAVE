using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SceneSwapperOFFLINE : MonoBehaviour
{

    public NetworkManager nManager;
    public SceneSwapper SS;
    void Start()
    {
        SS = GameObject.Find("SCENESWAPPER").GetComponent<SceneSwapper>();
        nManager = GameObject.Find("HUD").GetComponent<NetworkManager>();

        nManager.onlineScene = SS.sceneToSwapTo;

        if (SS.isHOST)
        {
            nManager.StartHost();
        }
        else
        {
            nManager.StartClient();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
