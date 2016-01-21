using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

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
    private int preGainScore;       //前回獲得スコア（フィーバー倍率なし）
    [SerializeField]private string popScoreTextPath = null;
    private GameObject popScoreTextObject = null;
    private FeverGauge _feverGauge;
    private FeverGauge feverGauge {
        get {
            if (_feverGauge == null)
                _feverGauge = transform.parent.FindChild("FeverMask").FindChild("FeverGauge").gameObject.GetComponent<FeverGauge>();

            return _feverGauge;
        }
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
        popScoreTextObject = Resources.Load<GameObject>(popScoreTextPath);
        
	}

    public void PopScoreText(Vector3 popPos)
    {
        GameObject psText = Instantiate<GameObject>(popScoreTextObject);

        psText.GetComponentInChildren<Text>().text = preGainScore.ToString();
        psText.transform.SetParent(transform);
        psText.transform.localScale = new Vector3(3.0f, 3.0f,3.0f);
        psText.transform.position = popPos;

        if (feverGauge.feverState == _eFeverState.FEVER)
            psText.transform.FindChild("SanbaiIceCream").gameObject.SetActive(true);
        
        psText.transform.DOLocalMove(psText.transform.localPosition + new Vector3(0.0f,50.0f,0.0f), 0.5f).SetEase(Ease.OutCirc)
            .OnComplete(() => {
                Destroy(psText);
            });

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

        preGainScore = (int)tempScore;
        //とりあえずフィーバー中はポイント3倍点
        tempScore *= feverGauge.feverState == _eFeverState.FEVER ? 3 : 1;

        gameScore += (int)tempScore;
        SetScore();

        return (int)tempScore;
    }

    public int Decide(Transform Pear) {
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
        scoreManager.transform.SetParent(Pear);
        return scoreManager.AddScore(gameScore);
    }
}
