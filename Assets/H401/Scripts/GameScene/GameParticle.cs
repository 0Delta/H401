using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameParticle : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    public void MoveFinishedDestroy(Vector3 endPos, float duration) {
        transform.DOMove(endPos, duration)
            .OnComplete(() => {
                Destroy(gameObject);
            })
            .SetEase(Ease.InCubic);
    }

    public void ChangeColorByScore(float score) {

    }	
}
