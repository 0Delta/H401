using UnityEngine;
using System.Collections;
using DG.Tweening;
//using Assets.Scripts.Utils;

public class Node : MonoBehaviour {

    static private readonly float ROT_HEX_ANGLE = 60.0f;      // 六角形パネルの回転角度

    [SerializeField] private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField] private float scaleSize  = 0.0f;       // タップ時の拡大サイズ
    [SerializeField] private float slideTime  = 0.0f;       // スライド時の移動にかかる時間

    static private NodeController nodeControllerScript = null;      // NodeController のスクリプト

    private Vector2     nodeID      = Vector2.zero;     // パネルリストのID
    private bool        isAction    = false;            // アクションフラグ
    private _eSlideDir  slideDir    = _eSlideDir.NONE;  // 現在のスライド方向

    public BitArray bitLink = new BitArray(6);  //道の繋がりのビット配列　trueが道
                                                //  5 0
                                                // 4   1
                                                //  3 2  とする
    public bool bChecked                        //走査済みフラグ 枝完成チェックに使用
    {
        get { return bChecked; }
        set { bChecked = value; }
    }

    public bool bComplete                       //完成済フラグ 走査終了時（枝完成時）に使用
    {
        get { return bComplete; }
        set { bComplete = value; }
    }


	// Use this for initialization
	void Start () {
        //とりあえずテスト
        bitLink.Set(1, true);
        bitLink.Set(4, true);

        //bitLink.
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

                BitLinkRotate();
                //nodeControllerScript.CheckLink();

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
            .OnComplete(() => {
                slideDir = _eSlideDir.NONE;
                //nodeControllerScript.CheckLink();
            });
        transform.DOMoveY(transform.position.y + vec.y, slideTime);
    }

    //道のビット配列を回転させる bitarrayに回転シフトがなかった
    void BitLinkRotate()
    {
        //右回転（左シフト）
        bool b5 = bitLink[5];
       // char[] deb = new char[6];
        for(int i = 4 ; i >= 0 ; i--)
        {
            bitLink[i + 1] = bitLink[i];
        }
        bitLink[0] = b5;

        //とりあえず表示してみる
        print(OrigLog.ToString(bitLink));
    }

    //ノードごとの道がつながっているか走査(親ビットの方向)
    public bool CheckBit(_eLinkDir linkDir)
    {
        bComplete = true;   //最初に立てて、ダメだったら戻す

        //まず元の方向と道が繋がっていないならダメ
        if (bitLink[(int)linkDir])
        {
            bComplete = false;
            return false;
        }
        //このノードがチェック済ならおｋとする
        if (bChecked)
            return true;

        //ビット配列をすべて見る
        for(int i = 0 ; i < (int)_eLinkDir.MAX ;i++)
        {
            //元の方向は飛ばす
            if (i == (int)linkDir)
                continue;

            //道があるならその方向のノードを走査
            if(bitLink[i])
            {
                //ノードコントローラにIDと走査方向を渡し、ノードがあるかを調べる
                _eLinkDir parentDir = (i + 3 >= 6) ? (_eLinkDir)( i + 3 - 6) : (_eLinkDir)(i + 3);
                
                Vector2 nextPos = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)i);

                if (nextPos.x == -1)//ない＝壁なので、その時はおｋを返す
                {
                    bChecked = true;
                    return true;
                }
                else
                {
                    //ある場合は、そこに元来た方向（走査方向＋３(-６)）を渡しさらに走査
                    if(nodeControllerScript.GetNodeScript(nextPos).CheckBit(parentDir))
                    {
                        bChecked = true;
                        return true;
                    }
                    else
                    {
                        bComplete = false;
                        return false;
                    }
                }
            }
        }

        //問題なければ閲覧済みフラグを立てておｋを返す
        bChecked = true;
        return true;
    }


    //根本のノードはこっちを呼ぶようにする
    public bool CheckBit()
    {
        bComplete = true;   //最初に立てて、ダメだったら戻す

        //基本的に変わらないが、↓２方向のうちどちらかが繋がっていないとダメ
        if (!(bitLink[(int)_eLinkDir.RD] || bitLink[(int)_eLinkDir.LU]))
        {
            bComplete = false;
            return false;
        }
        //ビット配列をすべて見る
        for (int i = 0; i < (int)_eLinkDir.MAX; i++)
        {
            //元の方向は飛ばす
            if (i == (int)_eLinkDir.RD || i == (int)_eLinkDir.LD)
                continue;

            //道があるならその方向のノードを走査
            if (bitLink[i])
            {
                //ノードコントローラにIDと走査方向を渡し、ノードがあるかを調べる
                _eLinkDir parentDir = (i + 3 >= 6) ? (_eLinkDir)(i + 3 - 6) : (_eLinkDir)(i + 3);

                Vector2 nextPos = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)i);

                if (nextPos.x == -1)//ない＝壁なので、その時はおｋを返す
                {
                    bChecked = true;
                    return true;
                }
                else
                {
                    //ある場合は、そこに元来た方向（走査方向＋３(-６)）を渡しさらに走査
                    if(nodeControllerScript.GetNodeScript(nextPos).CheckBit(parentDir))
                    {
                        bChecked = true;
                        return true;
                    }
                    else
                    {
                        bComplete = false;
                        return false;
                    }
                }
            }
        }

        //問題なければ閲覧済みフラグを立てておｋを返す
        bChecked = true;
        return true;
    }
}
