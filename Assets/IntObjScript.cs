using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntObjScript : MonoBehaviour
{
    public int objID;

    // Start is called before the first frame update
    void Awake()
    {
        for (int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetComponentsInChildren<IntObjScript>()[i] == this)
            {
                objID = i;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool interactMe;

    public void INTERACT()
    {
        interactMe = true;
    }
}
