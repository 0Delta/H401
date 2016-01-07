using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    //表示用の何か
    [SerializeField] private string ScoreManagerPath;
    //計算用の何か
    //[SerializeField] private int basePoint = 0;
    //[SerializeField] private int bonusAtNodes = 0;
    //[SerializeField] private int bonusPerCap = 0;
    //[SerializeField] private int bonusPer2Path = 0;
    //[SerializeField] private int bonusPer3Path = 0;
    //[SerializeField] private int bonusPer4Path = 0;

    private ScoreInfo scoreInfo;

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

    [SerializeField] private Text scoreText;

    void Awake()
    {
        //scoreText = GameObject.Find("ScoreNum").GetComponent<Text>();
    }

	// Use this for initialization
	void Start () {
        SetScore();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //表示機構
    public void SetScore()
    {
        scoreText.text = gameScore.ToString();
        scoreInfo = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().levelTables.ScoreRatio;
    }

    //計算機構
    public int PlusScore(NodeCountInfo nodeCount)
    {

        /*int tempScore =　nodeCount.nodes * scoreInfo.BonusAtNodes;
        tempScore += nodeCount.path2 * scoreInfo.BonusPerCap;
        tempScore += nodeCount.path2 * scoreInfo.BonusPer2Path;
        tempScore += nodeCount.path3 * scoreInfo.BonusPer3Path;
        tempScore += nodeCount.path4 * scoreInfo.BonusPer4Path;
        tempScore += scoreInfo.BasePoint;
        tempScore *= nodeCount.nodes;

        int tempScore = nodeCount.nodes + scoreInfo.BasePoint;  //基礎ポイント = ノード数×ベース
        tempScore *= nodeCount.cap * scoreInfo.BonusPerCap;     //先端ノード数分の倍率
        tempScore *= nodeCount.path2 * scoreInfo.BonusPer2Path;     //先端ノード数分の倍率
        tempScore *= nodeCount.path3 * scoreInfo.BonusPer3Path;     //先端ノード数分の倍率
        tempScore *= nodeCount.path4 * scoreInfo.BonusPer4Path;     //先端ノード数分の倍率
        gameScore += tempScore;
        */
        //ツムツム方式で得点計算をしてみる
        float tempScore = 0;
        for (int i = 1; i < nodeCount.nodes + 1; i++)           //ベースポイントを100とすると、
            tempScore += scoreInfo.BasePoint * i;               //100 + 200 + 300 +  ...  + (100 * 連鎖数)

        tempScore *= (1.0f + scoreInfo.BonusPerCap * nodeCount.cap);     //各ノードごとの
        tempScore *= (1.0f + scoreInfo.BonusPer2Path * nodeCount.path2); //ボーナスポイントを
        tempScore *= (1.0f + scoreInfo.BonusPer3Path * nodeCount.path3); //ツムツムでは加算していたが、
        tempScore *= (1.0f + scoreInfo.BonusPer4Path  * nodeCount.path4); //分岐の重みを増やすために乗算に

        gameScore += (int)tempScore;
        SetScore();

        return (int)tempScore;
    }

    public void Decide() {
        // スコア登録
        GameObject obj = Resources.Load(ScoreManagerPath) as GameObject;
        if(obj == null) {
            Debug.LogError("Failed Instantiate : RankingSystem");
            return;
        }
        ScoreManager scoreManager = Instantiate(obj).GetComponent<ScoreManager>();
        if(scoreManager == null) {
            Debug.LogError("Failed GetComponent : RankingSystem");
            return;
        }
        scoreManager.AddScore(gameScore);
    }
}
