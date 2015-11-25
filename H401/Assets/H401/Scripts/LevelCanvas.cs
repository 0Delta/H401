using UnityEngine;
using System.Collections;

public class LevelCanvas : MonoBehaviour {

    private Canvas canvas;
	// Use this for initialization
	void Start () {
        canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
