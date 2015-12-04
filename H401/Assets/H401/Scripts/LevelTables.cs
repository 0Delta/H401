using UnityEngine;
using System.Collections;

public class LevelTables : MonoBehaviour {
    [SerializeField] private FieldLevelInfo[] fieldLevelTable = null;// = new FieldLevelInfo[5];
    [SerializeField] private float timeLevelInterval = 0;    //時間難易度の変更感覚
    [SerializeField] public TimeLevelInfo[] timeLevelTable;// = new TimeLevelInfo[5];
    [SerializeField,Range(0.0f, 1.0f)]private float feverGainRatio = 0.0f;
    [SerializeField, Range(0.0f, 1.0f)]private float feverDecreaseRatio = 0.0f;
    [SerializeField]private Color[] nodeColorList = null;

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



    public float FeverGainRatio
    {
        get { return feverGainRatio;}
    }

    public float FeverDecreaseRatio
    {
        get { return feverDecreaseRatio; }
    }


	// Use this for initialization
	void Start () {
        fieldLevelCount = fieldLevelTable.Length;
        timeLevelCount = timeLevelTable.Length;

	}
	
	// Update is called once per frame
	void Update () {
	
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
}
