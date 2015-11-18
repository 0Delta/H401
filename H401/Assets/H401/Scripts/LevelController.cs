using UnityEngine;
using System.Collections;

public class LevelController : MonoBehaviour {

    [SerializeField]private float lyingDeviceAngle;     //デバイスを横と判定する角度範囲

    [SerializeField]private GameObject gameController = null;

    //ボタンのスクリプト
    //ボタン実体
    private _eLevelState levelState;
    private _eLevelState preState;

    [SerializeField] private GameObject mapPrefab;
    private GameObject mapObject;
    private LevelMap mapScript;
    private float lyingAngle;
    public float LyingAngle
    {
        get { return lyingAngle; }
    }

    private int nextLevel;  //次のレベル
    public int NextLevel { set { nextLevel = value; } }

	// Use this for initialization
	void Start () {
        nextLevel = -1;

        levelState = _eLevelState.STAND;
        preState = _eLevelState.STAND;
        Input.gyro.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
        preState = levelState;
        //姿勢が45度以上135度以下
        switch(levelState)
        {
            case _eLevelState.STAND:
                if (Input.GetKeyDown(KeyCode.Q) || (Input.gyro.attitude.eulerAngles.y > 90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < 90 + lyingDeviceAngle))
                {
                    //難易度選択用オブジェクトを90度回転して
                    lyingAngle = 90;
                    levelState = _eLevelState.LIE;
                }
                if (Input.GetKeyDown(KeyCode.W) || (Input.gyro.attitude.eulerAngles.y > -90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < -90 + lyingDeviceAngle))
                {

                    lyingAngle = -90;
                    levelState = _eLevelState.LIE;
                }
                break;
            case _eLevelState.LIE:
                if (Input.GetKeyDown(KeyCode.E))// || (Input.gyro.attitude.eulerAngles.y < 90 && Input.gyro.attitude.eulerAngles.y > -90))
                {
                    lyingAngle = 0;
                    levelState = _eLevelState.STAND;
                }
                break;
        }

        //else
            //levelState = _eLevelState.STAND;

        if(preState != levelState)
        {
            switch(levelState)
            {
                case _eLevelState.STAND:
                    FieldChangeEnd();
                    break;
                case _eLevelState.LIE:

   
                    FieldChangeStart();
                    break;
            }
        }

        //preState = levelState;
	}

    //難易度切り替え状態へ
    public void FieldChangeStart()
    {
        //ゲームをノンアクに
        //gameController.SetActive(false);

        //難易度選択をinstantiateする
        Transform trans = transform;
        //trans.Rotate(new Vector3(0.0f,0.0f,lyingAngle));;
        mapObject = (GameObject)Instantiate(mapPrefab, transform.position, transform.rotation);
        mapObject.transform.parent = this.transform;
        mapScript = mapObject.GetComponent<LevelMap>();
        //mapScript.SetLevelController(this);
 //       mapObject.transform.localScale.Set(0.1f,0.1f,0.1f);
 //       mapObject.transform.DOScale(1.0f, popTime).OnComplete(() => { gameController.SetActive(false); });
    }

    //切り替え終了
    public void FieldChangeEnd()
    {
        gameController.SetActive(true);
        gameController.GetComponentInChildren<NodeController>().SetFieldLevel(nextLevel);
        lyingAngle = 0;
        levelState = _eLevelState.STAND;
        //オブジェクト破棄
        Destroy(mapObject);

    }
    public void FChangeTest(float angle)
    {
        lyingAngle = angle;
        levelState = _eLevelState.LIE;
    }
    public void FChangeEnd()
    {
        levelState = _eLevelState.STAND;
    }
    
}
