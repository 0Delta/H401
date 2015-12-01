using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TitleButtons : MonoBehaviour {
    
    [SerializeField] private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField] private float scaleMag   = 0.0f;       // 拡大演出の倍率

    void ActionDirection(Transform trans) {
        // 回転処理(※TODO:new をなんとかしたい)
        trans.DORotate(new Vector3(0.0f, 0.0f, 60.0f), actionTime)
            .OnComplete(() => {
                transform.root.gameObject.GetComponent<AppliController>().ChangeScene(AppliController._eSceneID.GAME);
            });
        
        // 拡縮処理
        trans.DOScale(trans.localScale * scaleMag, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo);
    }

    public void OnClick(Transform trans) {
        ActionDirection(trans);
    }
}
