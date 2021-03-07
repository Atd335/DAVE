using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BulletScript : NetworkBehaviour
{
    public NetworkIdentity NID;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(NetworkServer.active)
        {
            transform.position += transform.forward * Time.deltaTime * 60;
            CmdUpdateBullet(transform.position);
            if (Vector3.Distance(transform.position, MovementScript.charPos) > 40)
            {
                CmdDestoryBullet();
            }
            RaycastHit hit;
            Physics.Raycast(transform.position,transform.forward,out hit);
            if (hit.distance<.15f)
            {
                CmdDestoryBullet();
            }
        }
        
    }

    [Command(ignoreAuthority = true)]
    void CmdDestoryBullet()
    {
        RpcDestoryBullet();
    }
    [ClientRpc]
    void RpcDestoryBullet()
    {
        Destroy(this.gameObject);
    }
    
    [Command(ignoreAuthority = true)]
    void CmdUpdateBullet(Vector3 pos)
    { 
        RpcUpdateBullet(pos);
    }

    [ClientRpc]
    void RpcUpdateBullet(Vector3 pos)
    {
        if (!NetworkServer.active)
        {
            Debug.Log("H");
            transform.position = pos;
        }
    }
}
