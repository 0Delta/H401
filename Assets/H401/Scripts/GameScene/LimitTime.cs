using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LimitTime : MonoBehaviour {
    
    [SerializeField] private float maxTime = 0.0f;  //時間の最大値(秒？)
    [SerializeField] private string gameEndPanelPath = null;
    public Image timeImage;
    private float nowTime;  //現在時間
    public float lastRate { get { return 1.0f - nowTime / maxTime; } }
    private float _eventRatio;   //状態ごとの時間の減り
    public float eventRatio { set { _eventRatio = value; } }

    private Animator ojityanAnimator = null;

    private TimeLevelInfo timeLevel;
    public TimeLevelInfo TimeLevel
    {
        set { timeLevel = value; }
    }

    private int nowTimeLevel;   //現在の難易度
    private float timeLevelInterval;    //時間難易度の変更感覚 
    private float startTime;
    
    private LevelTables _levelTableScript = null;
    public LevelTables levelTableScript{
        get{
            if(!_levelTableScript) {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _levelTableScript = gameScene.levelTables;
            }
            return _levelTableScript;
        }
    }

   public delegate void fdel(); 

	// Use this for initialization
	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        
        timeLevel = levelTableScript.GetTimeLevel(0);
        timeLevelInterval = levelTableScript.TimeLevelInterval;
        startTime = Time.time;

        nowTimeLevel = 0;

        ojityanAnimator = gameScene.gameUI.ojityanAnimator;
        _eventRatio = 0.0f; //開始演出が終わるまでは時間が減らない
	}
	
	// Update is called once per frame
	void Update () {
        nowTime += Time.deltaTime * timeLevel.SlipRatio * _eventRatio;

        SetImage();

        if(nowTime > maxTime)
        {
            //ここにゲームオーバー処理
            nowTime = maxTime - 0.1f;
            _eventRatio = 0.0f;
            GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

            gameScene.gameController.nodeController.SetSlideAll(true); //ノードを操作不能にする

            //スコアをもってくる
            GameInfoCanvas gInfoCanvas = GetComponentInParent<GameInfoCanvas>();
            gInfoCanvas.score.Decide();
            //ゲームオーバー時のパネルを出して、タップでリザルト画面に行く
            GameObject gameEndPanelObject = (GameObject)Instantiate(Resources.Load<GameObject>(gameEndPanelPath));
            GameEndPanel gameEndpanel = gameEndPanelObject.GetComponent<GameEndPanel>();
            gameEndpanel.transform.SetParent(gameScene.gameUI.gamePause.optionCanvas.transform);
            gameEndpanel.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            gameEndpanel.transform.localPosition = new Vector3(0.0f, 1334.0f, 0.0f);

            //ボタンでなく時間経過で勝手に移行するように
            // リザルトへ戻るボタンを設定
            fdel del = () => {
                AppliController AppliCtr = GetComponentInParent<AppliController>();
                AppliCtr.ChangeScene(AppliController._eSceneID.RANKING, 0.5f, 0.5f);
            };
            
            StartCoroutine(ToResult(del));

            gameEndpanel.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            //オプションボタンをノンアクに
            gameScene.gameUI.gamePause.optionCanvas.gameObject.GetComponentInChildren<Button>().interactable = false;
        }

        //時間経過による難易度変更処理
        if(nowTimeLevel < levelTableScript.TimeLevelCount && Time.time - startTime > timeLevelInterval * (nowTimeLevel + 1) )
        {
            nowTimeLevel++;
            timeLevel = levelTableScript.GetTimeLevel(nowTimeLevel);
        }
	}

    //枝の数と種類をもらって時間を割合で回復させる
    public void PlusTime(NodeCountInfo nodeCount)
    {
        //計算
        float tempRatio = 0.0f;
        //最大値を超えていたらカット

        //ノード３つ毎に１割増える ３分岐１つ毎に５分増える
        tempRatio = (float)nodeCount.nodes * timeLevel.RegainPerNodes  //ノード1つごとに数％
            + (float)nodeCount.cap * timeLevel.RegainPerCap                            //１パスのノード１つ毎に数％
            + (float)nodeCount.path2 * timeLevel.RegainPer2Path                        //２パスのノード１つ毎に数％
            + (float)nodeCount.path3 * timeLevel.RegainPer3Path                        //３パスのノード１つ毎に数％
            + (float)nodeCount.path3 * timeLevel.RegainPer4Path;

        //最大値以上は回復しない
        if (tempRatio > timeLevel.MaxRegainRatio)
            tempRatio = timeLevel.MaxRegainRatio;

        nowTime -= maxTime * tempRatio;
        if(nowTime < 0)
        {
            nowTime = 0.0f;
        }
    }

    private void SetImage()
    {
        timeImage.fillAmount = lastRate;
        if(ojityanAnimator.gameObject.activeSelf)
            ojityanAnimator.SetFloat("lastTime", lastRate);
    }
    
    public IEnumerator ToResult(fdel del)
    {
        yield return new WaitForSeconds(5.0f);
        del();
    }
}
