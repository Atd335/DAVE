using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickUpScript : MonoBehaviour
{
    public IntObjScript OBJ;
    // Start is called before the first frame update
    void Start()
    {
        OBJ = GetComponent<IntObjScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (OBJ.interactMe)
        {
            PickUpToggle();
            OBJ.interactMe = false;
        }
    }

    public void PickUpToggle()
    {
        if (!MovementScript.holdingItem)
        {
            GetComponent<Collider>().enabled = false;
            transform.parent = Camera.main.transform;
            transform.localPosition = new Vector3(0, -.6f, 2f);
            MovementScript.heldObj = this.gameObject;
            MovementScript.holdingItem = true;
        }
        else
        {
            GetComponent<Collider>().enabled = true;
            transform.parent = GameObject.Find("INTERACTABLEOBJECTS").transform;
            MovementScript.heldObj = null;
            MovementScript.holdingItem = false;
        }
    }
}
