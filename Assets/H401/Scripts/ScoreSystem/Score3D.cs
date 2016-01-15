using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Score3D : MonoBehaviour {
    public static float AlphaOneZ = 80;
    public static float AlphaZeroZ = 120;

    Transform trans;
    // Use this for initialization
    void Start() {
        trans = GetComponent<Transform>();

        this.UpdateAsObservable()
            .Select(_ => AlphaOneZ + trans.localPosition.z + AlphaZeroZ)
            .DistinctUntilChanged()
            .Subscribe(_ => { ColorSeter(); }).AddTo(this);

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
