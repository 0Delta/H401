using UnityEngine;
using System.Collections;

public class TitleBGCanvas : MonoBehaviour {
    
    [SerializeField] private string titleBacksPath;
    [SerializeField] private float adjustObjScale;
    
    private GameObject titleBacksObject;

	// Use this for initialization
	void Start () {
        titleBacksObject = Instantiate(Resources.Load<GameObject>(titleBacksPath));
        titleBacksObject.transform.SetParent(transform);
        titleBacksObject.transform.localPosition = Vector3.zero;
        titleBacksObject.transform.localScale = new Vector3(adjustObjScale, adjustObjScale, 1.0f);
    }
}
