using UnityEngine;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using System;
using UnityEngine.UI;

namespace RankingExtension {
    public static class InstantiateEx {
        // ゲームオブジェクトを生成して、子に登録する関数
        public static GameObject InstantiateChild(this MonoBehaviour Mono, string Name, bool WorldPositionStays = true) {
            GameObject ret = null;
            string Pass = "";
            RankingMGR mgr = Mono.GetComponentInParent<RankingMGR>();
            if(mgr.RankingPrefabFolderName != "") {
                Pass += mgr.RankingPrefabFolderName + "/";
            }
            Pass += Name;

            try {
                ret = MonoBehaviour.Instantiate(Resources.Load(Pass)) as GameObject;
                ret.transform.SetParent(Mono.transform, WorldPositionStays);
            }
            catch(Exception excep) {
                ret = null;
                Debug.LogException(excep);
            }
            return ret;
        }
    }
}

public class RankingMGR : MonoBehaviour {

    [SerializeField] public string RankingPrefabFolderName;
    [SerializeField] public string OnlinePrefabName;
    [SerializeField] public string OfflinePrefabName;
    [SerializeField] public string BGPrefabName;
    [SerializeField] public string ScorePrefabName;
    [SerializeField] public string CameraPrefabName;
    [SerializeField] public string ReturnButtonName;

    GameObject ScoreObj;
    GameObject OnlineObj;
    GameObject OfflineObj;
    GameObject BGObj;
    GameObject CameraObj;
    GameObject ReturnBtnObj;
    private bool FlipRanking = false;
    public enum RANKING_MODE : byte{
        OFFLINE = 0,
        ONLINE = 1
    };
    public RANKING_MODE Mode = RANKING_MODE.OFFLINE;

    // ゲームオブジェクトを生成して、子に登録する関数
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
        BGObj = InstantiateChild(BGPrefabName, false);
        CameraObj = InstantiateChild(CameraPrefabName, false);
        ReturnBtnObj = InstantiateChild(ReturnButtonName,false);
        OfflineObj = InstantiateChild(OfflinePrefabName, false);
        OnlineObj = null;   // オンラインオブジェは初期化しない
        

        // ランキングのフリップ処理
        this.UpdateAsObservable()
            .Select(_ => FlipRanking)   // フリップフラグがONになった瞬間を感知
            .DistinctUntilChanged()
            .Where(_ => _)
            .Select(x => Mode)          // オンラインかオフラインかのフラグを変数に設定
            .Subscribe(x => {
                switch(x) {
                    case RANKING_MODE.OFFLINE:
                        OfflineObj.transform.DOLocalMoveY(700, 0.5f);
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
            .First(_ => Mode == RANKING_MODE.ONLINE)
            .Subscribe(_ => {
                OnlineObj = InstantiateChild(OnlinePrefabName, false);
            });
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
}
