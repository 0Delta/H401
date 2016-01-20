using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PseudoGridLight : MonoBehaviour {

    [SerializeField] private float endPosX = 0.0f;
    [SerializeField] private float moveTime = 0.0f;
    [SerializeField] private float startWaitTime = 0.0f;

	// Use this for initialization
	IEnumerator Start () {
        yield return new WaitForSeconds(startWaitTime);

        transform.DOLocalMoveX(endPosX, moveTime)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear);
	}
}
