using UnityEngine;
using System.Collections;

public class LevelCanvas : MonoBehaviour {

    [SerializeField]private Vector3 uiCameraPos;

    void Awake()
    {

    }
	// Use this for initialization
	void Start () {

        //GetComponent<Canvas>().worldCamera = Camera.main;//transform.root.gameObject.GetComponent<AppliController>().gameScene.mainCamera;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetCamera(Camera c,float rot)
    {
        GetComponent<Canvas>().worldCamera = c;
        
        //rootから位置を相対的に決める
        GameObject uiCamera = gameObject.transform.FindChild("levelUICamera").gameObject;
        uiCamera.transform.Rotate(new Vector3(0.0f, 0.0f, rot));
        uiCamera.transform.position = transform.root.position + uiCameraPos;
    }
}
