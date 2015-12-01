using UnityEngine;
using System.Collections;

public class LevelCanvas : MonoBehaviour {

    [SerializeField]private string levelPanelString;
    private GameObject levelPanelObject;

	// Use this for initialization
	void Start () {
        levelPanelObject = Resources.Load<GameObject>(levelPanelString);

        levelPanelObject.transform.SetParent(gameObject.transform);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
