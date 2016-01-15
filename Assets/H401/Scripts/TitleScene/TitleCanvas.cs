using UnityEngine;
using System.Collections;

public class TitleCanvas : MonoBehaviour {
    
    [SerializeField] private string titleLogosPath;
    [SerializeField] private string titleBacksPath;
    [SerializeField] private string titleButtonsPath;
    
    private GameObject titleLogosObject;
    private GameObject titleBacksObject;
    private GameObject titleButtonsObject;

	// Use this for initialization
	void Start () {
        transform.GetComponent<Canvas>().worldCamera = Camera.main;

	    titleLogosObject = Instantiate(Resources.Load<GameObject>(titleLogosPath));
        titleLogosObject.transform.SetParent(transform);
        titleLogosObject.transform.localPosition = Vector3.zero;
        titleLogosObject.transform.localScale = Vector3.one;

        titleBacksObject = Instantiate(Resources.Load<GameObject>(titleBacksPath));
        titleBacksObject.transform.SetParent(transform);
        titleBacksObject.transform.localPosition = Vector3.zero;
        titleBacksObject.transform.localScale = Vector3.one;

        titleButtonsObject = Instantiate(Resources.Load<GameObject>(titleButtonsPath));
        titleButtonsObject.transform.SetParent(transform);
        titleButtonsObject.transform.localPosition = Vector3.zero;
        titleButtonsObject.transform.localScale = Vector3.one;
    }
    
    // タイトルシーンで使用している UI をアクティブにする
    public void EnableTitleUI() {
        titleLogosObject.SetActive(true);
        titleButtonsObject.SetActive(true);
    }

    // タイトルシーンで使用している UI を非アクティブにする
    public void DisableTitleUI() {
        titleLogosObject.SetActive(false);
        titleButtonsObject.SetActive(false);
    }

    public void InitButtonsTransform() {
        titleButtonsObject.GetComponent<TitleButtons>().InitButtonsTransform();
    }
}
