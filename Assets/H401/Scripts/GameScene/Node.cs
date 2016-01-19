using BitArrayExtension;
using System.Collections;
using UniRx;
using UnityEngine;
using DG.Tweening;
using RandExtension;

//using Assets.Scripts.Utils;

public class Node : MonoBehaviour
{
    private string NodeDebugLog = "";
    public string DebugLog { get { return NodeDebugLog; } }

    static private readonly float ROT_HEX_ANGLE = 60.0f;      // 六角形パネルの回転角度
    static private readonly float IN_ACTION_POSZ = -0.5f;     // アクション中のZ座標

    [SerializeField]
    private float actionTime = 0.0f;       // アクションにかかる時間
    [SerializeField]
    private float scaleSize = 0.0f;       // タップ時の拡大サイズ
    [SerializeField]
    private float slideTime = 0.0f;       // スライド時の移動にかかる時間
    [SerializeField]
    private float slideStartTime = 0.0f;  // スライド開始時の移動にかかる時間
    [SerializeField]
    private float slideEndTime = 0.0f;    // スライド終了時の移動にかかる時間
    [SerializeField]
    private float colorDuration = 0.0f;

    static private NodeController nodeControllerScript = null;      // NodeController のスクリプト

    private Vec2Int nodeID = Vec2Int.zero;     // パネルリストのID
    public Vec2Int NodeID
    {
        get { return nodeID; }
    }

    #region // ノードが動いているかのフラグセット
    [System.Flags]
    enum ActionFlag
    {
        WAIT = 0,
        SLIDE_START = 1 << 0,
        SLIDING = 1 << 1,
        SLIDE_END = 1 << 2,
        TURNING = 1 << 3,
        FLIP = 1 << 4,
        ALL = (1 << 5) - 1,
    }
    ActionFlag ActStatus;

    public bool IsAction
    {
        get { return ActStatus != 0; }
    }
    public bool IsSlideStart
    {
        set { if (value) { ActStatus |= ActionFlag.SLIDE_START; } else { ActStatus &= (ActionFlag.ALL ^ ActionFlag.SLIDE_START); } }
        get { return (ActStatus & ActionFlag.SLIDE_START) != 0; }
    }
    public bool IsSlide
    {
        set { if (value) { ActStatus |= ActionFlag.SLIDING; } else { ActStatus &= (ActionFlag.ALL ^ ActionFlag.SLIDING); } }
        get { return (ActStatus & ActionFlag.SLIDING) != 0; }
    }
    public bool IsSlideEnd
    {
        set { if (value) { ActStatus |= ActionFlag.SLIDE_END; } else { ActStatus &= (ActionFlag.ALL ^ ActionFlag.SLIDE_END); } }
        get { return (ActStatus & ActionFlag.SLIDE_END) != 0; }
    }

    public bool IsTurning
    {
        set { if (value) { ActStatus |= ActionFlag.TURNING; } else { ActStatus &= (ActionFlag.ALL ^ ActionFlag.TURNING); } }
        get { return (ActStatus & ActionFlag.TURNING) != 0; }
    }
    public bool IsFlip
    {
        set { if (value) { ActStatus |= ActionFlag.FLIP; } else { ActStatus &= (ActionFlag.ALL ^ ActionFlag.FLIP); } }
        get { return (ActStatus & ActionFlag.FLIP) != 0; }
    }
    #endregion

    #region // ノードがパズル外かのフラグ
    /// <summary>
    /// 画面外フラグ
    /// </summary>
    private bool isOutScreen = false;
    public bool IsOutScreen
    {
        set { isOutScreen = value; }
        get { return isOutScreen; }
    }
    /// <summary>
    /// パズル外フラグ 
    /// フレームノードの裏ならTRUE
    /// </summary>
    public bool IsOutPuzzle
    {
        get { return (nodeID.y < 1 || nodeControllerScript.Col - 2 < nodeID.y || nodeID.x < 1 || nodeControllerScript.AdjustRow(nodeID.y) - 2 < nodeID.x); }
    }
    /// <summary>
    /// 地面フラグ
    /// ノードIDが Y=0 ならTRUE
    /// </summary>
    public bool IsGroundNode
    {
        get { return (NodeID.y == 0); }
    }
    #endregion

    public BitArray bitLink = new BitArray(6);  //道の繋がりのビット配列　trueが道
                                                //  5 0
                                                // 4   1
                                                //  3 2  とする
                                                /*
    //応急処置
    public BitArray bitLink {
        get {
            //1.1の時だけ＋１回転した値を返す
            if (nodeID.x == 1 && nodeID.y == 1)
            {
                BitArray bl = new BitArray(6);
                bl = _bitLink;
                bool bitLast = bl[5];
                for(int i = 5; i > 0; i--)
                {
                    bl[i] = bl[i - 1];
                }
                bl[0] = bitLast;
                return bl;
            }
            else { return _bitLink; }  }
        set { _bitLink = value; }
    }*/
    private SpriteRenderer spriteRenderer = null;
    private NodeMask NodeMask = null;
    public NodeTemplate Temp = null;               // 使用したテンプレート
    private int _RotCounter = 0;
    public int RotCounter
    {
        get { return _RotCounter; }
        set { _RotCounter = value % 6; }
    }

    public SpriteRenderer SpriteRenderer
    {
        get { return spriteRenderer; }
    }
    
    private bool bChecked = false;

    public bool CheckFlag                       //走査済みフラグ 枝完成チェックに使用
    {
        get { return bChecked; }
        set { bChecked = value; }
    }

    private bool bChain;                        //枝がつながっているか？

    public bool ChainFlag
    {
        get { return bChain; }
        set { bChain = value; }
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        var ret = MonoBehaviour.Instantiate(Resources.Load("Prefabs/GameScene/NodeMask")) as GameObject;
        ret.transform.SetParent(transform);
        NodeMask = GetComponentInChildren<NodeMask>();
        NodeMask.transform.localPosition = new Vector3(0f, 0f, 0f);
        NodeMask.transform.localScale = new Vector3(1f, 1f, 1f);
        NodeMask.Enabled = false;
    }

    // Use this for initialization
    private void Start()
    {
        NodeDebugLog += "Start\n";

        // Transformを矯正
        Observable
            .EveryUpdate()
            .Select(_ => !IsSlideEnd)
            .DistinctUntilChanged()
            .Where(x => x)
            .Subscribe(_ =>
            {
                NodeDebugLog += "ForceTween : ID [" + nodeID.y + "][" + nodeID.x + "]\n";
                transform.DOMove(nodeControllerScript.NodePlacePosList[nodeID.y][nodeID.x], 0.1f);
            }).AddTo(this);

        // 回転処理
        Observable
            .EveryUpdate()
            .Select(_ => _RotCounter)
            .DistinctUntilChanged()
            .Subscribe(_ =>
            {
                NodeDebugLog += "Rotation : " + _RotCounter + "\n";
                IsTurning = true;                                                    // アクション開始
                Vector3 Rot = new Vector3(0, 0, ROT_HEX_ANGLE * (6 - RotCounter));  // 回転角確定
                transform.DOLocalRotate(Rot, actionTime)                            // DoTweenで回転
                .OnComplete(() =>
                {
                    this.DOKill();
                }).OnKill(() =>
                {
                    NodeDebugLog += "RotationComplete : " + _RotCounter + "\n";
                    BitLinkRotate(_RotCounter);                                     // 終了と同時にビット変更、アクション終了。

                    IsTurning = false;
                });
            }).AddTo(this);

        // ノードの色
        Observable
            .EveryUpdate()
            .Select(x => bChain)
            .DistinctUntilChanged()
            .Subscribe(x =>
            {
                NodeDebugLog += "ChangeEmission : " + bChain + "\n";
                if (x == true)
                {
                    ChangeEmissionColor(1);
                    NodeMask.Enabled = true;
                }
                else {
                    ChangeEmissionColor(0);
                    NodeMask.Enabled = false;
                }
            });
        ChangeEmissionColor(0);

        // フラグ不正修正
        Observable
            .EveryUpdate()
            .Where(_ => IsAction)
            .Select(_ => transform)
            .DistinctUntilChanged()
            .ThrottleFrame(5)
            .Subscribe(x =>
            {
                print("reset");
                NodeDebugLog += "Force Flag Reset";
                IsTurning = false;
                IsSlideStart = false;
                IsSlide = false;
                IsSlideEnd = false;
                IsFlip = false;
            });
    }

    public override string ToString()
    {
        return "Node[" + NodeID.y.ToString() + "][" + NodeID.x.ToString() + "]";
    }

    public void RegistNodeID(int row, int col)
    {
        nodeID.x = row;
        nodeID.y = col;
    }

    static public void SetNodeController(NodeController nodeControllerScript)
    {
        Node.nodeControllerScript = nodeControllerScript;
    }

    #region // ノードの回転
    // ノード回転処理
    public void RotationNode(bool NoWait = false, bool Reverse = false)
    {
        // 画面外ノードなら未処理
        if (IsOutScreen)
            return;

        // スライド中なら回転・拡縮は未処理
        if (IsSlide)
            return;

        // ----- 回転処理
        // ノーウエイト版。フィーバー時の配置に使用
        if (NoWait)
        {
            // アクションキャンセル
            IsTurning = false;
            // ビット配列まわして
            if (Reverse)
            {
                // 逆回転なら4回追加
                RotCounter = RotCounter + 4;
            }
            RotCounter = RotCounter + 1;
            BitLinkRotate(_RotCounter);
            // 強制離脱
            return;
        }

        // 回転処理
        transform.DOKill();
        transform.DOMoveZ(IN_ACTION_POSZ, 0.0f);

        RotCounter = RotCounter + 1;

        // 拡縮処理(※回転と同じように、補正処理が必要)
        transform.DOScale(scaleSize, actionTime * 0.5f).SetLoops(2, LoopType.Yoyo)
            .OnComplete(() => {
                transform.DOMoveZ(0.0f, 0.0f);
            });
    }

    //道のビット配列を回転させる bitarrayに回転シフトがなかった
    private void BitLinkRotate(int RotCnt = -1)
    {
        if (0 <= RotCnt && RotCnt < 6)
        {
            // テンプレから回転数を指定してシフト
            bool[] basebit = Temp.LinkDir.Clone() as bool[];

            for (int n = 0; n < 6 - RotCnt; n++)
            {
                bitLink[RotCnt + n] = basebit[n];
            }
            for (int n = 0; n < RotCnt; n++)
            {
                bitLink[n] = basebit[(6 - RotCnt) + n];
            }
        }
        else {
            //右回転（左シフト）
            bool b5 = bitLink[5];
            // char[] deb = new char[6];
            for (int i = 4; i >= 0; i--)
            {
                bitLink[i + 1] = bitLink[i];
            }
            bitLink[0] = b5;
        }
    }
    #endregion

    #region // ノードのスライド
    // ノード移動処理
    public void SlideNode(_eSlideDir dir, Vector2 pos)
    {
        // スライド方向が指定されていなければ未処理
        if (dir == _eSlideDir.NONE)
            return;

        // アクション開始
        IsSlide = true;

        transform.DOKill();

        float time = 0.0f;
        if (IsSlideStart)
        {
            time = slideStartTime;
            transform.DOMoveZ(IN_ACTION_POSZ, 0.0f);
        }
        else if (IsSlideEnd)
        {
            time = slideEndTime;
        }
        else {
            time = slideTime;
        }

        transform.DOMoveX(pos.x, time)
            .OnComplete(() => {
                IsSlide = false;
                if (IsSlideStart)
                    IsSlideStart = false;
                if (IsSlideEnd)
                {
                    IsSlideEnd = false;
                    transform.DOMoveZ(0.0f, 0.0f);
                }
            });
        transform.DOMoveY(pos.y, time);
    }
    public void StopTween()
    {
        transform.DOKill();
    }
    public void StartSlide()
    {
        IsSlideStart = true;
    }
    public void EndSlide()
    {
        IsSlideEnd = true;
    }
    #endregion

    #region // 枝判定
    // お隣さんから自分への道を保持する。
    private BitArray Negibor = new BitArray(6);

    // 情報の更新
    public void UpdateNegibor()
    {
        NodeDebugLog += "NegiborUpdate \n";
        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
        {
            // 周辺ノードを一周ぐるっと
            Vec2Int Target = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)n);
            if (Target.x != -1)
            {
                Node TgtNode = nodeControllerScript.GetNodeScript(Target);
                if (TgtNode.IsOutPuzzle)
                {
                    // 壁だったら繋がっている判定
                    Negibor[n] = true;
                }
                else {
                    // ノードがあれば、自分と接している場所を見て、取得する。
                    TgtNode.BitLinkRotate(TgtNode.RotCounter); // 隣の情報を更新
                    Negibor[n] = nodeControllerScript.GetNodeScript(Target).bitLink[(n + 3 >= 6) ? (n + 3 - 6) : (n + 3)];
                }
            }
            else {
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

    // 隣が壁か判定(壁だったらtrueを返す)
    public bool CheckLinkedWall()
    {
        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
        {
            // 周辺ノードを一周ぐるっと
            Vec2Int Target = nodeControllerScript.GetDirNode(nodeID, (_eLinkDir)n);
            if (Target.x != -1)
            {
                Node TgtNode = nodeControllerScript.GetNodeScript(Target);
                if (TgtNode.IsOutPuzzle)
                {
                    return true;
                }
            }
        }

        return false;
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

    // 隣接判定、ノードごとの処理
    public void NodeCheckAction(NodeController.NodeLinkTaskChecker Tc, _eLinkDir Link)
    {
        NodeDebugLog += "NodeCheckAction. CheckerID : " + Tc.ID + "\n";
        // チェック済みでスキップ
        if (bChecked) { Tc.Branch--; return; }

        // 各種変数定義
        bool bBranch = false;
        bChecked = true;
        Tc.SumNode++;
        bChain = false;

        // お隣さんを更新
        UpdateNegibor();

        // 状態表示
        Tc += (ToString() + "Action \n     Link : " + bitLink.ToStringEx() + "\nNegibor : " + Negibor.ToStringEx());
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
        bChain = true;
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
        else {
            TempBit.Set((int)Link, true);
        }
        TempBit.And(bitLink).Xor(bitLink);    // 自身の道とAND後、自身の道とXOR。
        if (TempBit.isZero())                  // 比較して一致なら除外方向以外に道がない = XOR後に全0なら終端
        {
            Tc.Branch--;                      // 終端ノードであればそこで終了
            return;
        }

        // 周囲のチェック
        // この時点で、TempBitは先が壁の道を除いた自分の道を示している。
        Tc += "ExcludeFrom MyWay : " + TempBit.ToStringEx();

        for (int n = 0; n < (int)_eLinkDir.MAX; n++)
        {
            var TempBit2 = new BitArray(6);
            // 隣接ノードのうち、道が無い場所に自分の道が伸びてたらそこは途切れている。
            TempBit2 = TempBit.retAnd(Negibor.retNot());
            // ノード繋がってない
            if (TempBit2[n])
            {
                nodeControllerScript.unChainController.AddObj(this, (_eLinkDir)n);
                Tc.NotFin = true;                               // 隣と繋がってないので、枝未完成として登録
            }
        }

        Tc += ("Negibor : " + Negibor.ToStringEx());
        TempBit.And(Negibor);                       // 隣接ノードと繋がっている場所を特定
        Tc += "Linked : " + TempBit.ToStringEx();
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
            else {
                Tc.Branch++;        // 二回目以降は分岐なので、枝カウンタを+1
            }

            // 次へ引き渡す
            Node TgtNode = nodeControllerScript.GetNodeScript(Target);
            if (TgtNode.IsOutPuzzle)
            {
                Tc.Branch--;        // 接続先が壁なら処理飛ばして枝解決
            }
            else {
                // 周囲のActionをトリガーさせる
                Observable
                .Return(TgtNode)
                .Subscribe(_ => {
                    TgtNode.NodeCheckAction(Tc, (_eLinkDir)((n + 3 >= 6) ? (n + 3 - 6) : (n + 3)));
                }).AddTo(this);
            }
        }

        if (Tc.NotFin && Tc.Branch > 0)
        {
            Tc.Branch--;
        }
    }
    #endregion

    //ノードにタイプ・テクスチャ・道ビット
    public void SetNodeType(NodeTemplate type, int Rot = -1)
    {
        NodeDebugLog += "SetNodeType. TempID : " + type.ID + "\n";
        // 使用したテンプレを記憶
        Temp = type;

        //テクスチャを設定
        SpriteRenderer.sprite = nodeControllerScript.GetSprite(Temp.SpriteIdx);
        NodeMask.SetSprite(nodeControllerScript.GetMaskSprite(Temp.MaskIdx));

        //ランダムに回転
        int RotI = RandomEx.RangeforInt(0, 5);
        RotCounter = (Rot == -1 ? RotI : Rot);

        // 色をリセット
        ChangeEmissionColor(0);
    }

    #region // ゲッター
    public bool GetLinkDir(_eLinkDir parentDir)
    {
        return bitLink[(int)parentDir];
    }

    //いちいちfor回すのはどうにかしたい
    public int GetLinkNum()
    {
        int n = 0;
        for (int i = 0; i < (int)_eLinkDir.MAX; i++)
        {
            n += bitLink[i] ? 1 : 0;
        }
        return n;
    }

    public float GetLeftPos()
    {
        float left = transform.position.x;
        left -= nodeControllerScript.NodeSize.x * 0.5f;

        return left;
    }

    public float GetRightPos()
    {
        float right = transform.position.x;
        right += nodeControllerScript.NodeSize.x * 0.5f;

        return right;
    }

    public float GetTopPos()
    {
        float top = transform.position.y;
        top += nodeControllerScript.NodeSize.y * 0.5f;

        return top;
    }

    public float GetBottomPos()
    {
        float bottom = transform.position.y;
        bottom -= nodeControllerScript.NodeSize.y * 0.5f;

        return bottom;
    }
    #endregion

    public void CopyParameter(Node copy)
    {
        NodeDebugLog += "Copy from " + copy.ToString() + "\n";
        transform.rotation = copy.transform.rotation;
        RotCounter = copy.RotCounter;
    }

    public void ChangeEmissionColor(int colorNum)
    {
        //SpriteRenderer.sharedMaterial.EnableKeyword("_Color");
        //SpriteRenderer.sharedMaterial.DOColor(nodeControllerScript.GetNodeColor(colorNum), "_Color", colorDuration);
    }

    public void FlipNode(float repRotateTime, Ease easeType)
    {
        IsFlip = true;
        Vector3 angle = transform.localEulerAngles;
        angle.y += 90.0f;
        transform.DOLocalRotate(angle, (repRotateTime / 2.0f))
            .OnComplete(() => {
                IsFlip = false;
            })
            .SetEase(easeType);
    }
    public void ForceRotateWithBit()
    {
        transform.DOKill();
        Vector3 angle = transform.localEulerAngles;
        angle.z = ROT_HEX_ANGLE * (6 - RotCounter);
        transform.rotation = Quaternion.identity;
        transform.Rotate(angle);
    }
}
