using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGANIM : MonoBehaviour
{
    public Sprite[] animFrames;
    int fr;
    public SpriteRenderer SR;
    public int spd;
    // Start is called before the first frame update
    void Start()
    {
        SR = GetComponentInChildren<SpriteRenderer>();
    }

    int frames;

    // Update is called once per frame
    void FixedUpdate()
    {
        frames++;
        if (frames%spd==0)
        {
            fr++;
            if (fr==animFrames.Length)
            {
                fr = 0;
            }
        }
        SR.sprite = animFrames[fr];
    }
}
