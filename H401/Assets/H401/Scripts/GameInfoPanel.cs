using UnityEngine;
using System.Collections;

public class GameInfoPanel : MonoBehaviour {

    private FeverGauge _feverGauge = null;
    private LimitTime _limitTime = null;
    private Score _score = null;

    public FeverGauge feverGauge { get { return _feverGauge; } }
    public LimitTime limitTime {get{return _limitTime;}}
    public Score score { get { return _score; } }

	// Use this for initialization
	void Start () {
        _feverGauge = GetComponentInChildren<FeverGauge>();
        _limitTime = GetComponentInChildren<LimitTime>();
        _score = GetComponentInChildren<Score>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
