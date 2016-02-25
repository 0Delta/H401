using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    [SerializeField,Range(0.0f,45.0f)]private float lyingDeviceAngle = 0.0f;     //デバイスを横と判定する角度範囲
    [SerializeField]private string levelChangePath= null;
    [SerializeField]private string animationPath = null;
    [SerializeField]private float changePopTime;
    [SerializeField]private float changeEndTime;
    private LevelTables levelTableScript = null;
    private GameController gameController = null;
    private GameObject levelChangeObject = null;
    private GameObject levelChangePrefab = null;
    private GameObject animationPrefab = null;
    private GameObject animationObject = null;
    private _eLevelState levelState;
    public _eLevelState LevelState { get { return levelState; } set { levelState = value; } }

    private float currentAngle = 0.0f;

    public delegate void FieldPop();             //回転再配置用のデリゲート
    
    private float lyingAngle;
    public float LyingAngle
    {
        get { return lyingAngle; }
    }

    private LevelChange fChangeScript;
    public LevelChange levelChange { get { return fChangeScript; } }

    private int nextLevel;  //次のレベル
    public int NextLevel { set { nextLevel = value; } get { return nextLevel; } }

    private bool isDebug;

    private AudioSource audioSource;

    private GameOption gOption;
    private AppliController appController;

	// Use this for initialization
	void Start () {
        appController = transform.root.gameObject.GetComponent<AppliController>();
        GameScene gameScene = appController.GetCurrentScene().GetComponent<GameScene>();
        gameController = gameScene.gameController;
        nextLevel = -1;

        audioSource = GetComponent<AudioSource>();

        levelState = _eLevelState.STAND;

        levelTableScript = gameScene.levelTables;

        levelChangePrefab = Resources.Load<GameObject>(levelChangePath);
        animationPrefab = Resources.Load<GameObject>(animationPath);

        gOption = gameScene.gameUI.gamePause;
 //       gameScene.gameUI.gameInfoCanvas.GetComponentInChildren<fieldImage>().gameObject.GetComponent<Button>().onClick.AddListener(TouchChange);

	}
	
	// Update is called once per frame
	void Update () {
        if (!appController.gyroEnable)  //ジャイロ設定がオフなら判定しない
            return;

        //ノードが１つでもアクション状態であれば判定しない
        if (gameController.nodeController.isNodeLock)
            return;

        //ポーズ中は判定しない
        if (gOption.IsPause)
            return;

        currentAngle = Input.acceleration.x * 90.0f;

        //姿勢が45度以上135度以下
        switch(levelState)
        {
            case _eLevelState.STAND:
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    isDebug = true;
                    //難易度選択用オブジェクトを90度回転して
                    lyingAngle = 90;
                    StartCoroutine(FieldChangeStart(FCStart));
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    isDebug = true;
                    lyingAngle = -90;
                    StartCoroutine(FieldChangeStart(FCStart));
                }
                if(currentAngle > 90 - lyingDeviceAngle && currentAngle < 90 + lyingDeviceAngle)
                {
                    lyingAngle = 90;
                    StartCoroutine(FieldChangeStart(FCStart));
                }
                if(currentAngle > -90 - lyingDeviceAngle && currentAngle < -90 + lyingDeviceAngle)
                {
                    lyingAngle = -90;
                    StartCoroutine(FieldChangeStart(FCStart));
                }
                break;
            case _eLevelState.LIE:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDebug = false;
                    lyingAngle = 0;
                    StartCoroutine( FieldChangeEnd(fChangeScript.Delete));
                }
                else 
                if(!isDebug && currentAngle < lyingDeviceAngle  && currentAngle > -lyingDeviceAngle)
                {
                    lyingAngle = 0;
                    StartCoroutine(FieldChangeEnd(fChangeScript.Delete));
                }
                break;
            case _eLevelState.CHANGE:
                //変更中は何もしない
                break;
        }
	}

    //難易度切り替え状態へ
    public IEnumerator FieldChangeStart(FieldPop fpMethod)
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        //アニメーターを生成
        levelState = _eLevelState.CHANGE;
        if (animationObject)
            Destroy(animationObject);
        animationObject = Instantiate(animationPrefab);
        animationObject.transform.SetParent(Camera.main.transform);

        //時間を止める
        gameScene.gameUI.gameInfoCanvas.limitTime.eventRatio = 0;

        audioSource.Play();

        //手前にあるオブジェクトを非表示
        gameScene.gameUI.ojityanAnimator.gameObject.SetActive(false);
        gameScene.gameUI.gamePause.optionButton.SetActive(false);
 
        yield return new WaitForSeconds(changePopTime);
        fpMethod();

        yield return new WaitForSeconds(changeEndTime);
        //アニメーションを消去
        Destroy(animationObject);
        animationObject = null;
        levelState = _eLevelState.LIE;
    }

    //難易度選択をinstantiateする
    void  FCStart()
    {
        levelChangeObject = Instantiate(levelChangePrefab);
        levelChangeObject.transform.SetParent(this.transform);
        fChangeScript = levelChangeObject.GetComponent<LevelChange>();
        fChangeScript.levelController = this;

        //メインカメラをノンアクにする
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        gameScene.mainCamera.transform.Rotate(new Vector3(0.0f, 0.0f, -lyingAngle),Space.Self);
        //gameScene.directionalLight.color = new Color(1.0f,1.0f,1.0f);



        //gameScene.gameController.gameObject.SetActive(false);
        gameScene.gameController.nodeController.gameObject.SetActive(false);
        gameScene.gameController.arrowControllerObject.SetActive(false);
        gameScene.gameController.frameControllerObject.SetActive(false);
        gameScene.gameUI.gameInfoCanvas.gameObject.SetActive(false);
    }

    //切り替え終了
    public IEnumerator FieldChangeEnd(FieldPop fpMethod)
    {
        if (animationObject)
            Destroy(animationObject);

        audioSource.Play();

        levelChange.levelPanel.PanelSlide();

        animationObject = Instantiate(animationPrefab);
        animationObject.transform.rotation = Quaternion.identity;
        animationObject.transform.SetParent(Camera.main.transform);
        levelState = _eLevelState.CHANGE;
        lyingAngle = 0;

        //各オブジェクトの表示復帰
       GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                

        yield return new WaitForSeconds(changePopTime);
        fChangeScript.Delete();
        gameScene.gameUI.ojityanAnimator.gameObject.SetActive(true);

        yield return new WaitForSeconds(changeEndTime);
        //アニメーションを消去
        Destroy(animationObject);
        animationObject = null;
        if (NextLevel != -1)
        {
            gameController.nodeController.currentLevel = NextLevel;
            //gameScene.directionalLight.color = levelTableScript.GetFieldLevel(nextLevel).lightColor;
        }
        else
        {
            //gameScene.directionalLight.color = levelTableScript.GetFieldLevel(gameController.nodeController.currentLevel).lightColor;
//            print("レベル変更なし");
        }
        LevelState = _eLevelState.STAND;
    }

    public void EndComplete()
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

        //gameScene.gameController.gameObject.SetActive(true);
        gameScene.gameController.nodeController.gameObject.SetActive(true);
        gameScene.gameController.arrowControllerObject.SetActive(true);
        gameScene.gameController.frameControllerObject.SetActive(true);
        gameScene.gameUI.ojityanAnimator.gameObject.SetActive(true);
        gameScene.gameUI.gameInfoCanvas.gameObject.SetActive(true);
        gameScene.gameUI.gamePause.optionButton.SetActive(true);
        //時間を戻す
        gameScene.gameUI.gameInfoCanvas.limitTime.eventRatio = 1;
        Destroy(levelChangeObject);
        
        Camera.main.gameObject.transform.localRotation = Quaternion.identity;
    }

    public string GetFieldName(int stage)
    {
        return levelTableScript.GetFieldLevel(stage).BG_Path;
    }

    public void TouchChange()
    {
     if (gameController.nodeController.isNodeLock)
                return;
        switch (LevelState)
        {
            case _eLevelState.STAND:
                isDebug = true;
                //難易度選択用オブジェクトを90度回転して
                lyingAngle = 90;
                StartCoroutine(FieldChangeStart(FCStart));
                break;
            case _eLevelState.LIE:
                isDebug = false;
                lyingAngle = 0;
                StartCoroutine(FieldChangeEnd(fChangeScript.Delete));
                break;

            case _eLevelState.CHANGE:
                //変更中は何もしない
                break;
        }

    }
}
