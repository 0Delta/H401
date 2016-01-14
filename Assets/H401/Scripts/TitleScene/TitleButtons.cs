using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

public class TitleButtons : MonoBehaviour {
    
    [SerializeField] private float  scaleTime   = 0.0f;       // 拡縮演出にかかる時間
    [SerializeField] private float  scaleMag    = 0.0f;       // 拡縮演出の倍率
    [SerializeField] private float  moveTime    = 0.0f;       // 移動演出にかかる時間
    [SerializeField] private float  moveEndPosX = 0.0f;       // 移動演出の移動終了座標(移動距離)
    [SerializeField] private AppliController._eSceneID[] sceneButtonOrder;       // シーン遷移するボタンの配置順番
    [SerializeField] private FadeTime[] fadeTimes;            // シーン切り替え用演出にかかる時間リスト

    private Transform[] buttonTransList;        // ボタンの Transform リスト
    private AudioSource audioSource;

    void Start() {
        buttonTransList = transform.GetComponentsInChildren<Transform>().Where(x => gameObject != x.gameObject).ToArray();
        audioSource = GetComponent<AudioSource>();
    }

    void ActionDirection(Transform trans) {
        // シーン遷移先を設定
        AppliController._eSceneID sceneID = AppliController._eSceneID.TITLE;
        int fadeID = 0;
        for(int i = 0; i < sceneButtonOrder.Length; ++i) {
            if(trans.name == buttonTransList[i].name) {
                sceneID = sceneButtonOrder[i];
                fadeID = i;
            }
        }

        // 遷移演出開始
        int cnt = 0;
        for(int i = 0; i < sceneButtonOrder.Length; ++i) {
            // 選択されたボタンなら未処理
            if (i == fadeID)
                continue;

            Transform handleTrans = buttonTransList[i];

            // 拡縮処理
            handleTrans.DOScale(handleTrans.localScale * scaleMag, scaleTime).OnComplete(() => {
                // 処理したボタン数をカウントアップ
                ++cnt;

                // 移動方向を確認
                int revSign = 1;
                if (cnt % 2 != 0) {
                    revSign = -revSign;
                }

                // 移動処理
                handleTrans.DOMoveX(moveEndPosX * revSign, moveTime).OnComplete(() => {
                    // 次のシーンへ
                    transform.root.gameObject.GetComponent<AppliController>().ChangeScene(sceneID, fadeTimes[fadeID].inTime, fadeTimes[fadeID].outTime);
                })
                .SetEase(Ease.OutCubic);
            })
            .SetEase(Ease.OutCubic);
        }        
    }

    public void OnClick(Transform trans) {
        ActionDirection(trans);
        audioSource.Play();
    }
}
