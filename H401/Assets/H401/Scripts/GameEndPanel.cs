using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class GameEndPanel : MonoBehaviour {

    [SerializeField]
    private float tweenDuration = 0.0f;

	// Use this for initialization
	void Start () {

        Button toResultButton = GetComponentInChildren<Button>();
        toResultButton.interactable = false;

        //パネル自体が降ってくるようにtween 透過はわからんかった
        transform.localPosition.Set(0.0f, 1334.0f, 0.0f);
        transform.DOLocalMoveY(0.0f,tweenDuration / 3.0f)
            .OnComplete(() =>{
                transform.DOLocalMoveY(1334.0f / 3.0f, tweenDuration / 2.0f).OnComplete(() =>
                {
                    transform.DOLocalMoveY(0.0f, tweenDuration / 4.0f).OnComplete(() =>
                    {
                        //ボタンをアクティブに
                        toResultButton.interactable = true;
                    });
                });
            });
       
        
        //ボタンにリザルトへ遷移するように設定
        toResultButton.onClick.AddListener(() => { });
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
