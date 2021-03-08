using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public float _speed = 10;
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

    //slower;
    public static float physicsSpeed = 1;
    public  float physicsSpeedDelta = 1;
    public float slowSpeed;

    //UI
    public Image slowBar;
    public float _slowMeter = 1;
    public static float slowMeter = 1;
    public float slowMeterDeplete = 1;
    public bool slowEmpty;

    void Start()
    {
        physicsSpeed = 1;
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
        HEAD.GetComponent<Camera>().fieldOfView = Mathf.Lerp(HEAD.GetComponent<Camera>().fieldOfView, 75f, Time.deltaTime/2f);
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
        if (slowMeter == 0) { slowEmpty = true; }
        if (Input.GetKey(KeyCode.Mouse0) && !slowEmpty)
        {
            physicsSpeedDelta = Mathf.Lerp(physicsSpeedDelta, slowSpeed, Time.deltaTime*10);
            _slowMeter -= slowMeterDeplete * Time.deltaTime;           
        }
        else
        {
            physicsSpeedDelta = Mathf.Lerp(physicsSpeedDelta, 1, Time.deltaTime*10);
            _slowMeter += slowMeterDeplete / 3 * Time.deltaTime;
            if (slowMeter == 1) { slowEmpty = false; }
        }
        if (!slowEmpty) { slowBar.color = new Color(1, 1, 1, .6f); }
        else { slowBar.color = new Color(.5f, .5f, .5f, .4f); }

        _slowMeter = Mathf.Clamp(_slowMeter,0,1);

        CmdSlowTime(physicsSpeedDelta,_slowMeter);
        slowBar.fillAmount = slowMeter;
    }

    [Command(ignoreAuthority = true)]
    void CmdSlowTime(float spd,float meter)
    {
        RpcSlowTime(spd,meter);   
    }
    [ClientRpc]
    void RpcSlowTime(float spd, float meter)
    {
        MovementScript.physicsSpeed = spd;
        MovementScript.slowMeter = meter;
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
        if (sound == 0)
        {
            HEAD.GetComponent<Camera>().fieldOfView = 80f;
        }
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
                        _speed *= 1.5f;
                        CmdPlaySound(0,1);//jumpsound
                        jumpCount++;
                        moveDirection.y = jumpHeight;
                        jumping = false;
                    }
                    else
                    {
                        moveDirection.y = -.01f;
                    }
                    _speed = Mathf.Lerp(_speed, speed, Time.deltaTime * 10 * physicsSpeed);
                }
                else
                {
                    _speed = Mathf.Lerp(_speed, speed * 1.25f, Time.deltaTime * 10f);
                    if (jumpCount < 2)
                    {
                        if (jumping)
                        {
                            _speed *= 1.5f;
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
                    moveDirection.y -= grav * Time.fixedDeltaTime * physicsSpeed;
                }

                //moveDirection.y = Mathf.Clamp(moveDirection.y,-6,999);
                //moveDirectionCam.y = Mathf.Clamp(moveDirectionCam.y, -50, 999);
                moveDirectionCam = moveDirection.x * HEADMAST.right + moveDirection.z * HEADMAST.forward + moveDirection.y * HEADMAST.up;
                moveDirectionCam.x *= _speed;
                moveDirectionCam.z *= _speed;
                CC.Move(moveDirectionCam * 10 * Time.fixedDeltaTime * physicsSpeed);
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
