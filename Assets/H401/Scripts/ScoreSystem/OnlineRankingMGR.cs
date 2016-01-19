using DG.Tweening;
using RankingExtension;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class OnlineRankingMGR : MonoBehaviour {

    [SerializeField]
    public string ScorePrefabName = null;

    [SerializeField, Range(0.01f, 0.8f)]
    public float TweenSpeed = 0.05f;

    private const float Radius = 40f;                                   // スコアの位置を決定する円の半径と中心
    private Vector3 CenterPoint = new Vector3(-5f, 0f, Radius * 5f);
    private List<Vector3> ScorePosList = new List<Vector3>();           // スコアの位置リストとスコアオブジェクトの位置
    private List<GameObject> ScoreList = new List<GameObject>();
    private int RingPos = 0;                                            // 現在のスコア位置

    // Use this for initialization
    private void Start()
    {
        // 円形に座標を設定
        Vector3 Pos;
        int n = 0;
        while ((n / 7f) < Mathf.PI)
        {
            Pos = CenterPoint;
            Pos.z -= Radius * (Mathf.Sin(n / 7f) * 4f);
            Pos.y -= Radius * (Mathf.Cos(n / 7f));
            Pos.x += (Pos.y * Pos.y / 50f);

            ScorePosList.Add(Pos);
            n++;
        }

        // 設定された座標にスコアを表示
        for (int i = 0; i < ScorePosList.Count; ++i)
        {
            GameObject obj = this.InstantiateChild(ScorePrefabName);
            ScoreList.Add(obj);
            obj.GetComponent<Transform>().localPosition = CenterPoint;
        }

        // 実際の座標を移動させる処理
        this.UpdateAsObservable()
            .Select(_ => RingPos)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                int x = 0;
                foreach (var it in ScoreList)
                {
                    Vector3 TargetPos = ScorePosList[((RingPos + x) >= ScorePosList.Count ? (RingPos + x) - ScorePosList.Count : (RingPos + x))];
                    it.GetComponent<Transform>().DOLocalMove(TargetPos, TweenSpeed);
                    x++;
                }
            }).AddTo(this);

        // オンラインとオフラインを切り替える時の処理
        this.UpdateAsObservable()
            .Select(_ => GetComponentInParent<RankingMGR>().Mode)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                if (x == RankingMGR.RANKING_MODE.OFFLINE)
                {
                    foreach (var it in ScoreList)
                    {
                        it.GetComponent<Transform>().DOLocalMove(CenterPoint, TweenSpeed);
                        it.GetComponent<Transform>().DOScale(0, TweenSpeed);
                    }
                }
                else if ((x == RankingMGR.RANKING_MODE.ONLINE))
                {
                    SetPosition();
                }
            }).AddTo(this);
    }

    private void SetPosition() {
        // 設定された座標にスコアを表示
        int x = 0;
        foreach(var it in ScoreList) {
            Vector3 TargetPos = ScorePosList[((RingPos + x) >= ScorePosList.Count ? (RingPos + x) - ScorePosList.Count : (RingPos + x))];
            it.GetComponent<Transform>().DOLocalMove(TargetPos, TweenSpeed);
            it.GetComponent<Transform>().DOScale(1, TweenSpeed);
            x++;
        }
    }

    // Update is called once per frame
    private void Update() {
        // テスト用移動(ポジションの切り替えのみ)
        if(GetComponentInParent<RankingMGR>().Mode == RankingMGR.RANKING_MODE.ONLINE) {
            if(Input.GetKeyDown(KeyCode.DownArrow)) {
                RingPos--;
                RingPos = (RingPos < 0 ? ScorePosList.Count : RingPos);
            }
            if(Input.GetKeyDown(KeyCode.UpArrow)) {
                RingPos++;
                RingPos = (RingPos > ScorePosList.Count ? 0 : RingPos);
            }
        }
        
    }
}