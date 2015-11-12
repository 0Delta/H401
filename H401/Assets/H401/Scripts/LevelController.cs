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

    private int nextLevel;  //次のレベル
    public int NextLevel { set { nextLevel = value; } }

	// Use this for initialization
	void Start () {
        levelState = _eLevelState.STAND;
        preState = _eLevelState.STAND;
        Input.gyro.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {

        //姿勢が45度以上135度以下
        if (Input.gyro.attitude.eulerAngles.y > 90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < 90 + lyingDeviceAngle)
        {
            //難易度選択用オブジェクトを90度回転して
            lyingAngle = 90;
            levelState = _eLevelState.LIE;
        }
        else if (Input.gyro.attitude.eulerAngles.y > -90 - lyingDeviceAngle && Input.gyro.attitude.eulerAngles.y < -90 + lyingDeviceAngle)
        {

            lyingAngle = -90;
            levelState = _eLevelState.LIE;
        }
        else
            levelState = _eLevelState.STAND;

        if(preState != levelState)
        {
            switch(levelState)
            {
                case _eLevelState.STAND:

                    break;
                case _eLevelState.LIE:
                    FieldChangeStart();
                    break;
            }
        }

        preState = levelState;
	}

    //難易度切り替え状態へ
    public void FieldChangeStart()
    {
        //ゲームをノンアクに
        gameController.SetActive(false);

        //難易度選択をinstantiateする
        mapObject = new GameObject();
        mapObject = (GameObject)Instantiate(mapPrefab, transform.position, transform.rotation);
        mapObject.transform.parent = this.transform;
        mapScript = mapObject.GetComponent<LevelMap>();
        mapScript.SetLevelController(this);
    }

    //切り替え終了
    public void FieldChangeEnd()
    {
        gameController.SetActive(true);
        gameController.GetComponent<NodeController>().SetFieldLevel(nextLevel);

        //オブジェクト破棄
        Destroy(mapObject);

    }

    
}
