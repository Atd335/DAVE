using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextSceneAssignerScript : MonoBehaviour
{
    public string scene;

    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("SCENESWAPPER").GetComponent<SceneSwapper>().sceneToSwapTo = scene;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
