using UnityEngine;
using System.Collections;

public class LevelTables : MonoBehaviour {

    [SerializeField] private FieldLevelInfo[] fieldLevelTable = null;
    [SerializeField] private float timeLevelInterval = 0;    //時間難易度の変更感覚
    [SerializeField] public TimeLevelInfo[] timeLevelTable;
    [SerializeField]private Color[] nodeColorList = null;
    [SerializeField]private ScoreInfo scoreRatio;
    [SerializeField]private FeverInfo feverRatio;
    [SerializeField]private float feverSpeedInterval = 0.0f;
    [SerializeField]private float[] feverSpeedTable;

    //難易度調整用クラス ここから各スクリプトに変数を渡す形に
    private int fieldLevelCount;        //フィールド難易度がいくつあるか？
    public int FieldLevelCount{
        get {return fieldLevelCount;}
    }

    public float TimeLevelInterval
    {
        get { return timeLevelInterval; }
    }

    private int timeLevelCount;        //時間難易度がいくつあるか
    public int TimeLevelCount
    {
        get { return timeLevelCount; }
    }

    public ScoreInfo ScoreRatio { get { return scoreRatio; } }

    public FeverInfo FeverRatio
    {
        get { return feverRatio;}
    }
    private int _feverSpeedCount;
    public int feverSpeedCount
    {
        get { return _feverSpeedCount; }
    }
    public float FeverSpeedInterval
    {
        get { return feverSpeedInterval; }
    }
    // Use this for initialization
    void Start () {
        fieldLevelCount = fieldLevelTable.Length;
        timeLevelCount = timeLevelTable.Length;
        _feverSpeedCount = feverSpeedTable.Length;
	}

    public TimeLevelInfo GetTimeLevel(int i)
    {
        return timeLevelTable[i];
    }
    public FieldLevelInfo GetFieldLevel(int i)
    {
        return fieldLevelTable[i];
    }
    public Color GetNodeColor(int i)
    {
        return nodeColorList[i];
    }

    public float GetFeverSpeedInfo(int i)
    {
        return feverSpeedTable[i];
    }
}
