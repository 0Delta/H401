using UnityEngine;
using System.Collections;
using DG.Tweening;
public class HighLightObject : MonoBehaviour {
    private float rotDuration = 0.8f;
	// Use this for initialization
	void Start () {
        transform.DOLocalRotate(new Vector3(0.0f, 0.0f,  -90.0f), rotDuration * 0.5f)/*.OnComplete(RotTurn)*/.SetEase(Ease.Linear).SetUpdate(true).SetLoops(-1,LoopType.Incremental);

    }
}
