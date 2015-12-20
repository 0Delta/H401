using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UnChainObject : MonoBehaviour {

    [SerializeField]private float tweenDuration;

    //位置と方向を覚えておいて、追加時に同じものを検索して追加するかを判定
    private Vec2Int _nodeVec;
    public Vec2Int nodeVec { get { return _nodeVec; } set { _nodeVec = value; } }
    private _eLinkDir _linkVec;
    public _eLinkDir linkVec { get { return _linkVec; } set { _linkVec = value; } }
    private bool _bChecked; //更新されなかったものは途切れでなくなったとして破棄するように
    public bool bChecked { get { return _bChecked; } set { _bChecked = value; } }

    private MeshRenderer mRenderer = null;
	// Use this for initialization
    void Awake()
    {
        _bChecked = true;

    }

	void Start () {
        //出現時tween
        mRenderer = gameObject.GetComponent<MeshRenderer>();
        mRenderer.material.DOFade(0.0f, 0.0f);
        mRenderer.material.DOFade(1.0f,tweenDuration);

        //_bChecked = true;
	
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
