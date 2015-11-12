using UnityEngine;
using System.Collections;

public class OptionButton : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick()
    {
        Time.timeScale = 0;
        //ゲーム本編を非表示

        //オプションのシーンをロード
        //Application.LoadLevelAdditive("GameOption");
    }
}
