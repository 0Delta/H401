using UnityEngine;
using System.Collections;

public class LevelTables : MonoBehaviour {


    //難易度調整用クラス ここから各スクリプトに変数を渡す形に
    private int fieldLevelCount;        //フィールド難易度がいくつあるか？
    public int FieldLevelCount{
        get {return fieldLevelCount;}
    }
    [SerializeField] private FieldLevelInfo[] fieldLevelTable;// = new FieldLevelInfo[5];


    [SerializeField] private float maxLimitTime; //制限時間の最大値
    [SerializeField] private float timeLevelInterval;    //時間難易度の変更感覚
    public float TimeLevelInterval
    {
        get { return timeLevelInterval; }
    }

    private int timeLevelCount;        //時間難易度がいくつあるか
    public int TimeLevelCount
    {
        get { return timeLevelCount; }
    }
    [SerializeField] public TimeLevelInfo[] timeLevelTable;// = new TimeLevelInfo[5];

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
}
