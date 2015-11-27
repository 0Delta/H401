using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
//using Assets.Scripts.Utils;


public class Node : MonoBehaviour {

    static private readonly float ROT_HEX_ANGLE = 60.0f;      // 六角形パネルの回転角度

    [SerializeField] private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField] private float scaleSize  = 0.0f;       // タップ時の拡大サイズ
    [SerializeField] private float slideTime  = 0.0f;       // スライド時の移動にかかる時間

    static private NodeController nodeControllerScript = null;      // NodeController のスクリプト

    private Vec2Int     nodeID      = Vec2Int.zero;     // パネルリストのID
    public Vec2Int NodeID
    {
        get { return nodeID; }
    }
    private bool        isAction    = false;            // アクションフラグ
    private bool        isSlide     = false;            // スライドフラグ
    private bool        isOutScreen = false;            // 画面外フラグ
    private bool        isOutPuzzle = false;            // パズル外フラグ

    public BitArray bitLink = new BitArray(6);  //道の繋がりのビット配列　trueが道
                                                //  5 0
                                                // 4   1
                                                //  3 2  とする

    //public BitArray bitTreePath = new BitArray(4);//走査時にどの木の一部かを記憶しておく
    public Vec2Int[] ChainNodes = new Vec2Int[5];

    private MeshRenderer meshRenderer;
    public MeshRenderer MeshRenderer
    {
        get { return meshRenderer; }
    }

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

    public bool IsOutScreen {
        set { isOutScreen = value; }
        get { return isOutScreen; }
    }

    static private readonly string[] HEX_TEXTURE = {
        "Materials/hex0",
        "Materials/hex1",
        "Materials/hex2",
        "Materials/hex3",
        "Materials/hex4",
        "Materials/hex5",
    };
                                                   
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }
    // Use this for initialization
    void Start()
    {
        //とりあえずテスト
        //bitLink.
        
        // IDが変化したときにパズル外フラグを更新
        Observable
            .EveryUpdate()
            .Select(_ => nodeID)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                CheckOutPuzzle();
            }).AddTo(this);
        CheckOutPuzzle();       // 初回だけ手動
    }
    
    // Update is called once per frame
    void Update () {
        
    }

    public void RegistNodeID(int row, int col) {
        nodeID.x = row;
        nodeID.y = col;
    }

    public void SetNodeController(NodeController nodeControllerScript) {
        Node.nodeControllerScript = nodeControllerScript;
    }
    
    public void RotationNode() {
        // 画面外ノードなら未処理
        if(isOutScreen)
            return;

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
        //print(nodeID);
    }

    public void SlideNode(_eSlideDir dir, Vector2 pos) {
        // スライド方向が指定されていなければ未処理
        if (dir == _eSlideDir.NONE)
            return;
        
        if(!isSlide)
            isSlide = true;

        if(!nodeControllerScript.IsNodeAction)
            nodeControllerScript.IsNodeAction = true;
        
        transform.DOKill();
        transform.DOMoveX(pos.x, slideTime)
            .OnComplete(() => {
                nodeControllerScript.CheckOutScreen(nodeID);
                if(nodeControllerScript.IsNodeAction)
                    nodeControllerScript.IsNodeAction = false;
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
        //print(OrigLog.ToString(bitLink));
    }

    ///  こっから妹尾
     
    // お隣さんから自分への道を保持する。
    private BitArray Negibor = new BitArray(6);
    // 情報の更新
    public void UpdateNegibor()
    {
        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
        {
            // 周辺ノードを一周ぐるっと
            Vec2Int Target = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)n);
            if (Target.x != -1)
            {
                Node TgtNode = nodeControllerScript.GetNodeScript(Target);
                if (TgtNode.isOutPuzzle)
                {
                    // 壁だったら繋がっている判定
                    Negibor[n] = true;
                }
                else
                {
                    // ノードがあれば、自分と接している場所を見て、取得する。
                    Negibor[n] = nodeControllerScript.GetNodeScript(Target).bitLink[(n + 3 >= 6) ? (n + 3 - 6) : (n + 3)];
                }
            }
            else
            {
                // 壁だったら繋がっている判定
                Negibor[n] = true;
            }
        }
    }

    // 隣と繋がっているかのANDがめんどくさかったんで関数化。
    public bool LinkedNegibor(_eLinkDir n)
    {
        return LinkedNegibor((int)n);
    }
    public bool LinkedNegibor(int n)
    {
        return Negibor[n] && bitLink[n];
    }

    // _eLinkDirのToStringもどき。デバック出力用。
    public string DLChker(int n)
    {
        return DLChker((_eLinkDir)n);
    }
    public string DLChker(_eLinkDir n)
    {
        string[] DbgLinkStr = { "RU", "R ", "RD", "LD", "L ", "LU" };
        return DbgLinkStr[(int)n];
    }

    // 隣接判定、ノードごとの処理
    public void NodeCheckAction(NodeController.NodeLinkTaskChecker Tc, _eLinkDir Link)
    {
        // チェック済みでスキップ
        if (bChecked) { Tc.Branch--; return; }

        // 各種変数定義
        bool bBranch = false;
        bChecked = true;
        Tc.SumNode++;

        // お隣さんを更新
        UpdateNegibor();

        // 状態表示
        if (Debug.isDebugBuild && NodeController.bNodeLinkDebugLog)
            Tc += ("[" + nodeID.x + "," + nodeID.y + "] " + "Node_Action \n"
            + (bitLink[0] ? "1" : "0") + (bitLink[1] ? "1" : "0") + (bitLink[2] ? "1" : "0") + (bitLink[3] ? "1" : "0") + (bitLink[4] ? "1" : "0") + (bitLink[5] ? "1" : "0") + "\n"
             + (Negibor[0] ? "1" : "0") + (Negibor[1] ? "1" : "0") + (Negibor[2] ? "1" : "0") + (Negibor[3] ? "1" : "0") + (Negibor[4] ? "1" : "0") + (Negibor[5] ? "1" : "0"));
        Tc += Tc.NotFin + " : " + Tc.Branch.ToString();

        // チェックスタート
        // 接地判定（根本のみ）
        if (Link == _eLinkDir.NONE)     // 根本か確認
        {
            if (!bitLink[(int)_eLinkDir.RD] && !bitLink[(int)_eLinkDir.LD]) // 下方向チェック
            {
                Tc.Branch--;
                Tc.SumNode--;
                bChecked = false;
                return;                 // 繋がってないなら未チェックとして処理終了
            }
            // 繋がっている
            if (Debug.isDebugBuild && NodeController.bNodeLinkDebugLog)
                Tc += ("Ground");
        }

        // この時点で枝が繋がっている事が確定
        meshRenderer.material.color = new Color(0.5f, 0.0f, 0.0f);   //とりあえず赤フィルターを掛けてみる
        Tc.NodeList.Add(this);  // チェッカに自身を登録しておく

        // 終端ノードであれば、周囲チェック飛ばす
        bool bEndNode = true;
        for (int n = 0; n < (int)_eLinkDir.MAX && bEndNode; n++)    // 周囲ぐるっと
        {
            if (Link == _eLinkDir.NONE)
            {
                // 根本なら、下方向は除外
                if (n != (int)_eLinkDir.RD && n != (int)_eLinkDir.LD && bitLink[n])
                {
                    bEndNode = false;   // !!デバックトレース時に必ず経由するけど、動作には影響なし!!
                }
            }
            else
            {
                // それ以外は来た方向を除外して判定
                if (n != (int)Link && bitLink[n])
                {
                    bEndNode = false;
                }
            }
        }
        if (bEndNode)   // 終端ノードであればそこで終了
        {
            Tc.Branch--;
            return;
        }

        // 周囲のチェック
        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
        {

            // 元の方向と、根本の時の下を除外
            if (Link == _eLinkDir.NONE && (n == (int)_eLinkDir.RD || n == (int)_eLinkDir.LD)) { continue; }
            if (n == (int)Link) { continue; }

            // それ以外で繋がっている場所を検索
            if (bitLink[n])
            {
                if (Negibor[n])
                {
                    // お隣さんと繋がっているので、処理引き渡しの準備
                    bChain = true;

                    // デバック表示
                    if (Debug.isDebugBuild && NodeController.bNodeLinkDebugLog)
                        Tc += ("Linked [" + DLChker(n) + "]");


                    // 接続先がおかしいならノーカンで
                    Vec2Int Target = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)n);
                    if (Target.x != -1)
                    {
                        // 分岐を検出してカウント
                        if (!bBranch)
                        {
                            // 一回目ならノーカン
                            bBranch = true;
                        }
                        else
                        {
                            // 二回目以降は分岐なので、枝カウンタを+1
                            Tc.Branch++;
                        }

                        // 次へ引き渡す
                        Node TgtNode = nodeControllerScript.GetNodeScript(Target);
                        if (TgtNode.isOutPuzzle)
                        {
                        // 接続先が壁なら処理飛ばして枝解決
                            Tc.Branch--;
                        }
                        else
                        {
                            // 周囲のActionをトリガーさせる
                            Observable
                            .Return(TgtNode)
                            .Subscribe(_ =>
                            {
                                TgtNode.NodeCheckAction(Tc, (_eLinkDir)((n + 3 >= 6) ? (n + 3 - 6) : (n + 3)));
                            }).AddTo(this);
                        }
                    }
                }
                else
                {
                    // 隣と繋がってないので、枝未完成として登録
                    Tc += ("NotFin");
                    Tc.NotFin = true;
                }
            }
        }
        if (Tc.NotFin && Tc.Branch > 0)
        {
            Tc.Branch--;
        }
    }

    public override string ToString()
    {
        return "Node[" + NodeID.y.ToString() + "][" + NodeID.x.ToString() + "]";
    }

    //ノードにタイプ・テクスチャ・道ビット
    public void SetNodeType(_eNodeType type)
    {
        //ビットと回転角度をリセット
        bitLink.SetAll(false);
        transform.rotation = Quaternion.identity;

        bitLink[0] = true;

        //ビットタイプ・テクスチャを設定
        switch(type)
        {
            case _eNodeType.CAP: 
                break;
            case _eNodeType.HUB2_A:
                bitLink[3] = true;
                
                break;
            case _eNodeType.HUB2_B:
                bitLink[2] = true;
                break;
            case _eNodeType.HUB2_C:
                bitLink[1] = true;
                break;
            case _eNodeType.HUB3_A:
                bitLink[2] = true;
                bitLink[4] = true;
                break;
            case _eNodeType.HUB3_B:
                bitLink[3] = true;
                bitLink[5] = true;
                break;
            //case _eNodeType.HUB3_C:
            //    bitLink[1] = true;
            //    bitLink[2] = true;
            //    break;

            default:
                break;
        }
        meshRenderer.material = Resources.Load<Material>(HEX_TEXTURE[(int)type]);//(Sprite)Object.Instantiate(nodeControllerScript.GetSprite(type));

        //ランダムに回転
        float angle = 0.0f;
        for (int i = 0; i < UnityEngine.Random.Range(0, 6); i++)
        {
            BitLinkRotate();
            angle -= ROT_HEX_ANGLE;
        }
        transform.rotation = Quaternion.Euler(new Vector3(0.0f,0.0f,angle));

    }

    public bool GetLinkDir(_eLinkDir parentDir)
    {
        return bitLink[(int)parentDir];
    }

    //いちいちfor回すのはどうにかしたい
    public int GetLinkNum()
    {
        int n = 0;
        for(int i = 0; i < (int)_eLinkDir.MAX; i++)
        {
            n += bitLink[i] ? 1 : 0;
        }
        return n;
    }

    private bool CheckOutPuzzle()
    {
        isOutPuzzle = (nodeID.y < 1 || nodeControllerScript.Col - 2 < nodeID.y || nodeID.x < 1 || nodeControllerScript.AdjustRow(nodeID.y) - 2 < nodeID.x);
        return isOutPuzzle;
    }

    public float GetLeftPos() {
        float left = transform.position.x;
        left -= nodeControllerScript.NodeSize.x * 0.5f;

        return left;
    }

    public float GetRightPos() {
        float right = transform.position.x;
        right += nodeControllerScript.NodeSize.x * 0.5f;

        return right;
    }

    public float GetTopPos() {
        float top = transform.position.y;
        top += nodeControllerScript.NodeSize.y * 0.5f;

        return top;
    }

    public float GetBottomPos() {
        float bottom = transform.position.y;
        bottom -= nodeControllerScript.NodeSize.y * 0.5f;

        return bottom;
    }

    public void StopTween() {
        transform.DOKill();
    }

    public void CopyParameter(Node copy) {
        meshRenderer.material = copy.meshRenderer.material;
        transform.rotation = copy.transform.rotation;
        bitLink = copy.bitLink;
    }
}
