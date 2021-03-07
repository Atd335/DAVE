using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformInfo : MonoBehaviour
{

    public Color[] platColors;
    public int platColor;

    //0 - white
    //1 - red
    //2 - green
    //3 - yellow
    //4 - blue

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", platColors[platColor]);
       // GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(transform.localScale.x / 2, transform.localScale.y / 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDrawGizmos()
    {
        GetComponent<MeshRenderer>().material.SetColor("_Color", platColors[platColor]);
        //GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2 (transform.localScale.x/2, transform.localScale.y/2);
    }
}
