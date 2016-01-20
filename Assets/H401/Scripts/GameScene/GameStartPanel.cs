using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class GameStartPanel : MonoBehaviour {

    [SerializeField]private float textWMargin;
    [SerializeField]private float textHMargin;

    [SerializeField]private float textCenterWidth;
    [SerializeField]private float tweenDuration;
    [SerializeField]private float outScreen;
    private Image gameText = null;
    private Image startText = null;
    private Sequence sec;


    // Use this for initialization
    //開始時点ですべて設定する
    void Start() {



        //子テキストにアニメーション設定
        gameText = transform.FindChild("GameTextImage").GetComponent<Image>();
        startText = transform.FindChild("StartTextImage").GetComponent<Image>();

        sec = DOTween.Sequence();
        //所定の位置

        gameObject.transform.position = transform.parent.position;

        gameText.transform.localPosition = new Vector3(-outScreen + textWMargin, textHMargin, 0.0f);
        startText.transform.localPosition = new Vector3(-outScreen - textWMargin, -textHMargin, 0.0f);

        sec.Prepend(gameText.transform.DOLocalMoveX(-outScreen + textWMargin + 0.01f, 0.8f));
        //中心へ入ってくる

        sec.Append(gameText.transform.DOLocalMoveX(-textWMargin - textCenterWidth, tweenDuration / 4.0f));
        sec.Append(startText.transform.DOLocalMoveX(textWMargin - textCenterWidth, tweenDuration / 2.0f));

        //中心でちょっと待機する
        sec.Join(gameText.transform.DOLocalMoveX(-textWMargin + textCenterWidth, tweenDuration / 2.0f));
        sec.Append(startText.transform.DOLocalMoveX(textWMargin + textCenterWidth, tweenDuration / 4.0f));

        //画面外へ移動する
        sec.Join(gameText.transform.DOLocalMoveX(outScreen + textWMargin, tweenDuration / 4.0f));
        sec.Append(startText.transform.DOLocalMoveX(outScreen - textWMargin, tweenDuration / 4.0f)
            .OnComplete( () => {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                gameScene.gameController.nodeController.SetSlideAll(false); //ノードを操作可能状態に
                gameScene.gameUI.gameInfoCanvas.limitTime.eventRatio = 1.0f;    //時間が減るように
                Destroy(this.gameObject);
            }));

        sec.Play();
        /*        gameObject.transform.position = new Vector3( 0.0f,150.0f -textHMargin,0.0f);
                Vector3 vPos = new Vector3(-outScreen - textWMargin, -textHMargin, 0.0f);
                gameText.transform.DOLocalMoveX(-outScreen - textWMargin,tweenDuration / 4.0f).OnComplete(() =>
                {
                gameText.transform.localPosition = vPos;
                gameText.transform.DOLocalMoveX( - textWMargin - textCenterWidth, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        gameText.transform.DOLocalMoveX(-textWMargin + textCenterWidth, tweenDuration / 2.0f).SetEase(Ease.Linear)
                            .OnComplete(() =>
                            {
                                gameText.transform.DOLocalMoveX(outScreen - textWMargin, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
                                    .OnComplete(() => {
                                        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                                        gameScene.gameController.nodeController.SetSlideAll(false); //ノードを操作可能状態に
                                        gameScene.gameUI.gameInfoCanvas.limitTime.eventRatio = 1.0f;    //時間が減るように
                                        Destroy(this.gameObject);
                                    });
                            });
                    });
                });
                vPos = new Vector3(-outScreen + textWMargin, +textHMargin, 0.0f);
                startText.transform.position = new Vector3(0.0f,150.0f + textHMargin,0.0f);
                startText.transform.localPosition = vPos;

                startText.transform.DOLocalMoveX(+textWMargin - textCenterWidth, tweenDuration / 4.0f).SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        startText.transform.DOLocalMoveX(textWMargin + textCenterWidth, tweenDuration / 2.0f).SetEase(Ease.Linear)
                            .OnComplete(() =>
                            {
                                startText.transform.DOLocalMoveX(outScreen + textWMargin, tweenDuration / 4.0f).SetEase(Ease.OutQuad);
                            });
                    });
        */
    }
}
