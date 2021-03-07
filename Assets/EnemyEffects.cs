using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEffects : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        EC = GetComponentInParent<EnemyController>();
    }

    public Mesh[] walkFrames;
    int animFrame;
    public MeshFilter MF;
    EnemyController EC;
    // Update is called once per frame
    void Update()
    {
        if (!EC.dead)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 15);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(2,-.01f,2), Time.deltaTime * 10);
            if (transform.localScale.y<=0)
            {
                Destroy(transform.parent.gameObject);
            }
        }
    }

    public float walkSpeed = 10;
    int frames;


    private void FixedUpdate()
    {
        if (EC.dead) { return; }
        frames++;
        if (frames%walkSpeed==0)
        {
            animFrame++;
            transform.localScale = new Vector3(1.1f,.9f,1.1f);
            if (animFrame==walkFrames.Length)
            {
                animFrame = 0;
            }
        }
        MF.mesh = walkFrames[animFrame];
    }
}
