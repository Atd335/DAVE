using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class SceneSwapper : MonoBehaviour
{

    public NetworkManager nManager;

    public string IPADDRESS;

    public string offlineSwapScene;
    public string sceneToSwapTo;

    public bool isHOST;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        nManager = GameObject.Find("HUD").GetComponent<NetworkManager>();
        IPADDRESS = nManager.networkAddress;
        //
        nManager.offlineScene = offlineSwapScene;
        //nManager.onlineScene = sceneToSwapTo;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.RightShift) && isHOST)// 1==2 not true
        {
            if (isHOST)
            {
                nManager.StopHost();
            }
            else
            {
                nManager.StopClient();
            }
        }
    }
}
