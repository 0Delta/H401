using UnityEngine;
using System.Collections;
using DG.Tweening;
using UniRx;
using BitArrayExtension;
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
    public bool IsAction
    {
        set { isAction = value; }
    }
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

    [SerializeField]private float colorDuration = 0.0f;
                                                   
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

    static public void SetNodeController(NodeController nodeControllerScript) {
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
    public string LinkDirToString(int n)
        {
        return LinkDirToString((_eLinkDir)n);
    }
    public string LinkDirToString(_eLinkDir n)
            {
        string[] DbgLinkStr = { "RU", "R ", "RD", "LD", "L ", "LU" };
        return DbgLinkStr[(int)n];
            }

    // BitArrayのToStringもどき。デバック出力用。
    public string ToString(BitArray array)
    {
        string str = "";
        for (int n = 0;n<array.Length ; n++){
            str += (array[n] ? "1" : "0");
        }
        return str;
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
        Tc += (ToString() + "Action \n     Link : " + ToString(bitLink) + "\nNegibor : " + ToString(Negibor));
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
            Tc += ("Ground");
        }

        // この時点で枝が繋がっている事が確定
        meshRenderer.material.color = new Color(0.5f, 0.0f, 0.0f);   //とりあえず赤フィルターを掛けてみる
        Tc.NodeList.Add(this);  // チェッカに自身を登録しておく

        // 終端ノードであれば、周囲チェック飛ばす
        var TempBit = new BitArray(6);
        // 除外方向設定
        TempBit.SetAll(false);
        if (Link == _eLinkDir.NONE)
        {
            TempBit.Set((int)_eLinkDir.RD, true);
            TempBit.Set((int)_eLinkDir.LD, true);
            }
            else
        {
            TempBit.Set((int)Link, true);
            }
        TempBit.And(bitLink).Xor(bitLink);    // 自身の道とAND後、自身の道とXOR。
        if (TempBit.isZero())                           // 比較して一致なら除外方向以外に道がない = XOR後に全0なら終端
            {
            Tc.Branch--;                                // 終端ノードであればそこで終了
            return;
        }

        // 周囲のチェック
        // この時点で、TempBitは先が壁の道を除いた自分の道を示している。
        Tc += "ExcludeFrom MyWay : " + ToString(TempBit);
                
        if (TempBit.retAnd(Negibor.retNot()).isNotZero())  // 隣接ノードのうち、道が無い場所に自分の道が伸びてたらそこは途切れている。
                {
            Tc += ("NotFin");
            Tc.NotFin = true;                       // 隣と繋がってないので、枝未完成として登録
                }

        Tc += ("Negibor : " + ToString(Negibor));
        TempBit.And(Negibor);                       // 隣接ノードと繋がっている場所を特定
        Tc += "Linked : "+ToString(TempBit);
        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
                    {
            if (!TempBit[n]) { continue; }          // ビット立ってないならスキップ

            // お隣さんと繋がっているので、処理引き渡しの準備
            bChain = true;

            // デバック表示
            Tc += ("Linked [" + LinkDirToString(n) + "]");


            // 接続先がおかしいならノーカンで
            Vec2Int Target = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)n);
            if (Target.x == -1) { continue; }
                        
            // 分岐を検出してカウント
            if (!bBranch)
            {
                bBranch = true;     // 一回目ならノーカン
                    }
                    else
                    {
                Tc.Branch++;        // 二回目以降は分岐なので、枝カウンタを+1
                    }

            // 次へ引き渡す
            Node TgtNode = nodeControllerScript.GetNodeScript(Target);
            if (TgtNode.isOutPuzzle)
            {
                Tc.Branch--;        // 接続先が壁なら処理飛ばして枝解決
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
        //transform.rotation = Quaternion.identity;
        //フィールド変更時とかの回転で困ったので、ｚ回転だけ初期化するように
        Vector3 rot = transform.eulerAngles;
        rot.z = 0.0f;
        //transform.eulerAngles.Set(rot.x,rot.y,0.0f);
        //transform.localEulerAngles.Set(rot.x, rot.y, 0.0f);

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

            default:
                break;
        }
        meshRenderer.material = nodeControllerScript.GetMaterial((int)type);//(Sprite)Object.Instantiate(nodeControllerScript.GetSprite(type));

        //ランダムに回転
        float angle = 0.0f;
        for (int i = 0; i < UnityEngine.Random.Range(0, 6); i++)
        {
            BitLinkRotate();
            angle -= ROT_HEX_ANGLE;
        }
        //rot = transform.eulerAngles;
        rot.z += angle;

        transform.rotation = Quaternion.identity;
        transform.Rotate(rot);
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

    public void ChangeEmissionColor(int colorNum)
    {
        meshRenderer.material.EnableKeyword("_EMISSION");
        meshRenderer.material.DOColor(nodeControllerScript.GetNodeColor(colorNum),"_EmissionColor",colorDuration);
    }
}
