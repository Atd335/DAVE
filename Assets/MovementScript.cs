using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MovementScript : NetworkBehaviour
{
    public NetworkIdentity controller;

    public bool legs;
    public bool head;

    CharacterController CC;

    Vector3 moveDirection;
    public Vector3 moveDirectionCam;
    public float speed = 10;
    public float grav;
    public float jumpHeight;
    public float walkMag = 1;
    public float sprintMag = 2;


    public Transform HEAD;
    public Transform HEADMAST;
    public float sens = 360;
    float xRot;
    float yRot;
    Vector3 camRot;

    Vector3 posLerp;
    Vector3 rotLerp;

    public GameObject bullet;

    NetworkManager MANAGER;

    public static Vector3 charPos;

    public Transform handPos;
    public Transform beamTransform;
    public float beamLength;
    public float beamThickness;
    public Material beamMat;

    public GameObject shotIndicator;

    public Transform handCam;

    float headTimer;
    public float headSpeed;
    public float headMag;

    public GameObject ping;

    //obj bools
    public static bool holdingItem;
    public static GameObject heldObj;
    public AudioSource AS;
    public AudioClip pickUp;
    public AudioClip putDown;

    //tools
    public bool hasGun;

    public Color[] gunColor;
    public int activeColor;
    public MeshRenderer gunRend;
    public MeshRenderer beamRend;

    void Start()
    {
        AS = GetComponent<AudioSource>();
        holdingItem = false;
        MANAGER = GameObject.Find("HUD").GetComponent<NetworkManager>();
        CC = GetComponent<CharacterController>();
        posLerp = transform.position;
    }

    public void secondStart()
    {
        if (controller.isServer)
        {
            head = true;
            legs = false;
        }
        else
        {
            legs = true;
            head = false;
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        RaycastHit hit;
        Physics.Raycast(HEAD.position, HEAD.forward, out hit);
        if (hit.collider!=null)
        {
            beamLength = (hit.point - beamTransform.position).magnitude;
            handPos.LookAt(hit.point);
        }
        else
        {
            beamLength = 999;
            handPos.LookAt(HEAD.position + HEAD.forward * 9999);
        }

        beamThickness = Mathf.Lerp(beamThickness,.1f,Time.deltaTime * 10);
        //beamMat.color = Color.Lerp(beamMat.color, new Color(1,1,1,0),Time.deltaTime * 5);

        beamRend.material.SetColor("_Color", Color.Lerp(beamRend.material.color, new Color(beamRend.material.color.r, beamRend.material.color.g, beamRend.material.color.b,0),Time.deltaTime * 5));

        beamTransform.localScale = new Vector3(beamThickness, beamLength, beamThickness);

        charPos = transform.position;
        if (!controller) { return; }
        if (Input.GetKeyDown(KeyCode.Return)) { head = true; legs = true; }


        CC.enabled = legs;


        if (head)
        {
            MOUSEINPUTS();
            if (NetworkServer.connections.Count>1 || !controller.isServer)
            {
                transform.position = Vector3.Lerp(transform.position, posLerp, Time.deltaTime * 40);
            }
        }
        if (legs)
        {
            WASDINPUTS();
            if (NetworkServer.connections.Count > 1 || !controller.isServer)
            {
                //HEAD.rotation = Quaternion.Euler(rotLerp);
                HEAD.rotation = Quaternion.Lerp(HEAD.rotation,Quaternion.Euler(rotLerp),Time.deltaTime*40);
            }
            CmdUpdateLegStates(new Vector3(moveDirectionCam.x, 0, moveDirection.z).magnitude > .1f);
        }
        HEADMAST.rotation = Quaternion.Euler(0, HEAD.rotation.eulerAngles.y, 0);
        gunRend.material.SetColor("_Color", Color.Lerp(gunRend.material.color, gunColor[activeColor],Time.deltaTime * 20));
    }

    public int colorImOn;

    [Command(ignoreAuthority = true)]
    void CmdUpdatePlatState(int platColor)
    {
        RpcUpdatePlatState(platColor);
    }
    [ClientRpc]
    void RpcUpdatePlatState(int platColor)
    {
        colorImOn = platColor;
    }

    void GunColorSync()
    {
        //gunRend.material.SetColor("_Color",gunColor[activeColor]);
        //beamRend.material.SetColor("_Color",new Color(gunColor[activeColor].r, gunColor[activeColor].b, gunColor[activeColor].g, beamRend.material.color.a));
    }
    
    public bool bodyWalking;

    [Command(ignoreAuthority = true)]
    void CmdUpdateLegStates(bool moving)
    {
        RpcUpdateLegStates(moving);
    }

    [ClientRpc]
    void RpcUpdateLegStates(bool moving)
    {
        bodyWalking = moving;
    }


    public bool jumping;

    void WASDINPUTS()
    {
        moveDirection.x = Input.GetAxis("Horizontal");
        moveDirection.z = Input.GetAxis("Vertical");
        //moveDirection = Vector3.ClampMagnitude(moveDirection,1);

        moveDirectionCam = moveDirection.x * HEADMAST.right + moveDirection.z * HEADMAST.forward + moveDirection.y * HEADMAST.up;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumping = true;
        }
        //moveDirectionCam.Normalize();
        if (!Input.GetKey(KeyCode.LeftShift))
        {
            moveDirectionCam.x *= walkMag;
            moveDirectionCam.z *= walkMag;
        }
        else
        {
            moveDirectionCam.x *= sprintMag;
            moveDirectionCam.z *= sprintMag;
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            CmdSwitchColor(-1);
        }
        else if(Input.GetKeyDown(KeyCode.RightBracket))
        {
            CmdSwitchColor(1);
        }

        if (Input.GetKeyDown(KeyCode.E) && !holdingItem)
        {
            RaycastHit hit;
            Physics.Raycast(HEAD.position, HEAD.forward, out hit);
            if (hit.collider != null && hit.collider.tag == "interactable" && hit.distance<3.5f)
            {
                CmdInteractWithObj(hit.collider.gameObject.GetComponent<IntObjScript>().objID);
            }
        }
        else if (Input.GetKeyDown(KeyCode.E) && holdingItem)
        {
            CmdPutDown();
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdSwitchColor( int increment)
    {
        RpcSwitchColor(increment);
        GunColorSync();
    }

    public AudioClip flit;

    [ClientRpc]
    void RpcSwitchColor(int increment)
    {
        activeColor += increment;
        AS.PlayOneShot(flit);
        if (activeColor<0)
        {
            activeColor = gunColor.Length - 1;
        }
        if (activeColor >= gunColor.Length)
        {
            activeColor = 0;
        }      
    }

    [Command(ignoreAuthority = true)]
    void CmdInteractWithObj(int objID)
    {
        RpcInteractWithObj(objID);
    }
    [ClientRpc]
    void RpcInteractWithObj(int objID)
    {
        AS.PlayOneShot(pickUp, .1f);
        foreach (IntObjScript o in GameObject.Find("INTERACTABLEOBJECTS").GetComponentsInChildren<IntObjScript>())
        {
            if (o.objID == objID)
            {
                o.INTERACT();
            }
        }
        //GameObject.Find("INTERACTABLEOBJECTS").GetComponentsInChildren<IntObjScript>()[objID].INTERACT();
    }

    [Command(ignoreAuthority = true)]
    void CmdPutDown()
    {
        RpcPutDown();
    }
    [ClientRpc]
    void RpcPutDown()
    {
        AS.PlayOneShot(putDown,.1f);
        heldObj.GetComponent<pickUpScript>().PickUpToggle();
    }

    void MOUSEINPUTS()
    {
        xRot -= Input.GetAxis("Mouse Y") * sens * Time.deltaTime;
        yRot += Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        xRot = Mathf.Clamp(xRot,-89,89);
        camRot = new Vector3(xRot,yRot,0);
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (hasGun)
            {
                fireGun();
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            RaycastHit hit;
            Physics.Raycast(HEAD.position, HEAD.forward, out hit);
            Vector3 v;
            Vector3 u;
            if (hit.collider != null)
            {
                v = hit.point;
                u = hit.normal;
                CmdSpawnPing(v,u);
            }
        }
    }

    void fireGun()
    {
        RaycastHit hit;
        Physics.Raycast(HEAD.position, HEAD.forward, out hit);
        Vector3 v;
        Vector3 u;
        if (hit.collider != null)
        {
            v = hit.point;
            u = hit.normal;
            hitTags(hit);
        }
        else
        {
            v = Vector3.one * 9999;
            u = Vector3.one;
        }
        CmdSpawnBeam(v, u);
        //CmdSpawnBullet(HEAD.forward);
    }

    void hitTags(RaycastHit hit)
    {
        if (hit.collider.tag == "dummy")
        {
            hit.collider.GetComponentInParent<DummyScript>().hitMe();
        }

        if (hit.collider.tag == "Enemy")
        {
            CmdHitEnemy(hit.collider.gameObject.GetComponent<NetworkIdentity>().netId);
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdHitEnemy(uint eID)
    {
        RpcHitEnemy(eID);
    }
    [ClientRpc]
    void RpcHitEnemy(uint eID)
    {
        foreach (NetworkIdentity n in GameObject.Find("ENEMYSPAWNER").GetComponentsInChildren<NetworkIdentity>())
        {
            if (n.netId == eID)
            {
                n.GetComponent<EnemyController>().health--;
                n.GetComponent<EnemyController>().pushDirection = (n.transform.position - charPos).normalized;
                if (n.GetComponent<EnemyController>().health==0)
                {
                    n.GetComponent<EnemyController>().dead = true;
                }
            }
        }
    }

    public AudioClip[] sounds;

    [Command(ignoreAuthority = true)]
    void CmdPlaySound(int sound,float vol)
    {
        RpcPlaySound(sound, vol);
    }
    [ClientRpc]
    void RpcPlaySound(int sound, float vol)
    {
        AS.PlayOneShot(sounds[sound],vol);
    }

    [Command(ignoreAuthority = true)]
    void CmdSpawnPing(Vector3 pos, Vector3 upper)
    {
        RpcSpawnPing(pos, upper);
    }

    [ClientRpc]
    void RpcSpawnPing(Vector3 pos, Vector3 upper)
    {
        if (!GameObject.Find("PINGS")) { GameObject pp = new GameObject("PINGS"); }
        GameObject p = Instantiate(ping, pos, Quaternion.identity, GameObject.Find("PINGS").transform);
        p.transform.up = upper;
    }

    [Command(ignoreAuthority = true)]
    void CmdSpawnBeam(Vector3 pos,Vector3 upper)
    {
        RpcSpawnBeam(pos,upper);
    }

    [ClientRpc]
    void RpcSpawnBeam(Vector3 pos, Vector3 upper)
    {
        GameObject s = Instantiate(shotIndicator,pos,Quaternion.identity);
        s.transform.up = upper;
        beamThickness = .02f;
        beamRend.material.SetColor("_Color",gunColor[activeColor]);
    }

    [Command(ignoreAuthority = true)]
    void CmdSpawnBullet(Vector3 dir)
    {
        RpcSpawnBullet(dir);
    }

    [ClientRpc]
    void RpcSpawnBullet(Vector3 dir)
    {
        if (!NetworkServer.active) { return; }
        GameObject b = Instantiate(bullet);
        NetworkServer.Spawn(b);
        b.transform.position = handPos.position;
        b.transform.forward = handPos.forward;
        //b.GetComponent<BulletScript>().NID = controller;
    }

    int jumpCount = 0;

    private void FixedUpdate()
    {
        if (!controller) { return; }
        if (!controller.isLocalPlayer)
        {

        }
        else
        {
            if (head)
            {
                HEAD.rotation = Quaternion.Euler(camRot);
                CmdUpdateHead(camRot,controller.netId);
            }
            if (legs)
            {
                if (transform.position.y < -20)
                {
                    CmdRespawn();
                }
                if (CC.isGrounded)
                {
                    jumpCount = 0;
                    if (jumping)
                    {
                        //Debug.Log("JUMPED");
                        CmdPlaySound(0,1);//jumpsound
                        jumpCount++;
                        moveDirection.y = jumpHeight;
                        jumping = false;
                    }
                    else
                    {
                        moveDirection.y = -.01f;
                    }
                }
                else
                {
                    if (jumpCount < 2)
                    {
                        if (jumping)
                        {
                            CmdPlaySound(0,1);//jumpsound
                            jumpCount++;
                            moveDirection.y = jumpHeight;
                            jumping = false;
                        }
                    }
                    else
                    {
                        jumping = false;
                    }
                    moveDirection.y -= grav * Time.fixedDeltaTime;
                }
                moveDirection.y = Mathf.Clamp(moveDirection.y,-6,999);
                //moveDirectionCam.y = Mathf.Clamp(moveDirectionCam.y, -50, 999);
                moveDirectionCam = moveDirection.x * HEADMAST.right + moveDirection.z * HEADMAST.forward + moveDirection.y * HEADMAST.up;
                moveDirectionCam.x *= speed;
                moveDirectionCam.z *= speed;
                CC.Move(moveDirectionCam * 10 * Time.fixedDeltaTime);
                CmdUpdatePosition(transform.position,controller.netId);
            }
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdRespawn()
    {
        RpcRespawn();
    }
    [ClientRpc]
    void RpcRespawn()
    {
        if (legs) { CC.enabled = false; }
        transform.position = Vector3.zero;
        if (legs) { CC.enabled = true; }
    }

    [Command(ignoreAuthority = true)]
    void CmdUpdatePosition(Vector3 pos, uint filter)
    {
        RpcUpdatePostition(pos,filter);
    }
    [ClientRpc]
    void RpcUpdatePostition(Vector3 pos, uint filter)
    {
        if (filter!=controller.netId)
        {
            //transform.position = pos;
            posLerp = pos;
        }
    }

    [Command(ignoreAuthority = true)]
    void CmdUpdateHead(Vector3 rot, uint filter)
    {
        RpcUpdateHead(rot, filter);
    }
    [ClientRpc]
    void RpcUpdateHead(Vector3 rot, uint filter)
    {
        if (filter != controller.netId)
        {
            //HEAD.rotation = Quaternion.Euler(rot);
            rotLerp = rot;
        }
    }

}
