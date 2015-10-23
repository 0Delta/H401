using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Node : MonoBehaviour {

    static private readonly float ROT_HEX_ANGLE = 60.0f;      // 六角形パネルの回転角度

    [SerializeField] private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField] private float scaleSize  = 0.0f;       // タップ時の拡大サイズ
    [SerializeField] private float slideTime  = 0.0f;       // スライド時の移動にかかる時間

    static private NodeController nodeControllerScript = null;      // NodeController のスクリプト

    private Vector2     nodeID      = Vector2.zero;     // パネルリストのID
    private bool        isAction    = false;            // アクションフラグ
    private _eSlideDir  slideDir    = _eSlideDir.NONE;  // 現在のスライド方向

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnMouseDown() {
        // アクション中なら未処理
        if(isAction)
            return;

        // アクション開始
        isAction = true;
        
        // ----- 回転処理
        // 回転準備
        float angle = transform.localEulerAngles.z + ROT_HEX_ANGLE;

        // 回転処理(※TODO:new をなんとかしたい)
        transform.DORotate(new Vector3(0.0f, 0.0f, angle), actionTime)
            .OnComplete(() => {
                // 回転成分を初期化
                //if(angle <= -360.0f) {
                //    transform.rotation = Quaternion.identity;
                //}

                // アクション終了
                isAction = false;
            });

        // 拡縮処理
        transform.DOScale(scaleSize, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo);

        // @Test ... タップしたノードのID
        print(nodeID);
    }

    void OnMouseEnter() {
        if(nodeControllerScript.IsDrag) {
            nodeControllerScript.AfterTapNodeID = nodeID;       // 移動させられるノードのIDを登録
            
            // ----- 移動処理
            nodeControllerScript.SlideNodes();
        }
    }

    void OnMouseExit() {
        if(nodeControllerScript.IsDrag) {
            nodeControllerScript.BeforeTapNodeID = nodeID;       // 移動させたいノードのIDを登録
        }
    }

    //void OnBecameInvisible() {
    //    print("Out of screen");

    //    nodeControllerScript.LoopBackNode(nodeID, slideDir);
    //}

    public void RegistNodeID(int row, int col) {
        nodeID.x = row;
        nodeID.y = col;
    }

    public void SetNodeController(NodeController nodeControllerScript) {
        Node.nodeControllerScript = nodeControllerScript;
    }

    public void SlideNode(_eSlideDir dir, Vector2 vec) {
        slideDir = dir;

        transform.DOMoveX(transform.position.x + vec.x, slideTime)
            .OnComplete(() => slideDir = _eSlideDir.NONE);
        transform.DOMoveY(transform.position.y + vec.y, slideTime);
    }
}
