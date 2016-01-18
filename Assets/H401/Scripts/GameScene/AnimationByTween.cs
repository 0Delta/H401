using UnityEngine;
using System.Collections;
using DG.Tweening;
public class AnimationByTween : MonoBehaviour {
    Sequence seq;
	// Use this for initialization
	void Start () {

        seq = DOTween.Sequence();

        seq.Prepend(transform.DOLocalMove(new Vector3(0.0f,0.0f,8.0f),0.8f).SetEase(Ease.OutSine));        //中心点を経由
        seq.Join(transform.DOLocalRotate(Vector3.zero, 0.8f).SetEase(Ease.Linear));
        seq.Append(transform.DOLocalMove(new Vector3(10.0f, -24.0f,8.0f), 0.8f).SetEase(Ease.InSine));
        seq.Join(transform.DOLocalRotate(new Vector3(0.0f,0.0f,-179.0f), 0.8f).SetEase(Ease.Linear));
        //終了地点

        seq.Play();
    }
}
