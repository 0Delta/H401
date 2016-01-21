using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    //表示用の何か
    [SerializeField] private string ScoreManagerPath;
    private ScoreInfo scoreInfo;
    private Text scoreText;
    private int HiScore;
    public int HIScore
    {
        set { HiScore = value; }
        get { return HiScore; }
    }

    private int gameScore;
    public int GameScore{
        set { gameScore = value; }
        get { return gameScore; }
    }

	// Use this for initialization
	void Start () {
        gameScore = 0;
        //swManager = new ScoreWordMGR();
        //swManager.Load();
        scoreText = GetComponentInChildren<Text>();
        scoreInfo = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().levelTables.ScoreRatio;
        //_scoreCanvas = null;
        SetScore();

	}

    //表示機構
    public void SetScore()
    {
        scoreText.text = gameScore.ToString();
    }

    //計算機構
    public int PlusScore(NodeCountInfo nodeCount)
    {
        //ツムツム方式で得点計算をしてみる
        float tempScore = 0;
        for (int i = 1; i < nodeCount.nodes + 1; i++)           //ベースポイントを100とすると、
            tempScore += scoreInfo.BasePoint * i;               //100 + 200 + 300 +  ...  + (100 * 連鎖数)

        tempScore *= (1.0f + scoreInfo.BonusPerCap * nodeCount.cap);     //各ノードごとの
        tempScore *= (1.0f + scoreInfo.BonusPer2Path * nodeCount.path2); //ボーナスポイントを
        tempScore *= (1.0f + scoreInfo.BonusPer3Path * nodeCount.path3); //ツムツムでは加算していたが、
        tempScore *= (1.0f + scoreInfo.BonusPer4Path  * nodeCount.path4); //分岐の重みを増やすために乗算に

        //とりあえずフィーバー中はポイント3倍点
        tempScore *= transform.parent.FindChild("FeverMask").FindChild("FeverGauge").gameObject.GetComponent<FeverGauge>().feverState == _eFeverState.FEVER ? 3 : 1;

        gameScore += (int)tempScore;
        SetScore();

        return (int)tempScore;
    }

    public int Decide() {
        // スコア登録
        GameObject obj = Resources.Load(ScoreManagerPath) as GameObject;
        if(obj == null) {
            Debug.LogError("Failed Instantiate : RankingSystem");
            return 0;
        }
        ScoreManager scoreManager = Instantiate(obj).GetComponent<ScoreManager>();
        if(scoreManager == null) {
            Debug.LogError("Failed GetComponent : RankingSystem");
            return 0;
        }
        return scoreManager.AddScore(gameScore);
    }
}
