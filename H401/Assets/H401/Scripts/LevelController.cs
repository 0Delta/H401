using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LevelController : MonoBehaviour {

    [SerializeField,Range(0.0f,45.0f)]private float lyingDeviceAngle = 0.0f;     //デバイスを横と判定する角度範囲
    [SerializeField]private string levelCanvasPath= null;
   
    private LevelTables levelTableScript = null;
    private GameController gameController = null;
    private GameObject levelCanvasObject = null;
    private GameObject levelCanvasPrefab = null;

    private _eLevelState levelState;
    public _eLevelState LevelState { get { return levelState; } set { levelState = value; } }

    private float currentAngle = 0.0f;

//    private GameObject panelObject;
    private LevelPanel panelScript;
    private float lyingAngle;
    public float LyingAngle
    {
        get { return lyingAngle; }
    }

    private int nextLevel;  //次のレベル
    public int NextLevel { set { nextLevel = value; } get { return nextLevel; } }



    private bool isDebug;

	// Use this for initialization
	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

        //levelCanvasObject = Resources.Load<GameObject>(levelCanvasString);
        gameController = gameScene.gameController;
        nextLevel = -1;

        levelState = _eLevelState.STAND;
        Input.gyro.enabled = true;

        levelTableScript = gameScene.levelTables;

        levelCanvasPrefab = Resources.Load<GameObject>(levelCanvasPath);
	}
	
	// Update is called once per frame
	void Update () {
        //右クリックしている間マウスの移動を姿勢回転に反映させる
        if (Input.GetMouseButton(1))
        {
            currentAngle += Input.GetAxis("Mouse X");
            print(currentAngle.ToString());
        }
        //姿勢が45度以上135度以下
        switch(levelState)
        {
            case _eLevelState.STAND:
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    isDebug = true;
                    //難易度選択用オブジェクトを90度回転して
                    lyingAngle = 90;
                    FieldChangeStart();
                }
                if (Input.GetKeyDown(KeyCode.W))
                {
                    isDebug = true;
                    lyingAngle = -90;
                    FieldChangeStart();
                }
                if(currentAngle > 90 - lyingDeviceAngle && currentAngle < 90 + lyingDeviceAngle)
                {
                    lyingAngle = 90;
                    FieldChangeStart();
                }
                if(currentAngle > -90 - lyingDeviceAngle && currentAngle < -90 + lyingDeviceAngle)
                {
                    lyingAngle = -90;
                    FieldChangeStart();
                }
                break;
            case _eLevelState.LIE:
                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDebug = false;
                    lyingAngle = 0;
                    FieldChangeEnd();
                }
                if(!isDebug && currentAngle < lyingDeviceAngle  && currentAngle > -lyingDeviceAngle)
                {
                    lyingAngle = 0;
                    FieldChangeEnd();
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
    public void FieldChangeStart()
    {
        
        //難易度選択をinstantiateする

        levelCanvasObject = Instantiate(levelCanvasPrefab);//(GameObject)Instantiate(canvasPrefab, transform.position, transform.rotation);
        levelCanvasObject.transform.SetParent(this.transform);
        
        panelScript = levelCanvasObject.GetComponentInChildren<LevelPanel>();
        levelState = _eLevelState.CHANGE;
    }

    //切り替え終了
    public void FieldChangeEnd()
    {
        levelState = _eLevelState.CHANGE;
        lyingAngle = 0;
        //オブジェクト破棄
        //小さくなって消えるように
        panelScript.Delete();

    }

    public void EndComplete()
    {
        Destroy(levelCanvasObject);
        if (NextLevel != -1)
        {
            gameController.nodeController.currentLevel = NextLevel;
            GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
            gameScene.directionalLight.color = levelTableScript.GetFieldLevel(nextLevel).lightColor;
        }
        else
            print("レベル変更なし");
        LevelState = _eLevelState.STAND;

        //ノードのemissionとdirectionalLightに干渉



    }



    public string GetFieldName(int stage)
    {
        return levelTableScript.GetFieldLevel(stage).fieldName;
    }


    
}
