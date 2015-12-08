using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class GameStartPanel : MonoBehaviour {

    [SerializeField]private float textWMargin;
    [SerializeField]private float textHMargin;

    [SerializeField]private float textCenterWidth;
    [SerializeField]private float tweenDuration;

    private Text gameText = null;
    private Text startText = null;

	// Use this for initialization
	//開始時点ですべて設定する
	void Start () {
        //子テキストにアニメーション設定
        gameText = transform.FindChild("GameText").GetComponent<Text>();
        startText = transform.FindChild("StartText").GetComponent<Text>();

        Vector3 vPos = new Vector3(-750 - textWMargin, -textHMargin, 0.0f);
        gameText.transform.localPosition = vPos;
        gameText.transform.DOLocalMoveX( - textWMargin - textCenterWidth, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                gameText.transform.DOLocalMoveX(-textWMargin + textCenterWidth, tweenDuration / 2.0f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        gameText.transform.DOLocalMoveX(750 - textWMargin, tweenDuration / 4.0f).SetEase(Ease.OutQuad);
                    });
            });

        vPos = new Vector3(-750 + textWMargin, +textHMargin, 0.0f);
        startText.transform.localPosition = vPos;
        startText.transform.DOLocalMoveX(-750 + textWMargin,tweenDuration / 4.0f).OnComplete(() =>
        {
            startText.transform.DOLocalMoveX(+textWMargin - textCenterWidth, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    startText.transform.DOLocalMoveX(textWMargin + textCenterWidth, tweenDuration / 2.0f).SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            startText.transform.DOLocalMoveX(750 + textWMargin, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
                                .OnComplete(() =>
                                {
                                    GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                                    gameScene.gameController.nodeController.SetActionAll(false); //ノードを操作可能状態に
                                    gameScene.gameUI.gameInfoCanvas.limitTime.eventRatio = 1.0f;    //時間が減るように
                                    Destroy(this.gameObject);
                                });
                        });
                });
        });
	}
	
	// Update is called once per frame
	void Update () {
	
	}

}
