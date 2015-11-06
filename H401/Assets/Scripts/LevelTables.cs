using UnityEngine;
using System.Collections;

public class LevelTables : MonoBehaviour {


    //難易度調整用クラス ここから各スクリプトに変数を渡す形に
    [SerializeField] public FieldLevelInfo[] fieldLevelTable = new FieldLevelInfo[5];


    [SerializeField] private float maxLimitTime; //制限時間の最大値
    [SerializeField] float timeLevelInterval;    //時間難易度の変更感覚
    public float TimeLevelInterval
    {
        get { return timeLevelInterval; }
    }
    [SerializeField] public TimeLevelInfo[] timeLevelTable = new TimeLevelInfo[5];

	// Use this for initialization
	void Start () {
	
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
