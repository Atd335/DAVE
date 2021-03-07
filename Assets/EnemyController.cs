using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EnemyController : NetworkBehaviour
{

    public CharacterController CC;

    public AudioSource AS;
    public AudioClip spawnClip;

    public MeshRenderer[] healthBlips;

    void Start()
    {
        AS = GetComponent<AudioSource>();
        AS.PlayOneShot(spawnClip,.3f);
        if (!NetworkServer.active)
        {
            Debug.Log("NETWORK SERVER IS INACTIVE");
            CC.enabled = false;
        }
    }

    public Vector3 moveDirection;
    public Vector3 pushDirection;

    public float speed = 5;
    public float weight = 7;

    public Vector3 lerpPos;

    public Transform body;
    Vector3 lookPosition = new Vector3(0,0,1);

    public int health;

    public RaycastHit hit;

    public Transform projPoint;

    public bool characterInSight;

    public bool dead;

    void Update()
    {
        if (!dead)
        {
            if (NetworkServer.active)
            {
                Physics.Raycast(projPoint.position, (Camera.main.transform.position - projPoint.position).normalized, out hit);
                Physics.Raycast(transform.position, Vector3.down, out RaycastHit downHit);

                if (downHit.distance > 0)
                {
                    CC.enabled = false;
                    transform.position = new Vector3(transform.position.x, downHit.point.y, transform.position.z);
                    CC.enabled = true;
                }

                conditionalMovement();
                if (CC.enabled)
                {
                    CC.Move(((pushDirection * weight) + (moveDirection * speed)) * Time.deltaTime);
                }
                CmdUpdatePosition(transform.position);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, lerpPos, Time.deltaTime * 40);
            }
            lookPosition = Vector3.Lerp(lookPosition, new Vector3(MovementScript.charPos.x, body.transform.position.y, MovementScript.charPos.z), Time.deltaTime * 15f);
            body.LookAt(lookPosition);

            for (int i = 0; i < healthBlips.Length; i++)
            {
                healthBlips[i].enabled = health > i;
            }
        }
        else
        {

        }
    }

    void conditionalMovement()
    {
        pushDirection = Vector3.Lerp(pushDirection, Vector3.zero, Time.deltaTime * 3);
        characterInSight = hit.collider != null && hit.collider.tag == "MainCamera";

        if (characterInSight)
        {
            moveDirection = MovementScript.charPos - transform.position;
            moveDirection.Normalize();
            moveDirection.y = 0;
        }
        else
        {            
            moveDirection = Vector3.Cross(MovementScript.charPos - transform.position, Vector3.up).normalized;
            moveDirection.Normalize();
            moveDirection.y = 0;
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdUpdatePosition(Vector3 pos)
    {
        RpcUpdatePosition(pos);
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector3 pos)
    {
        if (!NetworkServer.active)
        {
            lerpPos = pos;

        }
    }

}
