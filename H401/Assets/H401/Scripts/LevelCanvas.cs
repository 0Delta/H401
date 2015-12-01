using UnityEngine;
using System.Collections;

public class LevelCanvas : MonoBehaviour {


    void Awake()
    {

    }
	// Use this for initialization
	void Start () {

        GetComponent<Canvas>().worldCamera = Camera.main;//transform.root.gameObject.GetComponent<AppliController>().gameScene.mainCamera;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
