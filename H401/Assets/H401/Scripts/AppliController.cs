using UnityEngine;
using System.Collections;

public class AppliController : MonoBehaviour {

    [SerializeField]GameObject gamePrefab = null;
    private GameObject currentObject = null;
	// Use this for initialization
	void Start () {
        currentObject = (GameObject)Instantiate(gamePrefab,transform.position,transform.rotation);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
