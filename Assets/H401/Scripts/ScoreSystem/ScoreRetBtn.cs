using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreRetBtn : MonoBehaviour {

    // Use this for initialization
    void Start() {
        Button Btn = this.GetComponent<Button>();
        Btn.onClick.AddListener(() => onClick());
    }
    
    public void onClick() {
        AppliController AppliCtr = GetComponentInParent<AppliController>();
        AppliCtr.ChangeScene(AppliController._eSceneID.TITLE, 0.5f, 0.5f);
    }

    // Update is called once per frame
	void Update () {
	
	}
}
