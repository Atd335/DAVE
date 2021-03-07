using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localRotation = Quaternion.Euler(-90,Random.Range(0,360),0);
        GetComponent<AudioSource>().pitch += Random.Range(-.3f,.3f);
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale,Vector3.one * .41f,Time.deltaTime * 15);
        GetComponent<MeshRenderer>().material.SetColor("_Color", Color.Lerp(GetComponent<MeshRenderer>().material.color,new Color(1,1,1,0),Time.deltaTime * 15));
        if (transform.localScale.x>.4f)
        {
            Destroy(transform.parent.gameObject);
        }
    }
}
