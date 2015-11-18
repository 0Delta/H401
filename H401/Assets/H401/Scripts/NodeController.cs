using UnityEngine;
using System.Collections;
using UniRx;

using DG.Tweening;

/*  リストIDに関して

            (n,n)
     ・・・◇◇◇
    ・・・・・・
     ・・・・・・
    ◇◇◇・・・
     ◇◇◇・・・
    ◇◇◇・・・
    (0,0)
*/

public class NodeController : MonoBehaviour {

    private const float ADJUST_PIXELS_PER_UNIT = 0.01f;     // Pixels Per Unit の調整値
    private readonly Square CAMERA_AREA = new Square(0.0f, 0.0f, 5.0f * 2.0f * 750.0f / 1334.0f, 5.0f * 2.0f);    // カメラの描画領域

    [SerializeField] private int row = 0;       // 横配置数
    [SerializeField] private int col = 0;       // 縦配置数 偶数行は＋１とする
    [SerializeField] private GameObject nodePrefab = null;       // パネルのプレハブ
    [SerializeField] private float widthMargin  = 0.0f;  // パネル位置の左右間隔の調整値
    [SerializeField] private float heightMargin = 0.0f;  // パネル位置の上下間隔の調整値

    public bool bNodeLinkDebugLog;                       // ノード接続に関するデバックログを有効にするか

    private GameObject[,]   nodePrefabs;     // パネルのプレハブリスト
    private Node[,]         nodeScripts;     // パネルのnodeスクリプトリスト

    private Vector2 nodeSize = Vector2.zero;    // 描画するパネルのサイズ

    private bool        isDrag          = false;                // マウスドラッグフラグ
    private Vec2Int     beforeTapNodeID = Vec2Int.zero;         // 移動させたいノードのID
    private Vec2Int     afterTapNodeID  = Vec2Int.zero;         // 移動させられるノードのID(移動方向を判定するため)
    private _eSlideDir  slideDir        = _eSlideDir.NONE;      // スライド中の方向
    private Vector2     moveNodeInitPos = Vector2.zero;         // 移動中ノードの移動開始位置
    private Vector2     moveNodeDist    = Vector2.zero;         // 移動中ノードの基本移動量(移動方向ベクトル)
    private Vector2     moveNodeDistAbs = Vector2.zero;         // 移動中ノードの基本移動量の絶対値

    private Vector2 startTapPos = Vector2.zero;     // タップした瞬間の座標
    private Vector2 tapPos      = Vector2.zero;     // タップ中の座標
    private Vector2 prevTapPos  = Vector2.zero;     // 前フレームのタップ座標

    private Vector2 slideLeftUpPerNorm   = Vector2.zero;     // 左上ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)
    private Vector2 slideLeftDownPerNorm = Vector2.zero;     // 左下ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)
    
    private FieldLevelInfo fieldLevel;

    private float RatioSum = 0.0f;                           //合計割合

    //[SerializeField] private Sprite[] cashSprites = new Sprite[6];

    [SerializeField] LevelTables levelTables = null;

    //ノードの配置割合を記憶しておく

    public int Row {
        get { return this.row; }
    }
    
    public int Col {
        get { return this.col; }
    }

    public bool IsDrag {
        set { this.isDrag = value; }
        get { return this.isDrag; }
    }

    public Vec2Int BeforeTapNodeID {
        set { this.beforeTapNodeID = value; }
        get { return this.beforeTapNodeID; }
    }

    public Vec2Int AfterTapNodeID {
        set { this.afterTapNodeID = value; }
        get { return this.afterTapNodeID; }
    }

    public _eSlideDir SlideDir {
        get { return slideDir; }
    }

    public Vector2 NodeSize {
        get { return nodeSize; }
    }
    
    private Score scoreScript;          //スコアのスクリプト
    private LimitTime timeScript;            //制限時間のスクリプト
    

    void Awake() {
        nodePrefabs = new GameObject[row, col];
        nodeScripts = new Node[row, col];

    }
    
	// Use this for initialization
	void Start () {
        scoreScript = GameObject.Find("ScoreNum").GetComponent<Score>();
        timeScript = GameObject.Find("LimitTime").GetComponent<LimitTime>();
        fieldLevel = levelTables.GetFieldLevel(0);
            
        // ----- パネル準備
        // 描画するパネルの大きさを取得
        Vector3 pos = transform.position;
        nodeSize.x = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.width * nodePrefab.transform.localScale.x * ADJUST_PIXELS_PER_UNIT;
        nodeSize.y = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.height * nodePrefab.transform.localScale.y * ADJUST_PIXELS_PER_UNIT;
        nodeSize.x -= widthMargin * ADJUST_PIXELS_PER_UNIT;
        nodeSize.y -= heightMargin * ADJUST_PIXELS_PER_UNIT;

        RatioSum = fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3;  //全体割合を記憶


        // パネルを生成
        for(int i = 0; i < col; ++i) {
            // パネルの配置位置を調整(Y座標)
            pos.y = transform.position.y + nodeSize.y * -(col * 0.5f - (i + 0.5f));
            for (int j = 0; j < row; ++j) {
                // パネルの配置位置を調整(X座標)
                pos.x = i % 2 == 0 ? transform.position.x + nodeSize.x * -(row * 0.5f - (j + 0.25f)) : transform.position.x + nodeSize.x * -(row * 0.5f - (j + 0.75f));

                // 生成
        	    nodePrefabs[j,i] = (GameObject)Instantiate(nodePrefab, pos, transform.rotation);
                nodeScripts[j,i] = nodePrefabs[j,i].GetComponent<Node>();
                nodePrefabs[j,i].transform.parent = transform;

                // パネルの位置(リストID)を登録
                nodeScripts[j,i].RegistNodeID(j, i);

                //ランダムでノードの種類と回転を設定
                ReplaceNode(nodeScripts[j, i]);


//                if (j == 0 || i == 0 || j == row - 1 || i == col - 1)
//                    nodeScripts[j, i].SpRenderer.color = new Color(0.1f, 0.1f, 0.1f);
            }
        }
        
        // パネルに情報を登録
        nodeScripts[0,0].SetNodeController(this);

        // 画面外ノードを登録
        for(int i = 0; i < row; ++i) {
            for(int j = 0; j < col; ++j) {
                CheckOutScreen(i, j);
            }
        }

        // スライドベクトルの垂線を算出
        Vector3 leftUp = nodePrefabs[0,1].transform.position - nodePrefabs[1,0].transform.position;
        Vector3 leftDown = nodePrefabs[0,1].transform.position - nodePrefabs[0,0].transform.position;
        Matrix4x4 mtx = Matrix4x4.identity;
        mtx.SetTRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 90.0f), new Vector3(1.0f, 1.0f, 1.0f));
        leftUp = mtx.MultiplyVector(leftUp).normalized;
        leftDown = mtx.MultiplyVector(leftDown).normalized;
        slideLeftUpPerNorm = new Vector2(leftUp.x, leftUp.y);
        slideLeftDownPerNorm = new Vector2(leftDown.x, leftDown.y);

        // ----- インプット処理
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => {
                isDrag = true;

                // スライド処理
                if(slideDir != _eSlideDir.NONE) {
                    Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    prevTapPos = tapPos;
                    tapPos = new Vector2(worldTapPos.x, worldTapPos.y);
                    SlantMove();
                    LoopBackNode();
                    AllCheckOutScreen();
                }
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => {
                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startTapPos = new Vector2(worldTapPos.x, worldTapPos.y);
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => {
                AdjustNodeStop();

                isDrag   = false;
                slideDir = _eSlideDir.NONE;

                beforeTapNodeID = Vec2Int.zero;
                afterTapNodeID  = Vec2Int.zero;
            })
            .AddTo(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
        // @デバッグ用
        //for(int i = 0; i < row; ++i) {
        //    for(int j = 0; j < col; ++j) {
        //        if(nodeScripts[i,j].IsOutScreen)
        //            nodeScripts[i, j].SpRenderer.color = new Color(0.1f, 0.1f, 1.0f);
        //        else
        //            nodeScripts[i, j].SpRenderer.color = new Color(1.0f, 1.0f, 1.0f);
        //    }
        //}
	}
    
    void SlantMove() {
        // スライド対象となるノードの準備
        Vector2 pos       = tapPos;                     // 移動位置
        Vector2 slideDist = tapPos - startTapPos;       // スライド量
        Vector2 moveDist  = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, slideDist);      // 斜め移動量
        Vector2 deltaSlideDist = tapPos - prevTapPos;   // 前回フレームからのスライド量
        float checkDir    = 0.0f;                       // スライド方向チェック用

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // スライド方向を再計算
                if(tapPos.x - prevTapPos.x < 0.0f) {
                    slideDir = _eSlideDir.LEFT;
                } else if(tapPos.x - prevTapPos.x > 0.0f) {
                    slideDir = _eSlideDir.RIGHT;
                }

                // タップしているノードを移動
                pos.y = nodePrefabs[beforeTapNodeID.x,beforeTapNodeID.y].transform.position.y;
                nodeScripts[beforeTapNodeID.x,beforeTapNodeID.y].SlideNode(slideDir, pos);

                // タップしているノードより左側のノードを移動
                for(int i = beforeTapNodeID.x - 1, j = 1; i >= 0; --i, ++j) {
                    pos = tapPos - moveNodeDistAbs * j;
                    pos.y = nodePrefabs[i,beforeTapNodeID.y].transform.position.y;
                    nodeScripts[i,beforeTapNodeID.y].SlideNode(slideDir, pos);
                }
                // タップしているノードより右側のノードを移動
                for(int i = beforeTapNodeID.x + 1, j = 1; i < row; ++i, ++j) {
                    pos = tapPos + moveNodeDistAbs * j;
                    pos.y = nodePrefabs[i,beforeTapNodeID.y].transform.position.y;
                    nodeScripts[i,beforeTapNodeID.y].SlideNode(slideDir, pos);
                }
                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftUpPerNorm);
                if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.LEFTUP;
                } else if(checkDir < 0.0f) {
                    slideDir = _eSlideDir.RIGHTDOWN;
                }
                
                // タップしているノードを移動
                pos = moveNodeInitPos + moveDist;
                nodeScripts[beforeTapNodeID.x, beforeTapNodeID.y].SlideNode(slideDir, pos);

                // タップしているノードより左上側のノードを移動
                for (int i = beforeTapNodeID.y % 2 == 0 ? i = beforeTapNodeID.x - 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y + 1, k = 1; i >= 0 && j < col; ++j, ++k) {
                    pos.x = moveNodeInitPos.x + moveDist.x - moveNodeDistAbs.x * k;
                    pos.y = moveNodeInitPos.y + moveDist.y + moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);
                    
                    if(j % 2 == 0)
                        --i;
                }
                // タップしているノードより右下側のノードを移動
                for (int i = beforeTapNodeID.y % 2 != 0 ? i = beforeTapNodeID.x + 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y - 1, k = 1; i < row && j >= 0; --j, ++k) {
                    pos.x = moveNodeInitPos.x + moveDist.x + moveNodeDistAbs.x * k;
                    pos.y = moveNodeInitPos.y + moveDist.y - moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);

                    if(j % 2 != 0)
                        ++i;
                }
                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftDownPerNorm);
                if(checkDir < 0.0f) {
                    slideDir = _eSlideDir.LEFTDOWN;
                } else if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.RIGHTUP;
                }

                // タップしているノードを移動
                pos = moveNodeInitPos + moveDist;
                nodeScripts[beforeTapNodeID.x, beforeTapNodeID.y].SlideNode(slideDir, pos);

                // タップしているノードより右上側のノードを移動
                for (int i = beforeTapNodeID.y % 2 != 0 ? i = beforeTapNodeID.x + 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y + 1, k = 1; i < row && j < col; ++j, ++k) {
                    pos.x = moveNodeInitPos.x + moveDist.x + moveNodeDistAbs.x * k;
                    pos.y = moveNodeInitPos.y + moveDist.y + moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);
                    
                    if(j % 2 != 0)
                        ++i;
                }
                // タップしているノードより左下側のノードを移動
                for (int i = beforeTapNodeID.y % 2 == 0 ? i = beforeTapNodeID.x - 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y - 1, k = 1; i >= 0 && j >= 0; --j, ++k) {
                    pos.x = moveNodeInitPos.x + moveDist.x - moveNodeDistAbs.x * k;
                    pos.y = moveNodeInitPos.y + moveDist.y - moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);

                    if(j % 2 == 0)
                        --i;
                }
                break;

            default:
                break;
        }
    }

    //移動したいノードを確定
    //ドラッグを算出し移動したい方向列を確定
    //ドラッグされている間、列ごと移動、
        //タップ点からスワイプ地点まで座標の差分を算出し
        //列のすべてのノードをその位置へ移動させる
    //離すと一番近いノード確定位置まで調整

    public void StartSlideNodes() {
        int subRowID = afterTapNodeID.x - beforeTapNodeID.x;   // ノードIDの差分(横方向)
        int subColID = afterTapNodeID.y - beforeTapNodeID.y;   // ノードIDの差分(縦方向)
        moveNodeDist = new Vector2(nodePrefabs[afterTapNodeID.x, afterTapNodeID.y].transform.position.x,   // スライド方向ベクトル兼移動量を算出
                                    nodePrefabs[afterTapNodeID.x, afterTapNodeID.y].transform.position.y)
                    - new Vector2(nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y].transform.position.x,
                                    nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y].transform.position.y);
        moveNodeInitPos = nodePrefabs[beforeTapNodeID.x,beforeTapNodeID.y].transform.position;      // ノードの移動開始位置を保存
        
        // スライド方向を設定
        if(subRowID == -1 && subColID == 0) {
           // 左にスライド
            slideDir = _eSlideDir.LEFT;
        } else if(subRowID == 1 && subColID == 0) {
            // 右にスライド
            slideDir = _eSlideDir.RIGHT;
        } else if(subColID == 1 && moveNodeDist.x < 0.0f && moveNodeDist.y > 0.0f) {
            // 左上にスライド
            slideDir = _eSlideDir.LEFTUP;
        } else if (subColID == -1 && moveNodeDist.x < 0.0f && moveNodeDist.y < 0.0f) {
            // 左下にスライド
            slideDir = _eSlideDir.LEFTDOWN;
        } else if(subColID == 1 && moveNodeDist.x > 0.0f && moveNodeDist.y > 0.0f) {
            // 右上にスライド
            slideDir = _eSlideDir.RIGHTUP;
        } else if(subColID == -1 && moveNodeDist.x > 0.0f && moveNodeDist.y < 0.0f) {
            // 右下にスライド
            slideDir = _eSlideDir.RIGHTDOWN;
        }

        // 絶対値を算出
        moveNodeDistAbs.x = Mathf.Abs(moveNodeDist.x);
        moveNodeDistAbs.y = Mathf.Abs(moveNodeDist.y);
        
        SlantMove();
    }

    void LoopBackNode()
    {
        Vec2Int limitNodeID         = Vec2Int.zero;     // スライド方向の端のノードID
        Vec2Int reverseLimitNodeID  = Vec2Int.zero;     // スライド方向の逆端のノードID

        switch (slideDir)
        {
            // ----- 左にスライド
            case _eSlideDir.LEFT:
                // 回り込み調整用ノードを右端に移動
                if(nodeScripts[0,beforeTapNodeID.y].IsOutScreen) {
                    SortLeftOutNode();
                }

                // 左端ノードが画面外に出ていないかチェック
                if(nodeScripts[0,beforeTapNodeID.y].GetLeftPos() < CAMERA_AREA.left) {
                    nodeScripts[row - 1,beforeTapNodeID.y].CopyParameter(nodeScripts[0,beforeTapNodeID.y]);
                    nodeScripts[row - 1,beforeTapNodeID.y].IsOutScreen = false;
                }

                // 左端ノードが画面外に出たかチェック
                if(nodeScripts[0,beforeTapNodeID.y].GetRightPos() <= CAMERA_AREA.left) {
                    nodeScripts[0,beforeTapNodeID.y].IsOutScreen = true;
                    SortLeftOutNode();
                }

                break;
                
            // ----- 右にスライド
            case _eSlideDir.RIGHT:
                // 回り込み調整用ノードを左端に移動
                if (nodeScripts[row - 1, beforeTapNodeID.y].IsOutScreen) {
                    SortRightOutNode();
                }

                // 右端ノードが画面外に出ていないかチェック
                if (nodeScripts[row - 1, beforeTapNodeID.y].GetRightPos() > CAMERA_AREA.right)
                {
                    nodeScripts[0, beforeTapNodeID.y].CopyParameter(nodeScripts[row - 1, beforeTapNodeID.y]);
                    nodeScripts[0, beforeTapNodeID.y].IsOutScreen = false;
                }

                // 右端ノードが画面外に出たかチェック
                if (nodeScripts[row - 1, beforeTapNodeID.y].GetLeftPos() >= CAMERA_AREA.right) {
                    nodeScripts[row - 1, beforeTapNodeID.y].IsOutScreen = true;
                    SortRightOutNode();
                }

                break;

            // ----- 左上にスライド
            case _eSlideDir.LEFTUP:
                // 左上端のノードIDを検索
                limitNodeID.x = beforeTapNodeID.x;
                limitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(limitNodeID.x, limitNodeID.y, _eLinkDir.LU).x > -1) {
                    limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LU);
                }
                // 右下端のノードIDを検索
                reverseLimitNodeID.x = beforeTapNodeID.x;
                reverseLimitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(reverseLimitNodeID.x, reverseLimitNodeID.y, _eLinkDir.RD).x > -1) {
                    reverseLimitNodeID = GetDirNode(reverseLimitNodeID, _eLinkDir.RD);
                }

                // 回り込み調整用ノードが複数ある場合、右下端に一つだけ配置する
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RD);
                    }
                }
                // 回り込み調整用ノードを右下端に移動
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        SortLeftUpOutNode(limitNodeID);
                    }
                }

                // 左上端ノードが画面外に出ていないかチェック
                if( nodeScripts[limitNodeID.x,limitNodeID.y].GetLeftPos() < CAMERA_AREA.left ||
                    nodeScripts[limitNodeID.x,limitNodeID.y].GetTopPos()  > CAMERA_AREA.top) {
                    nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].CopyParameter(nodeScripts[limitNodeID.x,limitNodeID.y]);
                    nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen = false;
                }

                // 左上端ノードが画面外に出たかチェック
                if( nodeScripts[limitNodeID.x,limitNodeID.y].GetRightPos()  <= CAMERA_AREA.left ||
                    nodeScripts[limitNodeID.x,limitNodeID.y].GetBottomPos() >= CAMERA_AREA.top) {
                    nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true &&
                        nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen != true)
                        SortLeftUpOutNode(limitNodeID);
                }

                break;

            // ----- 右下にスライド
            case _eSlideDir.RIGHTDOWN:
                // 右下端のノードIDを検索
                limitNodeID.x = beforeTapNodeID.x;
                limitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(limitNodeID.x, limitNodeID.y, _eLinkDir.RD).x > -1) {
                    limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RD);
                }
                // 左上端のノードIDを検索
                reverseLimitNodeID.x = beforeTapNodeID.x;
                reverseLimitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(reverseLimitNodeID.x, reverseLimitNodeID.y, _eLinkDir.LU).x > -1) {
                    reverseLimitNodeID = GetDirNode(reverseLimitNodeID, _eLinkDir.LU);
                }
                
                // 回り込み調整用ノードが複数ある場合、左上端に一つだけ配置する
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LU);
                    }
                }
                // 回り込み調整用ノードを左上端に移動
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true) {
                    if (nodeScripts[limitNodeID.x, limitNodeID.y].IsOutScreen) {
                        SortRightDownOutNode(limitNodeID);
                    }
                }

                // 右下端ノードが画面外に出ていないかチェック
                if (nodeScripts[limitNodeID.x, limitNodeID.y].GetRightPos()  > CAMERA_AREA.right ||
                    nodeScripts[limitNodeID.x, limitNodeID.y].GetBottomPos() < CAMERA_AREA.bottom) {
                    nodeScripts[reverseLimitNodeID.x, reverseLimitNodeID.y].CopyParameter(nodeScripts[limitNodeID.x, limitNodeID.y]);
                    nodeScripts[reverseLimitNodeID.x, reverseLimitNodeID.y].IsOutScreen = false;
                }

                // 右下端ノードが画面外に出たかチェック
                if (nodeScripts[limitNodeID.x, limitNodeID.y].GetLeftPos() >= CAMERA_AREA.right ||
                    nodeScripts[limitNodeID.x, limitNodeID.y].GetTopPos()  <= CAMERA_AREA.bottom) {
                    nodeScripts[limitNodeID.x, limitNodeID.y].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true &&
                        nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen != true)
                        SortRightDownOutNode(limitNodeID);
                }

                break;

            // ----- 右上にスライド
            case _eSlideDir.RIGHTUP:
                // 右上端のノードIDを検索
                limitNodeID.x = beforeTapNodeID.x;
                limitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(limitNodeID.x, limitNodeID.y, _eLinkDir.RU).x > -1) {
                    limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RU);
                }
                // 左下端のノードIDを検索
                reverseLimitNodeID.x = beforeTapNodeID.x;
                reverseLimitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(reverseLimitNodeID.x, reverseLimitNodeID.y, _eLinkDir.LD).x > -1) {
                    reverseLimitNodeID = GetDirNode(reverseLimitNodeID, _eLinkDir.LD);
                }

                // 回り込み調整用ノードが複数ある場合、左下端に一つだけ配置する
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LD);
                    }
                }
                // 回り込み調整用ノードを左下端に移動
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        SortRightUpOutNode(limitNodeID);
                    }
                }

                // 右上端ノードが画面外に出ていないかチェック
                if( nodeScripts[limitNodeID.x,limitNodeID.y].GetRightPos() > CAMERA_AREA.right ||
                    nodeScripts[limitNodeID.x,limitNodeID.y].GetTopPos()   > CAMERA_AREA.top) {
                    nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].CopyParameter(nodeScripts[limitNodeID.x,limitNodeID.y]);
                    nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen = false;
                }

                // 右上端ノードが画面外に出たかチェック
                if( nodeScripts[limitNodeID.x,limitNodeID.y].GetLeftPos()   >= CAMERA_AREA.right ||
                    nodeScripts[limitNodeID.x,limitNodeID.y].GetBottomPos() >= CAMERA_AREA.top) {
                    nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true &&
                        nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen != true)
                        SortRightUpOutNode(limitNodeID);
                }

                break;

            // ----- 左下にスライド
            case _eSlideDir.LEFTDOWN:
                // 左下端のノードIDを検索
                limitNodeID.x = beforeTapNodeID.x;
                limitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(limitNodeID.x, limitNodeID.y, _eLinkDir.LD).x > -1) {
                    limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LD);
                }
                // 右上端のノードIDを検索
                reverseLimitNodeID.x = beforeTapNodeID.x;
                reverseLimitNodeID.y = beforeTapNodeID.y;
                while(GetDirNode(reverseLimitNodeID.x, reverseLimitNodeID.y, _eLinkDir.RU).x > -1) {
                    reverseLimitNodeID = GetDirNode(reverseLimitNodeID, _eLinkDir.RU);
                }
                
                // 回り込み調整用ノードが複数ある場合、右上端に一つだけ配置する
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen) {
                    if(nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen) {
                        limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RU);
                    }
                }
                // 回り込み調整用ノードを右上端に移動
                if(nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true) {
                    if (nodeScripts[limitNodeID.x, limitNodeID.y].IsOutScreen) {
                        SortLeftDownOutNode(limitNodeID);
                    }
                }

                // 左下端ノードが画面外に出ていないかチェック
                if (nodeScripts[limitNodeID.x, limitNodeID.y].GetLeftPos()   < CAMERA_AREA.left ||
                    nodeScripts[limitNodeID.x, limitNodeID.y].GetBottomPos() < CAMERA_AREA.bottom) {
                    nodeScripts[reverseLimitNodeID.x, reverseLimitNodeID.y].CopyParameter(nodeScripts[limitNodeID.x, limitNodeID.y]);
                    nodeScripts[reverseLimitNodeID.x, reverseLimitNodeID.y].IsOutScreen = false;
                }

                // 左下端ノードが画面外に出たかチェック
                if (nodeScripts[limitNodeID.x, limitNodeID.y].GetRightPos() <= CAMERA_AREA.left ||
                    nodeScripts[limitNodeID.x, limitNodeID.y].GetTopPos()   <= CAMERA_AREA.bottom) {
                    nodeScripts[limitNodeID.x, limitNodeID.y].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.x,reverseLimitNodeID.y].IsOutScreen != true &&
                        nodeScripts[limitNodeID.x,limitNodeID.y].IsOutScreen != true)
                        SortLeftDownOutNode(limitNodeID);
                }

                break;
        }
    }

    void AdjustNodeStop() {
        Vector2 pos         = Vector2.zero;
        Vector2 standardPos = Vector2.zero;
        Vector2 slideDist   = tapPos - startTapPos;       // スライド量

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y + 2].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y - 2].transform.position.x;
                }
                standardPos.y = nodePrefabs[beforeTapNodeID.x,beforeTapNodeID.y].transform.position.y;
                nodeScripts[beforeTapNodeID.x,beforeTapNodeID.y].SlideNode(slideDir, standardPos);

                // タップしているノードより左側のノードを移動
                for(int i = beforeTapNodeID.x - 1, j = 1; i >= 0; --i, ++j) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[i,beforeTapNodeID.y].SlideNode(slideDir, pos);
                }
                // タップしているノードより右側のノードを移動
                for(int i = beforeTapNodeID.x + 1, j = 1; i < row; ++i, ++j) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[i,beforeTapNodeID.y].SlideNode(slideDir, pos);
                }
                break;
                
            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y + 2].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y - 2].transform.position.x;
                }
                if(beforeTapNodeID.x + 1 < row) {
                    standardPos.y = nodePrefabs[beforeTapNodeID.x + 1,beforeTapNodeID.y].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[beforeTapNodeID.x - 1,beforeTapNodeID.y].transform.position.y;
                }
                nodeScripts[beforeTapNodeID.x,beforeTapNodeID.y].SlideNode(slideDir, standardPos);

                // タップしているノードより左上側のノードを移動
                for (int i = beforeTapNodeID.y % 2 == 0 ? i = beforeTapNodeID.x - 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y + 1, k = 1; i >= 0 && j < col; ++j, ++k) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * k;
                    pos.y = standardPos.y + moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);
                    
                    if(j % 2 == 0)
                        --i;
                }
                // タップしているノードより右下側のノードを移動
                for (int i = beforeTapNodeID.y % 2 != 0 ? i = beforeTapNodeID.x + 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y - 1, k = 1; i < row && j >= 0; --j, ++k) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * k;
                    pos.y = standardPos.y - moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);

                    if(j % 2 != 0)
                        ++i;
                }
                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y + 2].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.x, beforeTapNodeID.y - 2].transform.position.x;
                }
                if(beforeTapNodeID.x + 1 < row) {
                    standardPos.y = nodePrefabs[beforeTapNodeID.x + 1,beforeTapNodeID.y].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[beforeTapNodeID.x - 1,beforeTapNodeID.y].transform.position.y;
                }
                nodeScripts[beforeTapNodeID.x,beforeTapNodeID.y].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                for (int i = beforeTapNodeID.y % 2 != 0 ? i = beforeTapNodeID.x + 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y + 1, k = 1; i < row && j < col; ++j, ++k) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * k;
                    pos.y = standardPos.y + moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);
                    
                    if(j % 2 != 0)
                        ++i;
                }
                // タップしているノードより右下側のノードを移動
                for (int i = beforeTapNodeID.y % 2 == 0 ? i = beforeTapNodeID.x - 1 : i = beforeTapNodeID.x,
                        j = beforeTapNodeID.y - 1, k = 1; i >= 0 && j >= 0; --j, ++k) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * k;
                    pos.y = standardPos.y - moveNodeDistAbs.y * k;
                    nodeScripts[i, j].SlideNode(slideDir, pos);

                    if(j % 2 == 0)
                        --i;
                }
                break;
        }
    }

    void ChangeNodeInfo(int x, int y, GameObject prefab, Node script) {
        nodePrefabs[x, y] = prefab;
        nodeScripts[x, y] = script;
        nodeScripts[x, y].RegistNodeID(x, y);
    }

    void SortLeftOutNode() {
        GameObject outNode = nodePrefabs[0, beforeTapNodeID.y];
        Node outNodeScript = nodeScripts[0, beforeTapNodeID.y];
        
        // ノード入れ替え処理
        for (int i = 1; i < row; ++i) {
            ChangeNodeInfo(i - 1, beforeTapNodeID.y, nodePrefabs[i, beforeTapNodeID.y], nodeScripts[i, beforeTapNodeID.y]);
        }
        ChangeNodeInfo(row - 1, beforeTapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[row - 2, beforeTapNodeID.y].transform.position;
        pos.x += nodeSize.x * 1.5f;     // 画面内に見切れないように念のため 1.5倍にする
        nodePrefabs[row - 1, beforeTapNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        --beforeTapNodeID.x;
        --afterTapNodeID.x;
    }

    void SortRightOutNode() {
        GameObject outNode = nodePrefabs[row - 1, beforeTapNodeID.y];
        Node outNodeScript = nodeScripts[row - 1, beforeTapNodeID.y];
        
        // ノード入れ替え処理
        for (int i = row - 1; i >= 1; --i) {
            ChangeNodeInfo(i, beforeTapNodeID.y, nodePrefabs[i - 1, beforeTapNodeID.y], nodeScripts[i - 1, beforeTapNodeID.y]);
        }
        ChangeNodeInfo(0, beforeTapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[1, beforeTapNodeID.y].transform.position;
        pos.x -= nodeSize.x * 1.5f;     // 画面内に見切れないように念のため 1.5倍にする
        nodePrefabs[0, beforeTapNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        ++beforeTapNodeID.x;
        ++afterTapNodeID.x;
    }

    void SortLeftUpOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.x, outNodeID.y];
        Node outNodeScript = nodeScripts[outNodeID.x, outNodeID.y];

        // ノード入れ替え処理(右下方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.RD).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RD);
            ChangeNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.x, limitNodeID.y], nodeScripts[limitNodeID.x, limitNodeID.y]);
            prevSearchID = limitNodeID;
        }
        ChangeNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.LU);
        Vector3 pos = nodePrefabs[prevSearchID.x, prevSearchID.y].transform.position;
        pos.x += moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y -= moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.x, limitNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        if(beforeTapNodeID.y % 2 == 0)
            --beforeTapNodeID.x;
        ++beforeTapNodeID.y;
        if(afterTapNodeID.y % 2 == 0)
            --afterTapNodeID.x;
        ++afterTapNodeID.y;
    }

    void SortRightDownOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.x, outNodeID.y];
        Node outNodeScript = nodeScripts[outNodeID.x, outNodeID.y];

        // ノード入れ替え処理(左上方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.LU).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LU);
            ChangeNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.x, limitNodeID.y], nodeScripts[limitNodeID.x, limitNodeID.y]);
            prevSearchID = limitNodeID;
        }
        ChangeNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.RD);
        Vector3 pos = nodePrefabs[prevSearchID.x, prevSearchID.y].transform.position;
        pos.x -= moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y += moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.x, limitNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        if(beforeTapNodeID.y % 2 != 0)
            ++beforeTapNodeID.x;
        --beforeTapNodeID.y;
        if(afterTapNodeID.y % 2 != 0)
            ++afterTapNodeID.x;
        --afterTapNodeID.y;
    }

    void SortRightUpOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.x, outNodeID.y];
        Node outNodeScript = nodeScripts[outNodeID.x, outNodeID.y];

        // ノード入れ替え処理(左下方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.LD).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LD);
            ChangeNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.x, limitNodeID.y], nodeScripts[limitNodeID.x, limitNodeID.y]);
            prevSearchID = limitNodeID;
        }
        ChangeNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.RU);
        Vector3 pos = nodePrefabs[prevSearchID.x, prevSearchID.y].transform.position;
        pos.x -= moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y -= moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.x, limitNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        if(beforeTapNodeID.y % 2 != 0)
            ++beforeTapNodeID.x;
        ++beforeTapNodeID.y;
        if(afterTapNodeID.y % 2 != 0)
            ++afterTapNodeID.x;
        ++afterTapNodeID.y;
    }

    void SortLeftDownOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.x, outNodeID.y];
        Node outNodeScript = nodeScripts[outNodeID.x, outNodeID.y];

        // ノード入れ替え処理(右上方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.RU).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RU);
            ChangeNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.x, limitNodeID.y], nodeScripts[limitNodeID.x, limitNodeID.y]);
            prevSearchID = limitNodeID;
        }
        ChangeNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.LD);
        Vector3 pos = nodePrefabs[prevSearchID.x, prevSearchID.y].transform.position;
        pos.x += moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y += moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.x, limitNodeID.y].transform.position = pos;
        
        // 選択中のノードIDを更新
        if(beforeTapNodeID.y % 2 == 0)
            --beforeTapNodeID.x;
        --beforeTapNodeID.y;
        if(afterTapNodeID.y % 2 == 0)
            --afterTapNodeID.x;
        --afterTapNodeID.y;
    }

    public void CheckOutScreen(int x, int y) {
        if(!(nodeScripts[x,y].GetLeftPos()   < CAMERA_AREA.right  &&
             nodeScripts[x,y].GetRightPos()  > CAMERA_AREA.left   &&
             nodeScripts[x,y].GetTopPos()    > CAMERA_AREA.bottom &&
             nodeScripts[x,y].GetBottomPos() < CAMERA_AREA.top))
            nodeScripts[x,y].IsOutScreen = true;
        else
            nodeScripts[x,y].IsOutScreen = false;
    }

    public void CheckOutScreen(Vec2Int id) {
        CheckOutScreen(id.x, id.y);
    }

    void AllCheckOutScreen() {
        for(int i = 0; i < row; ++i) {
            for(int j = 0; j < col; ++j) {
                CheckOutScreen(i, j);
            }
        }
    }
    
    // ノードの接続を確認するチェッカー
    public class NodeLinkTaskChecker
    {
        static int IDCnt = 0;
        public int ID = 0;
        public int Branch = 0;
        public bool NotFin = false;
        public int SumNode = 0;
        public ArrayList NodeList = new ArrayList();

        public NodeLinkTaskChecker()
        {
            this.ID = ++IDCnt;
        }
    }

    // 接続をチェックする関数
    public void CheckLink()
    {
        if (Debug.isDebugBuild && bNodeLinkDebugLog)
            Debug.Log("CheckLink");
        ResetCheckedFragAll();

        // 根っこ分繰り返し
        for (int i = 1; i < row - 1; i++)
        {
            // チェッカを初期化
            var Checker = new NodeLinkTaskChecker();

            // 根っこを叩いて処理スタート
            var firstNodeAct = Observable
                .Return(i)
                .Subscribe(x =>
                {
                    if (Debug.isDebugBuild && bNodeLinkDebugLog)
                        Debug.Log("firstNodeAct_Subscribe [" + Checker.ID + "]");
                    Checker.Branch++;
                    nodeScripts[x, 1].Action(Checker, _eLinkDir.NONE);
                }).AddTo(this);

            // キャッチャを起動
            var CheckedCallback = Observable
                .NextFrame()
                .Repeat()
                .First(_ => Checker.Branch == 0)    // 処理中の枝が0なら終了
                .Subscribe(_ =>
                {
                    if (Debug.isDebugBuild && bNodeLinkDebugLog)
                        Debug.Log("CheckedCallback_Subscribe [" + Checker.ID + "]" + Checker.SumNode.ToString() + "/" + (Checker.NotFin ? "" : "Fin"));
                    if (Checker.SumNode >= 3 &&
                    Checker.NotFin == false)
                    {
                        foreach (Node Nodes in Checker.NodeList)
                        {
                            Nodes.CompleteFlag = true;
                        };
                        // 消去処理
                        ReplaceNodeAll();
                        if (Debug.isDebugBuild && bNodeLinkDebugLog)
                            print("枝が完成しました！");
                    }
                }).AddTo(this);
        }
    }

    //閲覧済みフラグを戻す処理
    public void ResetCheckedFragAll()
    {
        foreach (var nodes in nodeScripts)
        {
            //繋がりがない枝は色をここでもどす
            nodes.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
            nodes.CheckFlag = false;
        }
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(int x, int y, _eLinkDir toDir)
    {
        //走査方向のノードのcolとrow

        Vec2Int nextNodeID;

        nextNodeID.x = x;
        nextNodeID.y = y;

        bool Odd = ((y % 2) == 0) ? false : true;

        //次のノード番号の計算
        switch(toDir)
        {
            case _eLinkDir.RU:
                if (Odd)
                    nextNodeID.x++;
                nextNodeID.y++;
                break;
            case _eLinkDir.R:
                nextNodeID.x++;
                break;
            case _eLinkDir.RD:
                if (Odd)
                    nextNodeID.x++;
                nextNodeID.y--;
                break;
            case _eLinkDir.LD:
                if(!Odd)
                    nextNodeID.x--;
                nextNodeID.y--;
                break;
            case _eLinkDir.L:
                nextNodeID.x--;
                break;
            case _eLinkDir.LU:
                if (!Odd)
                    nextNodeID.x--;
                nextNodeID.y++;

                break;
        }

        if (nextNodeID.x < 0 || nextNodeID.x > row - 1 ||nextNodeID.y < 0 || nextNodeID.y > col - 1)
        {
            nextNodeID.x = -1;
            nextNodeID.y = -1;
        }

        return nextNodeID;
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(Vec2Int nodeID,_eLinkDir toDir)
    {
        return GetDirNode(nodeID.x, nodeID.y, toDir);
    }
    
    //指定した位置のノードのスクリプトをもらう
    public Node GetNodeScript(Vec2Int nodeID)
    {
        return nodeScripts[nodeID.x, nodeID.y];
    }

/*    public void CheckLinkAround()
    {

    }

    //木の繋がりが途絶えていた時、その木の一部であるノードすべてを未開通にする
    public void ResetTreeBit(BitArray treeBit)
    {
        foreach(var node in nodeScripts)
        {
            for (int i = 0; i < (int)_eTreePath.MAX; i++)
            {
                if (treeBit[i])
                {
                    node.bitTreePath[i] = false;

                }
            }
        }
    }
    */
    //ノードの配置 割合は指定できるが完全ランダムなので再考の余地あり
    void ReplaceNode(Node node)
    {
        node.CompleteFlag = false;
        node.CheckFlag = false;
        node.ChainFlag = false;

        float rand;
        rand = UnityEngine.Random.Range(0.0f, RatioSum);

        //暫定ランダム処理
        if (0.0f <= rand && rand <= fieldLevel.Ratio_Cap)
        {
            node.SetNodeType(_eNodeType.CAP);
        }
        else if (fieldLevel.Ratio_Cap < rand && rand <= fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2)
        {
            int n2;
            //2又のどれかはランダムでいいか
            n2 = UnityEngine.Random.Range(0, 3);                 //マジックナンバーどうにかしたい
            node.SetNodeType((_eNodeType)((int)(_eNodeType.HUB2_A + n2)));
        }
        else if (fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 < rand && rand <= fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3)
        {
            int n3;
            //3又のどれかはランダムでいいか
            n3 = UnityEngine.Random.Range(0, 2);                 //マジックナンバーどうにかしたい
            node.SetNodeType((_eNodeType)((int)(_eNodeType.HUB3_A + n3)));
        }
        else
        {
            //おかしな値が入力されていた際はキャップになるよう
            node.SetNodeType(_eNodeType.CAP);
        }
    }

    //完成した枝に使用しているノードを再配置する
    void ReplaceNodeAll()
    {
        int nNode = 0;
        int nCap = 0;
        int nPath2 = 0;
        int nPath3 = 0;
        foreach(var node in nodeScripts)
        {
            if (node.CompleteFlag)
            {
                nNode++;
                switch (node.GetLinkNum())
                {
                    case 1:
                        nCap++;
                        break;
                    case 2:
                        nPath2++;
                        break;
                    case 3:
                        nPath3++;
                        break;
                }

                
                ReplaceNode(node);
            }

        }
        scoreScript.PlusScore(nNode, nCap, nPath2, nPath3);
        timeScript.PlusTime(nNode, nCap, nPath2, nPath3);

    }

    public void SetFieldLevel(int level)
    {
        fieldLevel = levelTables.GetFieldLevel(level);
    }
}
