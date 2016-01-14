using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;


public class GameOption : MonoBehaviour {

    [SerializeField]private string pausePanelPath = null;
    [SerializeField]private string gameStartPanelPath = null;
    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;

    private GameObject panelObject = null;

    private _ePauseState _pauseState;
    public _ePauseState pauseState{ get{return _pauseState;}}
    [HideInInspector] public Canvas optionCanvas = null;
    [HideInInspector] public GameObject optionButton = null;
    private Button triggerButton;

	// Use this for initialization
    void Start()
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

        optionCanvas = GetComponentInChildren<Canvas>();
        optionCanvas.worldCamera= gameScene.mainCamera;
        optionCanvas.sortingLayerName = "Option"; 

        GameObject sPanel = Instantiate(Resources.Load<GameObject>(gameStartPanelPath));
        sPanel.transform.SetParent(optionCanvas.transform);
        sPanel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        optionButton = optionCanvas.gameObject.transform.FindChild("Option").gameObject;

        triggerButton = GetComponentInChildren<Button>();
        triggerButton.onClick.AddListener(StartOption);
    }

    public void StartOption()
    {
        //時間を止める
        Time.timeScale = 0.0f;

        _pauseState = _ePauseState.PAUSE;
        
        //オプションボタンをノンアクに
        triggerButton.interactable = false;

        //パネル生成
        panelObject = (GameObject)Instantiate(Resources.Load<GameObject>(pausePanelPath));
        
        //tweenによる出現演出
        //とりあえずフィールド変更と同じにしておく
        //ただし、タイムスケールに左右されない
 
        panelObject.transform.SetParent(transform.FindChild("PauseCanvas").transform);
        panelObject.transform.localPosition = panelObject.transform.parent.position; //addChildほしい
        panelObject.transform.localScale = new Vector3(popScale, popScale, 1.0f);
 
        panelObject.transform.DOScale(1.0f, popTime).SetUpdate(true)
            .OnComplete( () => { triggerButton.interactable = true; });

        //ここでもう終了時処理の設定をしておく
        triggerButton.onClick.RemoveAllListeners();
        triggerButton.onClick.AddListener(EndOption);

    }

    public void EndOption()
    {
        Time.timeScale = 1.0f;
        triggerButton.interactable = false;
        triggerButton.onClick.RemoveAllListeners();
        triggerButton.onClick.AddListener(StartOption);
        panelObject.transform.DOScale(popScale, popTime)
            .OnComplete(() =>
            {
                triggerButton.interactable = true;
                Destroy(panelObject);

                _pauseState = _ePauseState.GAME;

            });

    }
}
