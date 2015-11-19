using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
public class GameOption : MonoBehaviour {

    [SerializeField]private GameObject panelPrefab = null;
    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;
    private GameObject panelObject = null;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame

	private float realDeltaTime;
	private float lastRealTime;
 
	void Update () {
		CalcRealDeltaTime();
	}
 
	//現実時間基準でデルタ時間を求める.
	void CalcRealDeltaTime() {
		if(lastRealTime == 0) {
			lastRealTime = Time.realtimeSinceStartup;
		}
		realDeltaTime = Time.realtimeSinceStartup - lastRealTime;
		lastRealTime = Time.realtimeSinceStartup;
	}

    public void StartOption()
    {
        //時間を止める
        Time.timeScale = 0.0f;
        
        //オプションボタンをノンアクに
        gameObject.GetComponentInChildren<Button>().interactable = false;

        //パネル生成
        Transform trans = gameObject.transform.FindChild("PauseCanvas").transform;
        panelObject = (GameObject)Instantiate(panelPrefab, trans.position, trans.rotation);
        


        //tweenによる出現演出
        //とりあえずフィールド変更と同じにしておく

        //ただし、タイムスケールに左右されない
        panelObject.transform.localScale = new Vector3(popScale, popScale, popScale);

        panelObject.transform.DOScale(1.0f, popTime).SetUpdate(true);
        panelObject.transform.parent = gameObject.transform.FindChild("PauseCanvas").transform;

        //ここでもう終了時処理の設定をしておく
        panelObject.GetComponentInChildren<Button>().onClick.AddListener(EndOption);
    }

    public void EndOption()
    {
        Time.timeScale = 1.0f;
        panelObject.transform.DOScale(popScale, popTime)
            .OnComplete(() =>
            {
                Destroy(panelObject);
            }).timeScale = 1.0f;
        //オプションボタンをノンアクに
        gameObject.GetComponentInChildren<Button>().interactable = true;
    }

}
