using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Score : MonoBehaviour {

    //表示用の何か

    //計算用の何か
    [SerializeField] private int basePoint = 0;
    [SerializeField] private int bonusAtNodes = 0;
    [SerializeField] private int bonusPerCap = 0;
    [SerializeField] private int bonusPer2Path = 0;
    [SerializeField] private int bonusPer3Path = 0;
    [SerializeField] private string ScoreManagerPath = null;

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
    }

    //計算機構
    public void PlusScore(int nodeNum,int cap,int path2,int path3)
    {
        int tempScore = nodeNum * bonusAtNodes;
        tempScore += path2 * bonusPerCap;
        tempScore += path2 * bonusPer2Path;
        tempScore += path3 * bonusPer3Path;
        tempScore += basePoint;
        tempScore *= nodeNum;

        gameScore += tempScore;
        SetScore();
    }

    public void Decide() {
        // スコア登録
        ScoreManager scoreManager = Instantiate(Resources.Load<ScoreManager>(ScoreManagerPath));
        scoreManager.AddScore(gameScore);
    }
}
