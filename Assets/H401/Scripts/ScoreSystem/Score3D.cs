using UnityEngine;
using UniRx;
using UniRx.Triggers;
using System.Collections.Generic;

public class Score3D : MonoBehaviour {
    public static float AlphaOneZ = 80;
    public static float AlphaZeroZ = 120;

    public static List<int> RankingData;
    public int Rank = -1;

    Transform trans;
    // Use this for initialization
    void Start() {
        trans = GetComponent<Transform>();

        this.UpdateAsObservable()
            .Select(_ => AlphaOneZ + trans.localPosition.z + AlphaZeroZ)
            .DistinctUntilChanged()
            .Subscribe(_ => { ColorSeter(); }).AddTo(this);

        this.UpdateAsObservable()
            .SkipWhile(_ => RankingData == null)
            .Select(x => Rank)
            .DistinctUntilChanged()
            .Subscribe(x => {
                if (RankingData.Count > x && x > 0)
                {
                    GetComponent<TextMesh>().text = RankingData[x].ToString();
                }
            }).AddTo(this);

    }

    void ColorSeter() {
        Color col = GetComponent<MeshRenderer>().material.color;
        col.a = ((trans.localPosition.z - AlphaOneZ) / (AlphaOneZ - AlphaZeroZ));
        col.a = (col.a > 1) ? 1 : col.a;
        col.a = (col.a < 0) ? 0 : col.a;
        GetComponent<MeshRenderer>().material.color = col;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
