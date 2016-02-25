using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SetToggle : MonoBehaviour {

	// Use this for initialization
	void Start () {
        AppliController appCtrler = transform.root.gameObject.GetComponent<AppliController>();
        Toggle tog = GetComponent<Toggle>();
        tog.isOn = appCtrler.gyroEnable;

        tog.onValueChanged.AddListener(x => { appCtrler.gyroEnable = !appCtrler.gyroEnable; });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
