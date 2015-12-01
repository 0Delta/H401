using UnityEngine;
using System.Collections;

public class GameInfoCanvas : MonoBehaviour {


    private GameObject gameInfoPanelObject = null;

    private Score _score;
    public Score score { get { return _score; } }
    private LimitTime _limitTime;
    public LimitTime limitTime { get { return _limitTime; } }
    private FeverGauge _feverGauge;
    public FeverGauge feverGauge {get {return _feverGauge;}}



	// Use this for initialization
	void Start () {
        GetComponentInChildren<Canvas>().worldCamera = Camera.main;//transform.root.gameObject.GetComponent<AppliController>().gameScene.mainCamera;

        _score = GetComponentInChildren<Score>();
        _limitTime = GetComponentInChildren<LimitTime>();
        _feverGauge = GetComponentInChildren<FeverGauge>();

	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
