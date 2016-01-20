using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class GameInfoCanvas : MonoBehaviour {

    private Score _score;
    public Score score { get { return _score; } }
    private LimitTime _limitTime;
    public LimitTime limitTime { get { return _limitTime; } }
    private FeverGauge _feverGauge;
    public FeverGauge feverGauge {get {return _feverGauge;}}
    [HideInInspector]public int levelImageNum { set { levelText.text = levelStrings[value]; } }

    private Text levelText;
    [SerializeField]private string[] levelStrings;

	// Use this for initialization
	void Start () {
        GetComponent<Canvas>().worldCamera = Camera.main;

        _score = GetComponentInChildren<Score>();
        _limitTime = GetComponentInChildren<LimitTime>();
        _feverGauge = GetComponentInChildren<FeverGauge>();

        levelText = transform.FindChild("GameInfoPanel").FindChild("LevelText").GetComponent<Text>();
        levelText.text = "Very Easy";
	}
}
