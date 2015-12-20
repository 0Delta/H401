using UnityEngine;
using System.Collections;

public class TitleCanvas : MonoBehaviour {
    
    [SerializeField] private string titleButtonsPath;
    
    private GameObject titleButtonsObject;

	// Use this for initialization
	void Start () {
        transform.GetComponent<Canvas>().worldCamera = Camera.main;

	    titleButtonsObject = Instantiate(Resources.Load<GameObject>(titleButtonsPath));
        titleButtonsObject.transform.SetParent(transform);
        titleButtonsObject.transform.localPosition = Vector3.zero;
        titleButtonsObject.transform.localScale = Vector3.one;
    }
}
