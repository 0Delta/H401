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
    public void PlusScore(nodeCountInfo nodeCount)
    {
        int tempScore = nodeCount.nodes * scoreInfo.BonusAtNodes;
        tempScore += nodeCount.path2 * scoreInfo.BonusPerCap;
        tempScore += nodeCount.path2 * scoreInfo.BonusPer2Path;
        tempScore += nodeCount.path3 * scoreInfo.BonusPer3Path;
        tempScore += nodeCount.path4 * scoreInfo.BonusPer4Path;
        tempScore += scoreInfo.BasePoint;
        tempScore *= nodeCount.nodes;

        gameScore += tempScore;
        SetScore();
    }

    public void Decide() {
        // スコア登録
        ScoreManager scoreManager = Instantiate(Resources.Load<ScoreManager>(ScoreManagerPath));
        scoreManager.AddScore(gameScore);
    }
}
