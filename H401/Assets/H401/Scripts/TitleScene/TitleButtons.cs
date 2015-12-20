using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TitleButtons : MonoBehaviour {
    
    [SerializeField] private float  actionTime          = 0.0f;       // アクションにかかる時間
    [SerializeField] private float  scaleMag            = 0.0f;       // 拡大演出の倍率
    [SerializeField] private string gameButtonName      = null;       // ゲーム本編へ遷移するボタン名
    [SerializeField] private string howtoplayButtonName = null;       // 遊び方へ遷移するボタン名
    [SerializeField] private string rankingButtonName   = null;       // ランキングへ遷移するボタン名
    [SerializeField] private string optionButtonName    = null;       // オプションへ遷移するボタン名
    [SerializeField] private string creditButtonName    = null;       // クレジットへ遷移するボタン名
    [SerializeField] private FadeTime[] fadeTimes;                    // シーン切り替え用演出にかかる時間リスト

    void ActionDirection(Transform trans) {
        // 回転処理(※TODO:new をなんとかしたい)
        trans.DORotate(new Vector3(0.0f, 0.0f, 60.0f), actionTime)
            .OnComplete(() => {
                AppliController._eSceneID sceneID = AppliController._eSceneID.TITLE;
                int fadeID = 0;

                if(trans.name == gameButtonName) {
                    sceneID = AppliController._eSceneID.GAME;
                    fadeID = 0;
                } else if(trans.name == howtoplayButtonName) {
                    sceneID = AppliController._eSceneID.HOWTOPLAY;
                    fadeID = 1;
                } else if(trans.name == rankingButtonName) {
                    sceneID = AppliController._eSceneID.RANKING;
                    fadeID = 2;
                } else if(trans.name == optionButtonName) {
                    sceneID = AppliController._eSceneID.OPTION;
                    fadeID = 3;
                } else if(trans.name == creditButtonName) {
                    sceneID = AppliController._eSceneID.CREDIT;
                    fadeID = 4;
                }

                transform.root.gameObject.GetComponent<AppliController>().ChangeScene(sceneID, fadeTimes[fadeID].inTime, fadeTimes[fadeID].outTime);
            });
        
        // 拡縮処理
        trans.DOScale(trans.localScale * scaleMag, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo);
    }

    public void OnClick(Transform trans) {
        ActionDirection(trans);
    }
}
