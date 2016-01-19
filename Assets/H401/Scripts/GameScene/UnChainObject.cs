using UnityEngine;
using System.Collections;
using DG.Tweening;

public class UnChainObject : MonoBehaviour
{
    [SerializeField]
    private float tweenDuration;

    //位置と方向を覚えておいて、追加時に同じものを検索して追加するかを判定
    private Vec2Int _nodeVec;
    public Vec2Int nodeVec { get { return _nodeVec; } set { _nodeVec = value; } }
    private _eLinkDir _linkVec;
    public _eLinkDir linkVec { get { return _linkVec; } set { _linkVec = value; } }
    private bool _bChecked = true;      //更新されなかったものは途切れでなくなったとして破棄するように
    public bool bChecked { get { return _bChecked; } set { _bChecked = value; } }
    private bool bDeleted;

    private SpriteRenderer sRenderer = null;

    void Start()
    {
        //出現時tween
        sRenderer = gameObject.GetComponent<SpriteRenderer>();
/*        sRenderer.material.DOFade(0.0f, 0.0f);
        sRenderer.material.DOFade(1.0f, tweenDuration);*/
        

        bDeleted = false;
    }

    public void Vanish()
    {
        try
        {
            if (bDeleted)
                return;
            bDeleted = true;

                Destroy(this.gameObject);

        }
        catch(System.NullReferenceException) {
            return;
        }
    }
}
