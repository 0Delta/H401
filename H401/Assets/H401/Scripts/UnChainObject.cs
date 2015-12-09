using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UnChainObject : MonoBehaviour {

    [SerializeField]private float tweenDuration;

    MeshRenderer mRenderer = null;
	// Use this for initialization
	void Start () {
        //出現時tween
        mRenderer = gameObject.GetComponent<MeshRenderer>();
        mRenderer.material.DOFade(0.0f, 0.0f);
        mRenderer.material.DOFade(1.0f,tweenDuration);
	
	}

    public void Vanish()
    {
        mRenderer.material.DOFade(0.0f, tweenDuration)
            .OnComplete(() => {
                Destroy(this.gameObject);
            });
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
