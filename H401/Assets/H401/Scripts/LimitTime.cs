using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LimitTime : MonoBehaviour {


    [SerializeField] private float maxTime = 0.0f;  //時間の最大値(秒？)

    public Image timeImage;
    private float nowTime;  //現在時間


    private Animator ojityanAnimator = null;

    private TimeLevelInfo timeLevel;
    public TimeLevelInfo TimeLevel
    {
        set { timeLevel = value; }
    }

    private int nowTimeLevel;   //現在の難易度

    private float timeLevelInterval;    //時間難易度の変更感覚 

    private float startTime;

//    [SerializeField] private GameObject levelTableObject = null;

    private LevelTables _levelTableScript = null;
    public LevelTables levelTableScript{
        get{
            if(!_levelTableScript)
                _levelTableScript = transform.root.gameObject.GetComponent<AppliController>().gameScene.levelTables;
            return _levelTableScript;
        }
    }

	// Use this for initialization
	void Start () {

        //levelTableScript = transform.root.gameObject.GetComponent<AppliController>().gameScene.levelTables;

        timeLevel = levelTableScript.GetTimeLevel(0);
        timeLevelInterval = levelTableScript.TimeLevelInterval;
        startTime = Time.time;

        nowTimeLevel = 0;

        ojityanAnimator = transform.root.gameObject.GetComponent<AppliController>().gameScene.gameUI.ojityanAnimator;
	}
	
	// Update is called once per frame
	void Update () {
        nowTime += Time.deltaTime * timeLevel.SlipRatio;

        SetImage();

        if(nowTime > maxTime)
        {
            //ここにゲームオーバー処理

            //スコアをもってくる
            //ゲームオーバー時のパネルを出して、タップでリザルト画面に行く
            Resources.Load("GameOverPanel");

            print("タイムオーバー");
        }

        //時間経過による難易度変更処理
        if(nowTimeLevel < levelTableScript.TimeLevelCount && Time.time - startTime > timeLevelInterval * (nowTimeLevel + 1) )
        {
            nowTimeLevel++;
            timeLevel = levelTableScript.GetTimeLevel(nowTimeLevel);
            print("時間レベル変更：" + nowTimeLevel.ToString());
        }


	}

    //枝の数と種類をもらって時間を割合で回復させる
    public void PlusTime(int nodeNum, int cap, int path2, int path3)
    {
        //計算
        float tempRatio = 0.0f;
        //最大値を超えていたらカット

        //ノード３つ毎に１割増える ３分岐１つ毎に５分増える
        tempRatio = (float)(nodeNum / 3) * timeLevel.RegainPer3Nodes  //ノード３つごとに数％
            + cap * timeLevel.RegainPerCap                            //１パスのノード１つ毎に数％
            + path2 * timeLevel.RegainPer2Path                        //２パスのノード１つ毎に数％
            + path3 * timeLevel.RegainPer3Path;                       //３パスのノード１つ毎に数％

        if (tempRatio > timeLevel.MaxRegainRatio)
            tempRatio = timeLevel.MaxRegainRatio;

        nowTime -= maxTime * tempRatio;
        if(nowTime < 0)
        {
            nowTime = 0.0f;
        }

        //
    }


    private void SetImage()
    {
        float lastRate = 1.0f - nowTime / maxTime;

        timeImage.fillAmount = lastRate;

        ojityanAnimator.SetFloat("lastTime", lastRate);
    }
}
