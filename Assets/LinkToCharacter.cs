using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LinkToCharacter : NetworkBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        
    }

    public override void OnStartClient()
    {
        Debug.Log("HEY");
        if (isServer)
        {
            SyncObjects();
            //sync the colorID
        }
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        GameObject.Find("CHARACTER").GetComponent<MovementScript>().controller = GetComponent<NetworkIdentity>();
        GameObject.Find("CHARACTER").GetComponent<MovementScript>().secondStart();
        GameObject.Find("SCENESWAPPER").GetComponent<SceneSwapper>().isHOST = isServer;
        base.OnStartLocalPlayer();
    }

    void SyncObjects()
    {
        for (int i = 0; i < GameObject.Find("INTERACTABLEOBJECTS").transform.childCount; i++)
        {
            Debug.Log($"Syncing object - {GameObject.Find("INTERACTABLEOBJECTS").transform.GetChild(i).name}...");
            CmdSyncObjects(GameObject.Find("INTERACTABLEOBJECTS").transform.GetChild(i).transform.position, netId, i);
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdSyncObjects(Vector3 pos,uint filter, int ID)
    {
        RpcSyncObjects(pos, filter, ID);
    }
    [ClientRpc]
    void RpcSyncObjects(Vector3 pos, uint filter, int ID)
    {

        GameObject.Find("INTERACTABLEOBJECTS").transform.GetChild(ID).transform.position = pos;

    }
}
