using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {

    [SerializeField]private float lyingDeviceAngle = 0.0f;     //デバイスを横と判定する角度範囲
    [SerializeField]private string levelCanvasPath= null;
   
    private LevelTables levelTableScript = null;
    private GameController gameController = null;
    private GameObject levelCanvasObject = null;
    private GameObject levelCanvasPrefab = null;

    //ボタンのスクリプト
    //ボタン実体
    private _eLevelState levelState;
    public _eLevelState LevelState { get { return levelState; } set { levelState = value; } }




    private GameObject panelObject;
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
                if(Input.gyro.attitude.eulerAngles.y > 90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < 90 + lyingDeviceAngle)
                {
                    lyingAngle = 90;
                    FieldChangeStart();
                }
                if(Input.gyro.attitude.eulerAngles.y > -90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < -90 + lyingDeviceAngle)
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
                if(!isDebug && Input.gyro.attitude.eulerAngles.y < lyingAngle  && Input.gyro.attitude.eulerAngles.y > -lyingAngle)
                {
                    lyingAngle = 0;
                    FieldChangeEnd();
                }
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
        levelState = _eLevelState.LIE;
    }

    //切り替え終了
    public void FieldChangeEnd()
    {

        lyingAngle = 0;
        //オブジェクト破棄
        //小さくなって消えるように
        panelScript.Delete(gameController.GetComponentInChildren<NodeController>());

    }

    public string GetFieldName(int stage)
    {
        return levelTableScript.GetFieldLevel(stage).fieldName;
    }
    
}
