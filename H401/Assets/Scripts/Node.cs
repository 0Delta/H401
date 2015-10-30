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

    private Vec2Int     nodeID      = Vec2Int.zero;     // パネルリストのID
    private bool        isAction    = false;            // アクションフラグ
    private bool        isSlide     = false;            // スライドフラグ
    private _eSlideDir  slideDir    = _eSlideDir.NONE;  // 現在のスライド方向

    public BitArray bitLink = new BitArray(6);  //道の繋がりのビット配列　trueが道
                                                //  5 0
                                                // 4   1
                                                //  3 2  とする
    private bool bChecked = false;              

    public bool CheckFlag                       //走査済みフラグ 枝完成チェックに使用
    {
        get { return bChecked; }
        set { bChecked = value; }
    }
    private bool bCompleted;
    public bool CompleteFlag                    //完成済フラグ 走査終了時（枝完成時）に使用
    {
        get { return bCompleted; }
        set { bCompleted = value; }
    }
    private bool bChain;                        //枝がつながっているか？
    public bool ChainFlag
    {
        get { return bChain; }
        set { bChain = value; }
    }

	// Use this for initialization
	void Start () {
        //とりあえずテスト
        //bitLink.
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    void OnMouseDown() {
    }
    
    void OnMouseUp() {
        // アクション中なら未処理
        if(isAction)
            return;

        // スライド中なら回転・拡縮は未処理
        if(isSlide) {
            isSlide = false;
            return;
        }
        
        // アクション開始
        isAction = true;
        
        // ----- 回転処理
        // 回転準備
        float angle = transform.localEulerAngles.z - ROT_HEX_ANGLE;

        // 回転処理(※TODO:new をなんとかしたい)
        transform.DORotate(new Vector3(0.0f, 0.0f, angle), actionTime)
            .OnComplete(() => {
                // 回転成分を初期化
                if(angle <= -360.0f) {
                    transform.rotation = Quaternion.identity;
                }

                // アクション終了

                BitLinkRotate();
                nodeControllerScript.CheckLink();

                isAction = false;
            });

        // 拡縮処理
        transform.DOScale(scaleSize, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo);

        // @Test ... タップしたノードのID
        print(nodeID);
    }

    void OnMouseEnter() {
        // スライド中なら未処理
        if(nodeControllerScript.SlideDir != _eSlideDir.NONE)
            return;

        if(nodeControllerScript.IsDrag) {
            nodeControllerScript.AfterTapNodeID = nodeID;       // 移動させられるノードのIDを登録
            
            // ----- 移動処理
            nodeControllerScript.StartSlideNodes();
        }
    }

    void OnMouseExit() {
        // スライド中なら未処理
        if(nodeControllerScript.SlideDir != _eSlideDir.NONE)
            return;

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

    public void SlideNode(_eSlideDir dir, Vector2 pos) {
        slideDir = dir;

        if(!isSlide)
            isSlide = true;

        transform.DOMoveX(pos.x, slideTime)
            .OnComplete(() => {
                slideDir = _eSlideDir.NONE;
                //nodeControllerScript.CheckLink();
            });
        transform.DOMoveY(pos.y, slideTime);
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
        bool bTempChain = true;
        bCompleted = true;   //最初に立てて、ダメだったら戻す



        //まず元の方向と道が繋がっていないならダメ
        if(linkDir == _eLinkDir.NONE)
        {
            //根本の場合
            if (!(bitLink[(int)_eLinkDir.RD] || bitLink[(int)_eLinkDir.LD]))
            {
                bCompleted = false;
                return false;
            }
        }
        else
        {
            //それ以外の時、つまり他のノードから呼ばれている時
            if (!bitLink[(int)linkDir]) //元のノード側と道がつながっていなければ
            {
                bCompleted = false;
                return false;
            }
        }

        bChain = true;
        GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.0f, 0.0f);   //とりあえず赤フィルターを掛けてみる
        //このノードがチェック済ならおｋとする
        if (bChecked)
            return true;

        //ビット配列をすべて見る
        for(int i = 0 ; i < (int)_eLinkDir.MAX ;i++)
        {
            if (linkDir == _eLinkDir.NONE)
            {
                //根本の場合、↓方向は飛ばす
                if (i == (int)_eLinkDir.RD || i == (int)_eLinkDir.LD)
                    continue;
            }
            else
            {   //それ以外のときは、元の方向は飛ばす
                if (i == (int)linkDir)
                    continue;
            }
            //道があるならその方向のノードを走査
            if(bitLink[i])
            {
                //ノードコントローラにIDと走査方向を渡し、ノードがあるかを調べる
                _eLinkDir parentDir = (i + 3 >= 6) ? (_eLinkDir)( i + 3 - 6) : (_eLinkDir)(i + 3);
                
                Vec2Int nextPos = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)i);
                //ない＝壁なので、その時はおｋを返す
                if (nextPos.x == 0 || nextPos.y == 0 || nextPos.x == nodeControllerScript.Row - 1 || nextPos.y == nodeControllerScript.Col - 1)
                {
                    continue;
                }
                else
                {
                    //ある場合は、そこに元来た方向（走査方向＋３(-６)）を渡しさらに走査
                    if(nodeControllerScript.GetNodeScript(nextPos).CheckBit(parentDir))
                    {
                        continue;
                    }
                    else
                    {
                        bTempChain = false;
                        continue;
                    }
                }
            }
        }

        //問題なければ閲覧済みフラグを立てておｋを返す
        if (bTempChain)
        {
            bChecked = true;
            return true;
        }
        else
        {
            bCompleted = false;
            return false;
        }
    }

    //ノードにタイプ・テクスチャ・道ビット
    public void SetNodeType(_eNodeType type)
    {     
        //ビットタイプ・テクスチャを設定
        bitLink[0] = true;
        switch(type)
        {
            case _eNodeType.HUB2_A:
                bitLink[3] = true;
                break;
            case _eNodeType.HUB2_B:
                bitLink[1] = true;
                break;
            case _eNodeType.HUB2_C:
                bitLink[0] = true;
                bitLink[2] = true;
                break;
            case _eNodeType.HUB3_A:
                bitLink[2] = true;
                bitLink[3] = true;
                break;
            case _eNodeType.HUB3_B:
                bitLink[2] = true;
                bitLink[5] = true;
                break;
            case _eNodeType.HUB3_C:
                bitLink[1] = true;
                bitLink[2] = true;
                break;
        }

        //ランダムに回転
        float angle = 0.0f;
        for (int i = 0; i < UnityEngine.Random.Range(0, 5); i++)
        {
            BitLinkRotate();
            angle -= ROT_HEX_ANGLE;
        }
        transform.Rotate(0.0f, 0.0f, angle);

    }

    //周囲のノードを１つずつ見ていく走査
    public bool CheckAround()
    {
        bChain = true;
        for (int i = 0,parentDir = 3; i < (int)_eLinkDir.MAX ; i++, parentDir++)
        {
            if(parentDir >= 6)
                parentDir -= 6;



            Vec2Int nextPos = Vec2Int.zero;
            
            nodeControllerScript.GetDirNode(nodeID,(_eLinkDir)i);

            //枠外ノードは常につながっている判定とする
            if (nextPos.x == 0 || nextPos.y == 0 || nextPos.x == nodeControllerScript.Row - 1 || nextPos.y == nodeControllerScript.Col - 1)
            {
                continue;
            }

            //隣り合うノードの道がこっちにつながっているかどうか
            if(nodeControllerScript.GetNodeScript(nextPos).GetLinkDir((_eLinkDir)parentDir))
            {

            }
            else 
            {
                nodeControllerScript.GetNodeScript(nextPos).ChainFlag = false;
            }


            //だめならfalse


        }
        return true;
    }

    public bool GetLinkDir(_eLinkDir parentDir)
    {
        return bitLink[(int)parentDir];
    }

}
