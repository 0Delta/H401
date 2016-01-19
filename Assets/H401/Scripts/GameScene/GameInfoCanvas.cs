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
    public int levelImageNum { set { levelImage.sprite = levelSprites[value]; } }

    private Image levelImage;
    [SerializeField]private Sprite[] levelSprites;

	// Use this for initialization
	void Start () {
        GetComponent<Canvas>().worldCamera = Camera.main;

        _score = GetComponentInChildren<Score>();
        _limitTime = GetComponentInChildren<LimitTime>();
        _feverGauge = GetComponentInChildren<FeverGauge>();

        levelImage = transform.FindChild("GameInfoPanel").FindChild("LevelImage").GetComponent<Image>();
        levelImageNum = 0;
	}
}
