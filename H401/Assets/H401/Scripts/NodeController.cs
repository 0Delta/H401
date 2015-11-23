using UnityEngine;
using System.Collections;
using UniRx;

using DG.Tweening;

/*  リストIDに関して
　　col のIDが奇数の行は +1 とする

     ◇◇・・・   (col, row)
    ・・・・◇◇  (col - 1, row + 1)
     ・・・・・
    ・・・・・・
     ◇◇◇・・
    ◇◇◇・・・
     ◇◇・・・
    (0,0)
*/

public class NodeController : MonoBehaviour {

//    private const float ADJUST_PIXELS_PER_UNIT = 0.01f;     // Pixels Per Unit の調整値
//    private readonly Square GAME_AREA = new Square(0.0f, 0.0f, 5.0f * 2.0f * 750.0f / 1334.0f, 5.0f * 2.0f);    // ゲームの画面領域(パズル領域)
    private const float FRAME_POSZ_MARGIN = -1.0f;       // フレームとノードとの距離(Z座標)

    [SerializeField] private int row = 0;       // 横配置数 リストIDが奇数の行は＋１とする
    [SerializeField] private int col = 0;       // 縦配置数
    [SerializeField] private GameObject nodePrefab = null;       // ノードのプレハブ
    [SerializeField] private GameObject frameNodePrefab = null;       // フレームノードのプレハブ
    [SerializeField] private float widthMargin  = 0.0f;  // ノード位置の左右間隔の調整値
    [SerializeField] private float heightMargin = 0.0f;  // ノード位置の上下間隔の調整値
    [SerializeField] private float headerHeight = 0.0f;  // ヘッダーの高さ

    private GameObject[][]  nodePrefabs;        // ノードのプレハブリスト
    private Node[][]        nodeScripts;        // ノードのnodeスクリプトリスト
    private GameObject      frameController;    // フレームコントローラープレハブ

    private Square  gameArea = Square.zero;     // ゲームの画面領域(パズル領域)
    private Vector2 nodeSize = Vector2.zero;    // 描画するノードのサイズ

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
        nodePrefabs = new GameObject[col][];
        nodeScripts = new Node[col][];
        for(int i = 0; i < col; ++i) {
            nodePrefabs[i] = i % 2 == 0 ? new GameObject[row] : new GameObject[row + 1];
            nodeScripts[i] = i % 2 == 0 ? new Node[row] : new Node[row + 1];
        }
    }
    
	// Use this for initialization
	void Start () {
        scoreScript = GameObject.Find("ScoreNum").GetComponent<Score>();
        timeScript = GameObject.Find("LimitTime").GetComponent<LimitTime>();
        fieldLevel = levelTables.GetFieldLevel(0);

        // ----- ゲームの画面領域を設定(コライダーから取得)
        BoxCollider2D gameAreaInfo = transform.parent.GetComponent<BoxCollider2D>();    // ゲームの画面領域を担うコライダー情報を取得
        gameArea.x      = gameAreaInfo.offset.x;
        gameArea.y      = gameAreaInfo.offset.y;
        gameArea.width  = gameAreaInfo.size.x;
        gameArea.height = gameAreaInfo.size.y;
        
        // ----- ノード準備
        // 描画するノードの大きさを取得
        MeshFilter nodeMeshInfo = nodePrefab.GetComponent<MeshFilter>();    // ノードのメッシュ情報を取得
        Vector3 pos = transform.position;
        nodeSize.x = nodeMeshInfo.sharedMesh.bounds.size.x * nodePrefab.transform.localScale.x;
        nodeSize.y = nodeMeshInfo.sharedMesh.bounds.size.y * nodePrefab.transform.localScale.y;
        nodeSize.x -= widthMargin;
        nodeSize.y -= heightMargin;

        RatioSum = fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3;  //全体割合を記憶


        // フレームを生成
        frameController = new GameObject();
        frameController.transform.parent = transform.parent;
        frameController.name = "FrameController";

        // ノードを生成
        for(int i = 0; i < col; ++i) {
            GameObject  frameObject     = null;

            // ノードの配置位置を調整(Y座標)
            pos.y = transform.position.y - headerHeight + nodeSize.y * -(col * 0.5f - (i + 0.5f));
            for (int j = 0; j < AdjustRow(i); ++j) {
                // ノードの配置位置を調整(X座標)
                //pos.x = i % 2 == 0 ? transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - j) : transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (j + 0.5f));
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (j + 0.5f));
                pos.z = transform.position.z;

                // 生成
        	    nodePrefabs[i][j] = (GameObject)Instantiate(nodePrefab, pos, transform.rotation);
                nodeScripts[i][j] = nodePrefabs[i][j].GetComponent<Node>();
                nodePrefabs[i][j].transform.parent = transform;

                // ノードの位置(リストID)を登録
                nodeScripts[i][j].RegistNodeID(j, i);

                //ランダムでノードの種類と回転を設定
                ReplaceNode(nodeScripts[i][j]);
                
                // フレーム生成(上端)
                if(i >= col - 1) {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(frameNodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                }
                // フレーム生成(下端)
                if(i <= 0) {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(frameNodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                }
                
//                if (j == 0 || i == 0 || j == row - 1 || i == col - 1)
//                    nodeScripts[j, i].SpRenderer.color = new Color(0.1f, 0.1f, 0.1f);
            }
            
            // フレーム生成(左端)
            pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (0 + 0.5f));
            pos.z = transform.position.z + FRAME_POSZ_MARGIN;
            frameObject = (GameObject)Instantiate(frameNodePrefab, pos, transform.rotation);
            frameObject.transform.parent = frameController.transform;
            // フレーム生成(右端)
            pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (AdjustRow(i) - 1 + 0.5f));
            frameObject = (GameObject)Instantiate(frameNodePrefab, pos, transform.rotation);
            frameObject.transform.parent = frameController.transform;
        }
        
        // ノードに情報を登録
        nodeScripts[0][0].SetNodeController(this);

        // 画面外ノードを登録
        AllCheckOutScreen();

        // スライドベクトルの垂線を算出
        Vector3 leftUp = nodePrefabs[0][1].transform.position - nodePrefabs[0][0].transform.position;
        Vector3 leftDown = nodePrefabs[1][1].transform.position - nodePrefabs[0][0].transform.position;
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
        for(int i = 0; i < col; ++i) {
            for(int j = 0; j < AdjustRow(i); ++j) {
                if(nodeScripts[i][j].IsOutScreen)
                    nodeScripts[i][j].MeshRenderer.material.color = new Color(0.1f, 0.1f, 1.0f);
                else
                    nodeScripts[i][j].MeshRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
            }
        }
	}
    
    // ノード移動処理
    void SlantMove() {
        // スライド対象となるノードの準備
        Vector2 pos        = tapPos;                     // 移動位置
        Vector2 slideDist  = tapPos - startTapPos;       // スライド量
        Vector2 moveDist   = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, slideDist);      // 斜め移動量
        Vector2 deltaSlideDist = tapPos - prevTapPos;    // 前回フレームからのスライド量
        float   checkDir   = 0.0f;                       // スライド方向チェック用
        Vec2Int nextNodeID = Vec2Int.zero;

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // スライド方向を再計算
                if(tapPos.x - prevTapPos.x < 0.0f) {
                    slideDir = _eSlideDir.LEFT;
                } else if(tapPos.x - prevTapPos.x > 0.0f) {
                    slideDir = _eSlideDir.RIGHT;
                } else {
                    break;
                }

                // タップしているノードを移動
                pos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x].transform.position.y;
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, pos);

                // タップしているノードより左側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.L);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos = tapPos - moveNodeDistAbs * i;
                    pos.y = nodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.y;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
                }
                // タップしているノードより右側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.R);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos = tapPos + moveNodeDistAbs * i;
                    pos.y = nodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.y;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.R);
                }
                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftUpPerNorm);
                if(checkDir < 0.0f) {
                    slideDir = _eSlideDir.LEFTUP;
                    if(GetDirNode(beforeTapNodeID, _eLinkDir.LU).x < 0) {
                        break;
                    }
                } else if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.RIGHTDOWN;
                    if(GetDirNode(beforeTapNodeID, _eLinkDir.RD).x < 0) {
                        break;
                    }
                } else {
                    break;
                }
                
                // タップしているノードを移動
                pos = moveNodeInitPos + moveDist;
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, pos);

                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = moveNodeInitPos.x + moveDist.x - moveNodeDistAbs.x * i;
                    pos.y = moveNodeInitPos.y + moveDist.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = moveNodeInitPos.x + moveDist.x + moveNodeDistAbs.x * i;
                    pos.y = moveNodeInitPos.y + moveDist.y - moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
                }
                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftDownPerNorm);
                if(checkDir < 0.0f) {
                    slideDir = _eSlideDir.LEFTDOWN;
                    if(GetDirNode(beforeTapNodeID, _eLinkDir.LD).x < 0) {
                        break;
                    }
                } else if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.RIGHTUP;
                    if(GetDirNode(beforeTapNodeID, _eLinkDir.RU).x < 0) {
                        break;
                    }
                } else {
                    break;
                }

                // タップしているノードを移動
                pos = moveNodeInitPos + moveDist;
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, pos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = moveNodeInitPos.x + moveDist.x + moveNodeDistAbs.x * i;
                    pos.y = moveNodeInitPos.y + moveDist.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = moveNodeInitPos.x + moveDist.x - moveNodeDistAbs.x * i;
                    pos.y = moveNodeInitPos.y + moveDist.y - moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
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
        moveNodeDist = new Vector2(nodePrefabs[afterTapNodeID.y][afterTapNodeID.x].transform.position.x,   // スライド方向ベクトル兼移動量を算出
                                    nodePrefabs[afterTapNodeID.y][afterTapNodeID.x].transform.position.y)
                    - new Vector2(nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x].transform.position.x,
                                    nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x].transform.position.y);
        moveNodeInitPos = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x].transform.position;      // ノードの移動開始位置を保存
        
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

    // ゲームの画面外にはみ出したノードを逆側に移動する
    void LoopBackNode()
    {
        Vec2Int limitNodeID         = Vec2Int.zero;     // スライド方向の端のノードID
        Vec2Int reverseLimitNodeID  = Vec2Int.zero;     // スライド方向の逆端のノードID

        switch (slideDir)
        {
            // ----- 左にスライド
            case _eSlideDir.LEFT:
                // 回り込み調整用ノードを右端に移動
                if(nodeScripts[beforeTapNodeID.y][0].IsOutScreen) {
                    SortLeftOutNode();
                }

                // 左端ノードが画面外に出ていないかチェック
                if(nodeScripts[beforeTapNodeID.y][0].GetLeftPos() < gameArea.left) {
                    nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].CopyParameter(nodeScripts[beforeTapNodeID.y][0]);
                    nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].IsOutScreen = false;
                }

                // 左端ノードが画面外に出たかチェック
                if(nodeScripts[beforeTapNodeID.y][0].GetRightPos() <= gameArea.left) {
                    nodeScripts[beforeTapNodeID.y][0].IsOutScreen = true;
                    SortLeftOutNode();
                }

                break;
                
            // ----- 右にスライド
            case _eSlideDir.RIGHT:
                // 回り込み調整用ノードを左端に移動
                if (nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].IsOutScreen) {
                    SortRightOutNode();
                }

                // 右端ノードが画面外に出ていないかチェック
                if (nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].GetRightPos() > gameArea.right) {
                    nodeScripts[beforeTapNodeID.y][0].CopyParameter(nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1]);
                    nodeScripts[beforeTapNodeID.y][0].IsOutScreen = false;
                }

                // 右端ノードが画面外に出たかチェック
                if (nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].GetLeftPos() >= gameArea.right) {
                    nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].IsOutScreen = true;
                    SortRightOutNode();
                }

                break;

            // ----- 左上にスライド
            case _eSlideDir.LEFTUP:
                limitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.LU);           // 左上端のノードIDを検索
                // 左上端のノードが選択中ノードと同一なら未処理
                if(limitNodeID == beforeTapNodeID)
                    break;
                reverseLimitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.RD);    // 右下端のノードIDを検索
                
                // 回り込み調整用ノードを右下端に移動
                if(nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen) {
                    SortLeftUpOutNode(limitNodeID);
                }

                // 左上端ノードが画面外に出ていないかチェック
                if( nodeScripts[limitNodeID.y][limitNodeID.x].GetLeftPos() < gameArea.left ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetTopPos()  > gameArea.top) {
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].CopyParameter(nodeScripts[limitNodeID.y][limitNodeID.x]);
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen = false;
                }

                // 左上端ノードが画面外に出たかチェック
                if( nodeScripts[limitNodeID.y][limitNodeID.x].GetRightPos()  <= gameArea.left ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetBottomPos() >= gameArea.top) {
                    nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen != true &&
                        nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen != true)
                        SortLeftUpOutNode(limitNodeID);
                }

                break;

            // ----- 右下にスライド
            case _eSlideDir.RIGHTDOWN:
                limitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.RD);           // 右下端のノードIDを検索
                // 右下端のノードが選択中ノードと同一なら未処理
                if(limitNodeID == beforeTapNodeID)
                    break;
                reverseLimitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.LU);    // 左上端のノードIDを検索
                
                // 回り込み調整用ノードを左上端に移動
                if (nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen) {
                    SortRightDownOutNode(limitNodeID);
                }

                // 右下端ノードが画面外に出ていないかチェック
                if (nodeScripts[limitNodeID.y][limitNodeID.x].GetRightPos()  > gameArea.right ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetBottomPos() < gameArea.bottom) {
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].CopyParameter(nodeScripts[limitNodeID.y][limitNodeID.x]);
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen = false;
                }

                // 右下端ノードが画面外に出たかチェック
                if (nodeScripts[limitNodeID.y][limitNodeID.x].GetLeftPos() >= gameArea.right ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetTopPos()  <= gameArea.bottom) {
                    nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen != true &&
                        nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen != true)
                        SortRightDownOutNode(limitNodeID);
                }

                break;

            // ----- 右上にスライド
            case _eSlideDir.RIGHTUP:
                limitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.RU);           // 右上端のノードIDを検索
                // 右上端のノードが選択中ノードと同一なら未処理
                if(limitNodeID == beforeTapNodeID)
                    break;
                reverseLimitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.LD);    // 左下端のノードIDを検索
                
                // 回り込み調整用ノードを左下端に移動
                if(nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen) {
                    SortRightUpOutNode(limitNodeID);
                }

                // 右上端ノードが画面外に出ていないかチェック
                if( nodeScripts[limitNodeID.y][limitNodeID.x].GetRightPos() > gameArea.right ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetTopPos()   > gameArea.top) {
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].CopyParameter(nodeScripts[limitNodeID.y][limitNodeID.x]);
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen = false;
                }

                // 右上端ノードが画面外に出たかチェック
                if( nodeScripts[limitNodeID.y][limitNodeID.x].GetLeftPos()   >= gameArea.right ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetBottomPos() >= gameArea.top) {
                    nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen != true &&
                        nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen != true)
                        SortRightUpOutNode(limitNodeID);
                }

                break;

            // ----- 左下にスライド
            case _eSlideDir.LEFTDOWN:
                limitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.LD);           // 左下端のノードIDを検索
                // 左下端のノードが選択中ノードと同一なら未処理
                if(limitNodeID == beforeTapNodeID)
                    break;
                reverseLimitNodeID = SearchLimitNode(beforeTapNodeID, _eLinkDir.RU);    // 右上端のノードIDを検索
                
                // 回り込み調整用ノードを右上端に移動
                if (nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen) {
                    SortLeftDownOutNode(limitNodeID);
                }

                // 左下端ノードが画面外に出ていないかチェック
                if (nodeScripts[limitNodeID.y][limitNodeID.x].GetLeftPos()   < gameArea.left ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetBottomPos() < gameArea.bottom) {
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].CopyParameter(nodeScripts[limitNodeID.y][limitNodeID.x]);
                    nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen = false;
                }

                // 左下端ノードが画面外に出たかチェック
                if (nodeScripts[limitNodeID.y][limitNodeID.x].GetRightPos() <= gameArea.left ||
                    nodeScripts[limitNodeID.y][limitNodeID.x].GetTopPos()   <= gameArea.bottom) {
                    nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen = true;
                    if( nodeScripts[reverseLimitNodeID.y][reverseLimitNodeID.x].IsOutScreen != true &&
                        nodeScripts[limitNodeID.y][limitNodeID.x].IsOutScreen != true)
                        SortLeftDownOutNode(limitNodeID);
                }

                break;
        }
    }

    // 移動を終了するノードの位置を調整する
    void AdjustNodeStop() {
        Vector2 pos         = Vector2.zero;
        Vector2 standardPos = Vector2.zero;
        Vector2 slideDist   = tapPos - startTapPos;       // スライド量
        Vec2Int nextNodeID  = Vec2Int.zero;

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y + 2][beforeTapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y - 2][beforeTapNodeID.x].transform.position.x;
                }
                standardPos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x].transform.position.y;
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左側のノードを移動
                for(int i = beforeTapNodeID.x - 1, j = 1; i >= 0; --i, ++j) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[beforeTapNodeID.y][i].SlideNode(slideDir, pos);
                }
                // タップしているノードより右側のノードを移動
                for(int i = beforeTapNodeID.x + 1, j = 1; i < AdjustRow(beforeTapNodeID.y); ++i, ++j) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[beforeTapNodeID.y][i].SlideNode(slideDir, pos);
                }
                break;
                
            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y + 2][beforeTapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y - 2][beforeTapNodeID.x].transform.position.x;
                }
                if(beforeTapNodeID.x + 1 < AdjustRow(beforeTapNodeID.y)) {
                    standardPos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x + 1].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x - 1].transform.position.y;
                }
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, standardPos);
                
                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
                }

                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // タップしているノードを移動
                if(beforeTapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y + 2][beforeTapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[beforeTapNodeID.y - 2][beforeTapNodeID.x].transform.position.x;
                }
                if(beforeTapNodeID.x + 1 < AdjustRow(beforeTapNodeID.y)) {
                    standardPos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x + 1].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[beforeTapNodeID.y][beforeTapNodeID.x - 1].transform.position.y;
                }
                nodeScripts[beforeTapNodeID.y][beforeTapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
                }

                break;
        }
    }

    // 任意のノード情報をコピーする
    void CopyNodeInfo(int x, int y, GameObject prefab, Node script) {
        nodePrefabs[y][x] = prefab;
        nodeScripts[y][x] = script;
        nodeScripts[y][x].RegistNodeID(x, y);
    }

    // 左にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortLeftOutNode() {
        GameObject outNode = nodePrefabs[beforeTapNodeID.y][0];
        Node outNodeScript = nodeScripts[beforeTapNodeID.y][0];
        
        // ノード入れ替え処理
        for (int i = 1; i < AdjustRow(beforeTapNodeID.y); ++i) {
            CopyNodeInfo(i - 1, beforeTapNodeID.y, nodePrefabs[beforeTapNodeID.y][i], nodeScripts[beforeTapNodeID.y][i]);
        }
        CopyNodeInfo(AdjustRow(beforeTapNodeID.y) - 1, beforeTapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 2].transform.position;
        pos.x += nodeSize.x * 1.5f;     // 画面内に見切れないように念のため 1.5倍にする
        nodePrefabs[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.L);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.L);
    }

    // 右にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortRightOutNode() {
        GameObject outNode = nodePrefabs[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1];
        Node outNodeScript = nodeScripts[beforeTapNodeID.y][AdjustRow(beforeTapNodeID.y) - 1];
        
        // ノード入れ替え処理
        for (int i = AdjustRow(beforeTapNodeID.y) - 1; i >= 1; --i) {
            CopyNodeInfo(i, beforeTapNodeID.y, nodePrefabs[beforeTapNodeID.y][i - 1], nodeScripts[beforeTapNodeID.y][i - 1]);
        }
        CopyNodeInfo(0, beforeTapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[beforeTapNodeID.y][1].transform.position;
        pos.x -= nodeSize.x * 1.5f;     // 画面内に見切れないように念のため 1.5倍にする
        nodePrefabs[beforeTapNodeID.y][0].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.R);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.R);
    }

    // 左上にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortLeftUpOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.y][outNodeID.x];
        Node outNodeScript = nodeScripts[outNodeID.y][outNodeID.x];

        // ノード入れ替え処理(右下方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.RD).x > -1) {
            prevSearchID = limitNodeID;
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RD);
            CopyNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.y][limitNodeID.x], nodeScripts[limitNodeID.y][limitNodeID.x]);
        }
        CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.LU);
        Vector3 pos = nodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
        pos.x += moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y -= moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LU);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.LU);
    }

    // 右下にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortRightDownOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.y][outNodeID.x];
        Node outNodeScript = nodeScripts[outNodeID.y][outNodeID.x];

        // ノード入れ替え処理(左上方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.LU).x > -1) {
            prevSearchID = limitNodeID;
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LU);
            CopyNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.y][limitNodeID.x], nodeScripts[limitNodeID.y][limitNodeID.x]);
        }
        CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.RD);
        Vector3 pos = nodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
        pos.x -= moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y += moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RD);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.RD);
    }

    // 右上にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortRightUpOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.y][outNodeID.x];
        Node outNodeScript = nodeScripts[outNodeID.y][outNodeID.x];

        // ノード入れ替え処理(左下方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.LD).x > -1) {
            prevSearchID = limitNodeID;
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.LD);
            CopyNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.y][limitNodeID.x], nodeScripts[limitNodeID.y][limitNodeID.x]);
        }
        CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.RU);
        Vector3 pos = nodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
        pos.x -= moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y -= moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.RU);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.RU);
    }

    // 左下にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortLeftDownOutNode(Vec2Int outNodeID) {
        GameObject outNode = nodePrefabs[outNodeID.y][outNodeID.x];
        Node outNodeScript = nodeScripts[outNodeID.y][outNodeID.x];

        // ノード入れ替え処理(右上方向に探索)
        Vec2Int limitNodeID  = outNodeID;
        Vec2Int prevSearchID = outNodeID;
        while(GetDirNode(limitNodeID, _eLinkDir.RU).x > -1) {
            prevSearchID = limitNodeID;
            limitNodeID = GetDirNode(limitNodeID, _eLinkDir.RU);
            CopyNodeInfo(prevSearchID.x, prevSearchID.y, nodePrefabs[limitNodeID.y][limitNodeID.x], nodeScripts[limitNodeID.y][limitNodeID.x]);
        }
        CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

        // 位置を調整
        prevSearchID = GetDirNode(limitNodeID, _eLinkDir.LD);
        Vector3 pos = nodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
        pos.x += moveNodeDistAbs.x * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        pos.y += moveNodeDistAbs.y * 2.0f;     // 画面内に見切れないように念のため 2.0倍にする
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        beforeTapNodeID = GetDirNode(beforeTapNodeID, _eLinkDir.LD);
        afterTapNodeID  = GetDirNode(afterTapNodeID, _eLinkDir.LD);
    }

    // ゲーム画面外にはみ出ているかチェックする
    public void CheckOutScreen(Vec2Int id) {
        if(!(nodeScripts[id.y][id.x].GetLeftPos()   < gameArea.right  &&
             nodeScripts[id.y][id.x].GetRightPos()  > gameArea.left   &&
             nodeScripts[id.y][id.x].GetTopPos()    > gameArea.bottom &&
             nodeScripts[id.y][id.x].GetBottomPos() < gameArea.top))
            nodeScripts[id.y][id.x].IsOutScreen = true;
        else
            nodeScripts[id.y][id.x].IsOutScreen = false;
    }

    // ゲーム画面外にはみ出ているかチェックする
    public void CheckOutScreen(int x, int y) {
        CheckOutScreen(new Vec2Int(x, y));
    }

    // ゲーム画面外にはみ出ているか、全てのノードをチェックする
    void AllCheckOutScreen() {
        for(int i = 0; i < col; ++i) {
            for(int j = 0; j < AdjustRow(i); ++j) {
                CheckOutScreen(j, i);
            }
        }
    }

    // 検索したい col に合わせた row を返す
    int AdjustRow(int col) {
        return col % 2 == 0 ? row : row + 1;
    }
    
    Vec2Int SearchLimitNode(Vec2Int id, _eLinkDir dir) {
        Vec2Int limitNodeID = id;
        while(GetDirNode(limitNodeID, dir).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, dir);
        }

        return limitNodeID;
    }

    Vec2Int SearchLimitNode(int x, int y, _eLinkDir dir) {
        return SearchLimitNode(new Vec2Int(x, y), dir);
    }

    public void CheckLink()
    {
        bool bComplete = false; //完成した枝があるか？
        int nodeNum = 0;
        //すべてのノードの根本を見る
        for (int i = 1; i < AdjustRow(1) - 1; i++)
        {
            if((nodeNum = nodeScripts[1][i].CheckBit(_eLinkDir.NONE,0)) >= 3)   //親ノードの方向は↓向きのどちらかにしておく
            {
                bComplete = true;                
            }
            //１走査ごとに閲覧済みフラグを戻す
            ResetCheckedFragAll();



        }
        //枝ができていた場合
        if(bComplete)
        {
            //ここに完成しました的なエフェクトを入れる？

            //枝を構成するノードを再配置
            ReplaceNodeAll();
            print("枝が完成しました！");
        }
        for (int i = 1; i < col - 1; i++  )
        {
            for(int j = 1 ; j < AdjustRow(i) - 1; j++)
            {
                if(!nodeScripts[i][j].ChainFlag)
                    nodeScripts[i][j].MeshRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
                nodeScripts[i][j].ChainFlag = false;
            }
        }
    }

    //閲覧済みフラグを戻す処理
    public void ResetCheckedFragAll()
    {
        for(int i = 0; i < col; ++i) {
            foreach (var nodes in nodeScripts[i]) {
                //繋がりがない枝は色をここでもどす
                //nodes.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
                nodes.CheckFlag = false;
            }
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
                if (!Odd)
                    nextNodeID.x++;
                nextNodeID.y++;
                break;
            case _eLinkDir.R:
                nextNodeID.x++;
                break;
            case _eLinkDir.RD:
                if (!Odd)
                    nextNodeID.x++;
                nextNodeID.y--;
                break;
            case _eLinkDir.LD:
                if(Odd)
                    nextNodeID.x--;
                nextNodeID.y--;
                break;
            case _eLinkDir.L:
                nextNodeID.x--;
                break;
            case _eLinkDir.LU:
                if (Odd)
                    nextNodeID.x--;
                nextNodeID.y++;

                break;
        }

        if (nextNodeID.x < 0 || nextNodeID.x > AdjustRow(nextNodeID.y) - 1 ||nextNodeID.y < 0 || nextNodeID.y > col - 1)
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
        return nodeScripts[nodeID.y][nodeID.x];
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
        for(int i = 0; i < col; ++i) {
            foreach(var node in nodeScripts[i])
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
        }
        scoreScript.PlusScore(nNode, nCap, nPath2, nPath3);
        timeScript.PlusTime(nNode, nCap, nPath2, nPath3);

    }

    public void SetFieldLevel(int level)
    {
        fieldLevel = levelTables.GetFieldLevel(level);
    }
}
