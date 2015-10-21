using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
using UnityEngine.UI;

public class Node : MonoBehaviour {

    private const float ROT_HEX_ANGLE = 60.0f;      // 六角形パネルの回転角度

    [SerializeField] private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField] private float scaleSize  = 0.0f;       // タップ時の拡大サイズ

    private bool isAction = false;      // アクションフラグ

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnMouseDown() {
        // アクション中なら未処理
        if(isAction)
            return;

        // アクション開始
        isAction = true;
        
        // 回転準備
        float angle = transform.localEulerAngles.z - ROT_HEX_ANGLE;

        // 回転処理(※TODO:new をなんとかしたい)
        transform.DORotate(new Vector3(0.0f, 0.0f, angle), actionTime)
            .OnComplete(() => {
                // 回転成分を初期化
                if(angle <= -360.0f) {
                    transform.rotation = Quaternion.identity;
                }

                // アクション終了
                isAction = false;
            });

        // 拡縮処理
        transform.DOScale(scaleSize, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo);
    }
}
