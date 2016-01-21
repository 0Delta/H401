#pragma warning disable 414
using UnityEngine;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using System;
using System.Collections.Generic;

namespace RankingExtension
{
    public static class InstantiateEx
    {
        // ゲームオブジェクトを生成して、子に登録する関数
        public static GameObject InstantiateChild(this MonoBehaviour Mono, string Name, bool WorldPositionStays = true)
        {
            GameObject ret = null;
            string Pass = "";
            try
            {
                RankingMGR mgr = Mono.GetComponentInParent<RankingMGR>();
                if (mgr.RankingPrefabFolderName != "")
                {
                    Pass += mgr.RankingPrefabFolderName + "/";
                }
            }
            catch { return null; }
            Pass += Name;

            try
            {
                ret = MonoBehaviour.Instantiate(Resources.Load(Pass)) as GameObject;
                ret.transform.SetParent(Mono.transform, WorldPositionStays);
            }
            catch { ret = null; }
            return ret;
        }
    }
}

public class RankingMGR : MonoBehaviour {

    [SerializeField] public string RankingPrefabFolderName = null;
    [SerializeField] public string OnlinePrefabName = null;
    [SerializeField] public string OfflinePrefabName = null;
    [SerializeField] public string ScorePrefabName = null;
    [SerializeField] public string CameraPrefabName = null;
    [SerializeField] public string ReturnButtonName = null;
    [SerializeField] public List<string> BGPrefabNameList = null;
    [SerializeField] public Vector3 BGPosition = new Vector3(0f, 0f, 0f);
    [SerializeField] private Color filterColor = new Color(0f, 0f, 0f);
    private Color filterColorBak = new Color(0f, 0f, 0f);

    public GameObject ScoreObj = null;
    GameObject OnlineObj = null;
    GameObject OfflineObj = null;
    GameObject CameraObj = null;
    GameObject ReturnBtnObj = null;
    List<GameObject> BGObjList = new List<GameObject>();
    ScoreWordMGR Sword = new ScoreWordMGR();
    private bool FlipRanking = false;
    public enum RANKING_MODE : byte{
        OFFLINE = 0,
        ONLINE = 1
    };
    public RANKING_MODE Mode = RANKING_MODE.OFFLINE;

    /// <summary>
    /// ゲームオブジェクトを生成して、子に登録する関数
    /// </summary>
    /// <param name="Name">オブジェクト名</param>
    /// <param name="WorldPositionStays">座標固定</param>
    /// <returns>失敗した場合NULL</returns>
    private GameObject InstantiateChild(string Name, bool WorldPositionStays = true) {
        GameObject ret = null;
        string Pass = "";
        if(RankingPrefabFolderName != "") {
            Pass += RankingPrefabFolderName + "/";
        }
        Pass += Name;

        try {
            ret = Instantiate(Resources.Load(Pass)) as GameObject;
            ret.transform.SetParent(transform, WorldPositionStays);
        }
        catch(Exception excep) {
            ret = null;
            Debug.LogError("Exception ! : " + Pass);
            Debug.LogException(excep);
        }
        return ret;
    }

    // Use this for initialization
    void Start() {
        // オブジェクト初期化
        ScoreObj = InstantiateChild(ScorePrefabName);
        CameraObj = InstantiateChild(CameraPrefabName, false);
        OfflineObj = InstantiateChild(OfflinePrefabName, false);
        OnlineObj = null;   // オンラインオブジェは初期化しない
        ReturnBtnObj = InstantiateChild(ReturnButtonName,false);

        Transform BGPear = null;
        try {
            var Objtemp = GetComponentsInChildren<Transform>();
            BGPear = Objtemp[2].transform;
        }
        catch
        {
            BGPear = transform;
        }

        foreach (var it in BGPrefabNameList)
        {
            try
            {
                var ret = Instantiate(Resources.Load(it)) as GameObject;
                ret.transform.SetParent(BGPear, false);
                BGObjList.Add(ret);
            }
            catch (Exception excep)
            {
                Debug.LogError("Exception ! : " + it);
                Debug.LogException(excep);
            }
        }
        try {
            BGObjList[0].transform.localPosition = BGPosition;
            BGObjList[1].transform.position += new Vector3(100,0,0);
            BGObjList[2].transform.position += new Vector3(100,0,0);
        }
        catch
        {
            // Failed Load BG
        }
        Observable
            .NextFrame(FrameCountType.EndOfFrame)
            .Subscribe(_ => {
                try
                {
                    filterColorBak = BGObjList[0].GetComponentInChildren<MeshRenderer>().material.GetColor("_TexColor");
                    BGObjList[0].GetComponentInChildren<MeshRenderer>().material.SetColor("_TexColor", filterColor);     // ポップアップ中はタイトルにフィルターをかける
                    var Cont = BGObjList[1].GetComponentInChildren<TitleNodeController>();
                    Cont.InitNodesPosition();
                    Cont.isMoveNodes = false;                      // ノードの位置を初期位置へ戻す
                }
                catch { }

            }).AddTo(this);
        
        // ランキングのフリップ処理
        this.UpdateAsObservable()
            .Select(_ => FlipRanking)   // フリップフラグがONになった瞬間を感知
            .DistinctUntilChanged()
            .Where(_ => _)
            .Select(x => Mode)          // オンラインかオフラインかのフラグを変数に設定
            .Subscribe(x => {
                switch(x) {
                    case RANKING_MODE.OFFLINE:
                        OfflineObj.transform.DOLocalMoveY(1000, 0.5f);
                        Mode = RANKING_MODE.ONLINE;
                        if(OnlineObj != null) {
                            //OnlineObj.gameObject.SetActive(true);
                        }
                        break;
                    case RANKING_MODE.ONLINE:
                        OfflineObj.transform.DOLocalMoveY(0, 0.5f);
                        Mode = RANKING_MODE.OFFLINE;
                        if(OnlineObj != null) {
                            //OnlineObj.gameObject.SetActive(false);
                        }
                        break;
                }
                FlipRanking = false;
            }).AddTo(this);

        // オンラインランキングの初期化
        this.UpdateAsObservable()
            .Where(_ => Mode == RANKING_MODE.ONLINE)
            .Take(1)
            .Subscribe(_ => {
                OnlineObj = InstantiateChild(OnlinePrefabName, false);
            }).AddTo(this);
    }

    // Update is called once per frame
    void Update() {
        if(Input.GetKeyDown(KeyCode.Space)) {
            Flip();
        }
    }

    // ランキングを切り替えるトリガーを引く関数
    public void Flip() {
        FlipRanking = true;
    }

    public void OnDestroy()
    {
        BGObjList[0].GetComponentInChildren<MeshRenderer>().material.SetColor("_TexColor", filterColorBak);     // ポップアップ中はタイトルにフィルターをかける
    }
}
#pragma warning restore 414

