using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DummyScript : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.Euler(new Vector3(0,0,0));
    }

    public bool isHit;

    float animTimer;
    public float speed;

    public AnimationCurve hit;

    public float animMag; 

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            isHit = true;
        }

        if (isHit)
        {
            animTimer += Time.deltaTime * speed;
            animTimer = Mathf.Clamp(animTimer,0,1);
            transform.localRotation = Quaternion.Euler(0,0,hit.Evaluate(animTimer)*animMag);
            if (animTimer==1)
            {
                isHit = false;
                animTimer = 0;
            }
        }
    }

    public void hitMe()
    {
        //Destroy(this.gameObject);
        CmdHitMe();
    }

    [Command(ignoreAuthority = true)]
    void CmdHitMe()
    {
        RpcHitMe();
    }

    [ClientRpc]
    void RpcHitMe()
    {
        if (!isHit)
        {
            isHit = true;
            animTimer = 0;
        }
    }

}
