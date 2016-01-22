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

    private AudioSource audioSource;

    private bool _isPause = false;
    public bool IsPause {  get { return _isPause; } }

    private Image pinchPanel;
    public bool IsPinch
    {
        set
        {
            if (value == true)
            {
                pinchPanel.DOFade(0.2f, 0.5f).SetEase(Ease.InCubic).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                pinchPanel.DOKill();
                pinchPanel.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            }
        }
    }

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

        audioSource = GetComponent<AudioSource>();

        pinchPanel = optionCanvas.gameObject.transform.FindChild("GamePinchFade").gameObject.GetComponent<Image>();
    }

    public void StartOption()
    {
        //時間を止める
        Time.timeScale = 0.0f;

        _isPause = true;
        audioSource.Play();

        _pauseState = _ePauseState.PAUSE;
        
        //オプションボタンをノンアクに
        triggerButton.interactable = false;



        //パネル生成
        panelObject = (GameObject)Instantiate(Resources.Load<GameObject>(pausePanelPath));
        
        //tweenによる出現演出
        //とりあえずフィールド変更と同じにしておく
        //ただし、タイムスケールに左右されない
 
        panelObject.transform.SetParent(transform.FindChild("PauseCanvas").transform);
        panelObject.transform.localScale = Vector3.one;
        panelObject.transform.localPosition = new Vector3(0.0f, -1500.0f,0.0f);
        panelObject.transform.DOLocalMoveY(0.0f, popTime).SetUpdate(true).OnComplete( () =>
        {
            
            triggerButton.interactable = true;
        });
        
        /*
        panelObject.transform.position = panelObject.transform.parent.position; //addChildほしい
        panelObject.transform.localScale = new Vector3(popScale, popScale, 1.0f);
 
        panelObject.transform.DOScale(1.0f, popTime).SetUpdate(true)
            .OnComplete( () => { triggerButton.interactable = true; });
*/
        AppliController appController = transform.root.gameObject.GetComponent<AppliController>();
        Button[] butttons = panelObject.transform.GetComponentsInChildren<Button>();
        butttons[0].onClick.AddListener(() => { Time.timeScale = 1.0f; appController.ChangeScene(AppliController._eSceneID.GAME,0.5f,0.5f); audioSource.Play(); });
        butttons[1].onClick.AddListener(() => { Time.timeScale = 1.0f; appController.ChangeScene(AppliController._eSceneID.TITLE,1.0f,1.0f); audioSource.Play(); });

        //ステージ遷移ボタンをノンアクに
        GameScene gameScene = appController.GetCurrentScene().GetComponent<GameScene>();
        gameScene.gameUI.gameInfoCanvas.stageSelectButton.interactable = false;

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
        //panelObject.transform.DOScale(popScale, popTime)

        audioSource.Play();

        panelObject.transform.DOLocalMoveY(1500.0f,popTime)
            .OnComplete(() =>
            {
                
                triggerButton.interactable = true;
                Destroy(panelObject);
                GameScene gameScene = transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                gameScene.gameUI.gameInfoCanvas.stageSelectButton.interactable = true;
                _pauseState = _ePauseState.GAME;
                _isPause = false;
            });

    }
}
