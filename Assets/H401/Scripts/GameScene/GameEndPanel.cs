using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class GameEndPanel : MonoBehaviour {

    [SerializeField]
    private float tweenDuration = 0.0f;

	// Use this for initialization
	void Start () {
        //パネル自体が降ってくるようにtween 透過はわからんかった
        transform.localPosition.Set(0.0f, 1334.0f, 0.0f);
        transform.DOLocalMoveY(0.0f,tweenDuration / 2.0f).SetEase(Ease.InCubic)
            .OnComplete(() =>{
                transform.DOLocalMoveY(1334.0f / 3.0f, tweenDuration / 4.0f).SetEase(Ease.OutCubic).OnComplete(() =>
                {
                    transform.DOLocalMoveY(0.0f, tweenDuration / 4.0f).SetEase(Ease.InCubic);
                });
            });
	}

    public void SetScore(int Score,int Rank)
    {
        Text tex = GetComponentInChildren<Text>();
        tex.text = "Score\n";
        tex.text += Score.ToString() + "\n";
    }
}
