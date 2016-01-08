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

//    private GameObject panelObject;
    //private LevelPanel panelScript;
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

	// Use this for initialization
	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        //Input.gyro.enabled = true;
        //levelCanvasObject = Resources.Load<GameObject>(levelCanvasString);
        gameController = gameScene.gameController;
        nextLevel = -1;

        levelState = _eLevelState.STAND;

        levelTableScript = gameScene.levelTables;

        levelChangePrefab = Resources.Load<GameObject>(levelChangePath);
        animationPrefab = Resources.Load<GameObject>(animationPath);
	}
	
	// Update is called once per frame
	void Update () {
        //右クリックしている間マウスの移動を姿勢回転に反映させる
        /*
        if (Input.GetMouseButton(1))
        {
            currentAngle += Input.GetAxis("Mouse X");
            print(currentAngle.ToString());
        }*/

        currentAngle = 0.0f;//Input.acceleration.x * 90.0f;
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

        //else
            //levelState = _eLevelState.STAND;

        //preState = levelState;
	}

    //難易度切り替え状態へ
    public IEnumerator FieldChangeStart(FieldPop fpMethod)
    {
        //アニメーターを生成
        if (animationObject)
            Destroy(animationObject);
        animationObject = Instantiate(animationPrefab);
        animationObject.transform.SetParent(Camera.main.transform);
        yield return new WaitForSeconds(changePopTime);
        fpMethod();
        yield return new WaitForSeconds(changeEndTime);
        //アニメーションを消去
        Destroy(animationObject);
        animationObject = null;

    }
        //難易度選択をinstantiateする
        void  FCStart()
        {
                levelChangeObject = Instantiate(levelChangePrefab);//(GameObject)Instantiate(canvasPrefab, transform.position, transform.rotation);
                levelChangeObject.transform.SetParent(this.transform);
                fChangeScript = levelChangeObject.GetComponent<LevelChange>();
                fChangeScript.levelController = this;
                //panelScript = levelCanvasObject.GetComponentInChildren<LevelPanel>();
                //メインカメラをノンアクにする
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                gameScene.mainCamera.transform.Rotate(new Vector3(0.0f, 0.0f, -lyingAngle),Space.Self);
        gameScene.mainCamera.orthographic = false;
        gameScene.directionalLight.color = new Color(1.0f,1.0f,1.0f);

                gameScene.gameController.gameObject.SetActive(false);
                gameScene.gameUI.ojityanAnimator.gameObject.SetActive(false);
                gameScene.gameUI.gameInfoCanvas.gameObject.SetActive(false);
                gameScene.gameUI.gamePause.gameObject.SetActive(false);

        //animationObject.transform.Rotate(new Vector3(0.0f, 0.0f, -lyingAngle));
        levelState = _eLevelState.CHANGE;
        }

    //切り替え終了
    public IEnumerator FieldChangeEnd(FieldPop fpMethod)
    {
        if (animationObject)
            Destroy(animationObject);
        animationObject = Instantiate(animationPrefab);
        animationObject.transform.rotation = Quaternion.identity;
        animationObject.transform.SetParent(Camera.main.transform);
        levelState = _eLevelState.CHANGE;
        lyingAngle = 0;
        //オブジェクト破棄
        //小さくなって消えるように

        yield return new WaitForSeconds(changePopTime);
        fChangeScript.Delete();
        yield return new WaitForSeconds(changeEndTime);
        //アニメーションを消去
        Destroy(animationObject);
        animationObject = null;
    }

    public void EndComplete()
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

        gameScene.gameController.gameObject.SetActive(true);
        gameScene.gameUI.ojityanAnimator.gameObject.SetActive(true);
        gameScene.gameUI.gameInfoCanvas.gameObject.SetActive(true);
        gameScene.gameUI.gamePause.gameObject.SetActive(true);
        Destroy(levelChangeObject);

        if (NextLevel != -1)
        {
            gameController.nodeController.currentLevel = NextLevel;
            gameScene.directionalLight.color = levelTableScript.GetFieldLevel(nextLevel).lightColor;
        }
        else
        {
            gameScene.directionalLight.color = levelTableScript.GetFieldLevel(gameController.nodeController.currentLevel).lightColor;
            print("レベル変更なし");
        }
        LevelState = _eLevelState.STAND;
        
        Camera.main.gameObject.transform.localRotation = Quaternion.identity;
        gameScene.mainCamera.orthographic = true;
        //        animationObject.transform.rotation = Quaternion
        //ノードのemissionとdirectionalLightに干渉
    }

    public string GetFieldName(int stage)
    {
        return levelTableScript.GetFieldLevel(stage).BG_Path;
    }


    
}
