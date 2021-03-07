using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingScript : MonoBehaviour
{

    public SpriteRenderer pingSprite;
    public Transform pingCircle;
    // Start is called before the first frame update
    void Start()
    {
        foreach (PingScript p in transform.parent.GetComponentsInChildren<PingScript>())
        {
            if (p!=this)
            {
                Destroy(p.gameObject);
            }
        }

        GetComponent<AudioSource>().pitch += Random.Range(-.3f, .3f);
        GetComponent<AudioSource>().Play();
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale,Vector3.one,Time.deltaTime * 10);
        float p = (Mathf.Sin(Time.time * 15/2) / 2) + .5f;
        p *= .3f;
        float o = 1 - p;
        pingCircle.localScale = new Vector3(o,o,pingCircle.localScale.z);
        pingSprite.transform.LookAt(Camera.main.transform);

        float k = (Mathf.Sin(Time.time * 15) / 2) + .5f;
        k *= .3f;
        float l = 1 - k;

        pingSprite.transform.localScale = Vector3.one * l;
    }
}
