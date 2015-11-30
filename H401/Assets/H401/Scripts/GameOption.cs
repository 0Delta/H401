using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;


public class GameOption : MonoBehaviour {

    [SerializeField]private GameObject panelPrefab = null;
    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;
    private GameObject panelObject = null;

    private _ePauseState _pauseState;
    public _ePauseState pauseState{ get{return _pauseState;}}
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame

 
	void Update () {

	}
 


    public void StartOption()
    {
        //時間を止める
        Time.timeScale = 0.0f;

        _pauseState = _ePauseState.PAUSE;
        
        //オプションボタンをノンアクに
        gameObject.GetComponentInChildren<Button>().interactable = false;

        //パネル生成
        Transform trans = gameObject.transform.FindChild("PauseCanvas").transform;
        panelObject = (GameObject)Instantiate(panelPrefab, trans.position, trans.rotation);
        


        //tweenによる出現演出
        //とりあえずフィールド変更と同じにしておく

        //ただし、タイムスケールに左右されない
        panelObject.transform.SetParent(gameObject.transform.FindChild("PauseCanvas").transform);
        panelObject.transform.localScale = new Vector3(popScale, popScale, popScale);

 
        panelObject.transform.DOScale(1.0f, popTime).SetUpdate(true);

        //ここでもう終了時処理の設定をしておく
        panelObject.GetComponentInChildren<Button>().onClick.AddListener( EndOption);
    }

    public void EndOption()
    {
        Time.timeScale = 1.0f;
        panelObject.transform.DOScale(popScale, popTime)
            .OnComplete(() =>
            {
                gameObject.GetComponentInChildren<Button>().interactable = true;
                Destroy(panelObject);

                _pauseState = _ePauseState.GAME;
            });
        
    }

}
