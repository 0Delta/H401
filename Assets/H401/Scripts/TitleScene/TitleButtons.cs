using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Linq;

public class TitleButtons : MonoBehaviour {

    [System.Serializable]
    private struct SceneButton {
        public AppliController._eSceneID order;
        public FadeTime fadeTime;
    };
    
    [SerializeField] private float  scaleTime   = 0.0f;       // 拡縮演出にかかる時間
    [SerializeField] private float  scaleMag    = 0.0f;       // 拡縮演出の倍率
    [SerializeField] private float  moveTime    = 0.0f;       // 移動演出にかかる時間
    [SerializeField] private float  moveEndPosX = 0.0f;       // 移動演出の移動終了座標(移動距離)
    [SerializeField] private SceneButton[] sceneButtons;      // シーン遷移するボタンのリスト
    [SerializeField] private AppliController._eSceneID[] popupSceneList;       // ポップアップ的に遷移するシーンのリスト

    private TitleScene titleScene = null;   // タイトルシーンのスクリプト
    private Transform[] buttonTransList;        // ボタンの Transform リスト
    private Vector3[] buttonPosList;        // ボタンの position リスト
    private Vector3[] buttonScaleList;        // ボタンの scale リスト
    private AudioSource audioSource;
    private bool isOnClick = false;     // ボタン押下フラグ

    void Start() {
        titleScene = transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<TitleScene>();
        buttonTransList = transform.GetComponentsInChildren<Transform>().Where(x => gameObject != x.gameObject).ToArray();
        audioSource = GetComponent<AudioSource>();

        isOnClick = false;

        buttonPosList = new Vector3[buttonTransList.Length];
        buttonScaleList = new Vector3[buttonTransList.Length];
        for(int i = 0; i < buttonTransList.Length; ++i) {
            buttonPosList[i] = buttonTransList[i].localPosition;
            buttonScaleList[i] = buttonTransList[i].localScale;
        }
    }

    void ActionDirection(Transform trans) {
        // シーン遷移先を設定
        AppliController._eSceneID sceneID = AppliController._eSceneID.TITLE;
        int fadeID = 0;
        for(int i = 0; i < sceneButtons.Length; ++i) {
            if(trans.name == buttonTransList[i].name) {
                sceneID = sceneButtons[i].order;
                fadeID = i;
            }
        }

        // 遷移演出開始
        int cnt = 0;
        for(int i = 0; i < sceneButtons.Length; ++i) {
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
                    // 普通のシーン遷移かポップアップシーン遷移かをチェック
                    bool isPopup = false;
                    foreach(var id in popupSceneList) {
                        if(sceneID == id) {
                            isPopup = true;
                            break;
                        }
                    }
                    
                    // クリックフラグを戻す
                    isOnClick = false;

                    // 次のシーンへ
                    if(isPopup) {
                        titleScene.PopupChangeScene(sceneID, sceneButtons[fadeID].fadeTime.inTime, sceneButtons[fadeID].fadeTime.outTime);
                    } else {
                        transform.root.gameObject.GetComponent<AppliController>().ChangeScene(sceneID, sceneButtons[fadeID].fadeTime.inTime, sceneButtons[fadeID].fadeTime.outTime);
                    }
                })
                .SetEase(Ease.OutCubic);
            })
            .SetEase(Ease.OutCubic);
        }
    }

    public void InitButtonsTransform() {
        // ボタンの Transform を初期値へ戻す
        for(int i = 0; i < buttonTransList.Length; ++i) {
            buttonTransList[i].localPosition = buttonPosList[i];
            buttonTransList[i].localScale = buttonScaleList[i];
        }
    }

    public void OnClick(Transform trans) {
        if(!isOnClick) {
            audioSource.Play();

            ActionDirection(trans);

            isOnClick = true;
        }
    }

    public FadeTime GetFadeTime(AppliController._eSceneID sceneID) {
        FadeTime fadeTime = new FadeTime(0.0f, 0.0f);

        foreach (var scene in sceneButtons) {
            if(scene.order == sceneID) {
                fadeTime = scene.fadeTime;
                break;
            }
        }

        return fadeTime;
    }
}
