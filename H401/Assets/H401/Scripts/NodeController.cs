using UnityEngine;
using System.Collections;
using UniRx;

using DG.Tweening;
using System.Collections.Generic;
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
    [SerializeField] private GameObject treePrefab = null; 

    private GameObject[][]  nodePrefabs;        // ノードのプレハブリスト
    private Node[][]        nodeScripts;        // ノードのnodeスクリプトリスト
    private Vector3[][]     nodePlacePosList;   // ノードの配置位置リスト
    private GameObject      frameController;    // フレームコントローラープレハブ

    private Square  gameArea = Square.zero;     // ゲームの画面領域(パズル領域)
    private Vector2 nodeSize = Vector2.zero;    // 描画するノードのサイズ

    private bool        isTap           = false;                // タップ成功フラグ
    private bool        isSlide         = false;                // ノードスライドフラグ
    private bool        isNodeAction    = false;                // ノードがアクション中かフラグ
    private Vec2Int     tapNodeID       = Vec2Int.zero;         // タップしているノードのID
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

    [SerializeField] private GameObject levelTableObject = null;

    private LevelTables levelTableScript = null;

    //ノードの配置割合を記憶しておく

    public int Row {
        get { return this.row; }
    }
    
    public int Col {
        get { return this.col; }
    }

    public bool IsSlide {
        set { this.isSlide = value; }
        get { return this.isSlide; }
    }

    public bool IsNodeAction {
        set { this.isNodeAction = value; }
        get { return this.isNodeAction; }
    }

    public _eSlideDir SlideDir {
        get { return slideDir; }
    }

    public Vector2 NodeSize {
        get { return nodeSize; }
    }
    
    private Score scoreScript;          //スコアのスクリプト
    private LimitTime timeScript;            //制限時間のスクリプト
    private FeverGauge feverScript;

    [SerializeField]private float repRotateTime = 0;//ノード再配置時の時間

    public delegate void Replace();             //回転再配置用のデリゲート

    [SerializeField]private Material[] nodeMaterials = null;
    public Material GetMaterial(int nodeType){return nodeMaterials[nodeType];}

    private int _currentLevel;
    public int currentLevel
    {
        get { return _currentLevel; }
        set { _currentLevel = value;
        fieldLevel = levelTableScript.GetFieldLevel(_currentLevel);
        StartCoroutine(ReplaceRotate(ReplaceNodeAll));
        }
    }

    void Awake() {
        nodePrefabs      = new GameObject[col][];
        nodeScripts      = new Node[col][];
        nodePlacePosList = new Vector3[col][];
        for(int i = 0; i < col; ++i) {
            nodePrefabs[i]      = i % 2 == 0 ? new GameObject[row] : new GameObject[row + 1];
            nodeScripts[i]      = i % 2 == 0 ? new Node[row] : new Node[row + 1];
            nodePlacePosList[i] = i % 2 == 0 ? new Vector3[row] : new Vector3[row + 1];
        }
    }
    
	// Use this for initialization
	void Start () {
        scoreScript = GameObject.Find("ScoreNum").GetComponent<Score>();
        timeScript = GameObject.Find("LimitTime").GetComponent<LimitTime>();
        feverScript = GameObject.Find("FeverGauge").GetComponent<FeverGauge>();

        levelTableScript = levelTableObject.GetComponent<LevelTables>();
        fieldLevel = levelTableScript.GetFieldLevel(0);

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

        Node.SetNodeController(this); //ノードにコントローラーを設定

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
                nodePrefabs[i][j].transform.SetParent(transform);
                nodePlacePosList[i][j] = nodePrefabs[i][j].transform.position;

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
                // タップに成功していなければ未処理
                if(!isTap)
                    return;

                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // スライド処理
                if(isSlide) {
                    prevTapPos = tapPos;
                    tapPos = new Vector2(worldTapPos.x, worldTapPos.y);
                    SlantMove();
                    AllCheckOutScreen();
                    LoopBackNode();

                    return;
                }
                
                // スライド判定
                if(tapNodeID.x > -1) {
                    Vec2Int nextNodeID = GetDirNode(tapNodeID, CheckSlideDir(startTapPos, worldTapPos));
                    if(nextNodeID.x > -1) {
                        if (Vector3.Distance(nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position, worldTapPos) >
                            Vector3.Distance(nodePrefabs[nextNodeID.y][nextNodeID.x].transform.position, worldTapPos)) {
                            isSlide = true;
                            StartSlideNodes(nextNodeID, CheckSlideDir(startTapPos, worldTapPos));
                        }
                    }
                }
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => {
                // ノードがアクション中なら未処理
                if(isNodeAction)
                    return;
                
                // タップ成功
                isTap = true;

                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startTapPos = new Vector2(worldTapPos.x, worldTapPos.y);
                
                // タップしているノードのIDを検索
                tapNodeID = SearchTapNode(worldTapPos);

                print(tapNodeID);
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => {
                // タップに成功していなければ未処理
                if(!isTap)
                    return;
                
                // タップ終了
                isTap = false;

                if(isSlide) {
                    AdjustNodeStop();

                    isSlide   = false;
                    slideDir = _eSlideDir.NONE;

                    tapNodeID = Vec2Int.zero;
                } else {
                    if(tapNodeID.x > -1)
                        nodeScripts[tapNodeID.y][tapNodeID.x].RotationNode();
                }
            })
            .AddTo(this.gameObject);

        CheckLink();
	}
	
	// Update is called once per frame
	void Update () {
        // @デバッグ用
        //for(int i = 0; i < col; ++i) {
        //    for(int j = 0; j < AdjustRow(i); ++j) {
        //        if(nodeScripts[i][j].IsOutScreen)
        //            nodeScripts[i][j].MeshRenderer.material.color = new Color(0.1f, 0.1f, 1.0f);
        //        else
        //            nodeScripts[i][j].MeshRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
        //    }
        //}
	}
    
    // ノード移動処理
    void SlantMove() {
        // スライド対象となるノードの準備
        Vector2     pos         = tapPos;                   // 移動位置
        Vector2     standardPos = tapPos;                   // タップノードの移動後座標
        Vector2     slideDist   = tapPos - startTapPos;     // スライド量
        Vector2     moveDist    = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, slideDist);      // 斜め移動量
        Vector2     deltaSlideDist = tapPos - prevTapPos;   // 前回フレームからのスライド量
        float       checkDir    = 0.0f;                     // スライド方向チェック用
        Vec2Int     nextNodeID  = Vec2Int.zero;             // 検索用ノードIDテンポラリ
        
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
                pos = AdjustGameScreen(pos);    // タップ位置調整
                pos.y = nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y;
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, pos);

                // タップしているノードより左側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = tapPos.x - moveNodeDistAbs.x * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
                }
                // タップしているノードより右側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = tapPos.x + moveNodeDistAbs.x * i;
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
                } else if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.RIGHTDOWN;
                }

                // タップしているノードを移動
                Vec2Int lu = SearchLimitNode(tapNodeID, _eLinkDir.LU);
                Vec2Int rd = SearchLimitNode(tapNodeID, _eLinkDir.RD);
                standardPos = moveNodeInitPos + moveDist;
                if (standardPos.x < nodePlacePosList[lu.y][lu.x].x)
                    standardPos.x = nodePlacePosList[lu.y][lu.x].x;
                if (standardPos.x > nodePlacePosList[rd.y][rd.x].x)
                    standardPos.x = nodePlacePosList[rd.y][rd.x].x;
                if (standardPos.y > nodePlacePosList[lu.y][lu.x].y)
                    standardPos.y = nodePlacePosList[lu.y][lu.x].y;
                if (standardPos.y < nodePlacePosList[rd.y][rd.x].y)
                    standardPos.y = nodePlacePosList[rd.y][rd.x].y;
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
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
                } else if(checkDir > 0.0f) {
                    slideDir = _eSlideDir.RIGHTUP;
                }

                // タップしているノードを移動
                Vec2Int ru = SearchLimitNode(tapNodeID, _eLinkDir.RU);
                Vec2Int ld = SearchLimitNode(tapNodeID, _eLinkDir.LD);
                standardPos = moveNodeInitPos + moveDist;
                if (standardPos.x > nodePlacePosList[ru.y][ru.x].x)
                    standardPos.x = nodePlacePosList[ru.y][ru.x].x;
                if (standardPos.x < nodePlacePosList[ld.y][ld.x].x)
                    standardPos.x = nodePlacePosList[ld.y][ld.x].x;
                if (standardPos.y > nodePlacePosList[ru.y][ru.x].y)
                    standardPos.y = nodePlacePosList[ru.y][ru.x].y;
                if (standardPos.y < nodePlacePosList[ld.y][ld.x].y)
                    standardPos.y = nodePlacePosList[ld.y][ld.x].y;
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
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

    public void StartSlideNodes(Vec2Int nextNodeID, _eSlideDir newSlideDir) {
        moveNodeDist = new Vector2(nodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.x, nodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.y)
                     - new Vector2(nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);   // スライド方向ベクトル兼移動量を算出
        moveNodeInitPos = nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position;      // ノードの移動開始位置を保存
        
        // スライド方向を設定
        slideDir = newSlideDir;

        // 絶対値を算出
        moveNodeDistAbs.x = Mathf.Abs(moveNodeDist.x);
        moveNodeDistAbs.y = Mathf.Abs(moveNodeDist.y);
        
        SlantMove();
    }

    // ゲームの画面外にはみ出したノードを逆側に移動する
    void LoopBackNode() {
        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // 両端のバッファノードに、ゲーム画面内の両端ノードのパラメータをコピーする
                nodeScripts[tapNodeID.y][AdjustRow(tapNodeID.y) - 1].CopyParameter(nodeScripts[tapNodeID.y][1]);
                nodeScripts[tapNodeID.y][0].CopyParameter(nodeScripts[tapNodeID.y][AdjustRow(tapNodeID.y) - 2]);

                // はみ出したノードをソートする
                if(slideDir == _eSlideDir.LEFT) {
                    if(nodeScripts[tapNodeID.y][0].IsOutScreen) {
                        SortLeftOutNode();
                    }
                } else {
                    if(nodeScripts[tapNodeID.y][AdjustRow(tapNodeID.y) - 1].IsOutScreen) {
                        SortRightOutNode();
                    }
                }

                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                Vec2Int lu = SearchLimitNode(tapNodeID, _eLinkDir.LU);  // 左上端のノードIDを検索
                Vec2Int rd = SearchLimitNode(tapNodeID, _eLinkDir.RD);  // 右下端のノードIDを検索
                
                // 両端のバッファノードに、ゲーム画面内の両端ノードのパラメータをコピーする
                Vec2Int lurdTmp = SearchLimitNode(rd, _eLinkDir.LU);
                nodeScripts[lu.y][lu.x].CopyParameter(nodeScripts[lurdTmp.y][lurdTmp.x]);
                lurdTmp = SearchLimitNode(lu, _eLinkDir.RD);
                nodeScripts[rd.y][rd.x].CopyParameter(nodeScripts[lurdTmp.y][lurdTmp.x]);
                
                // はみ出したノードをソートする
                if(slideDir == _eSlideDir.LEFTUP) {
                    if(nodeScripts[lu.y][lu.x].IsOutScreen) {
                        SortLeftUpOutNode(lu);
                    }
                } else {
                    if(nodeScripts[rd.y][rd.x].IsOutScreen) {
                        SortRightDownOutNode(rd);
                    }
                }

                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                Vec2Int ru = SearchLimitNode(tapNodeID, _eLinkDir.RU);  // 右上端のノードIDを検索
                Vec2Int ld = SearchLimitNode(tapNodeID, _eLinkDir.LD);  // 左下端のノードIDを検索
                
                // 両端のバッファノードに、ゲーム画面内の両端ノードのパラメータをコピーする
                Vec2Int ruldTmp = SearchLimitNode(ld, _eLinkDir.RU);
                nodeScripts[ru.y][ru.x].CopyParameter(nodeScripts[ruldTmp.y][ruldTmp.x]);
                ruldTmp = SearchLimitNode(ru, _eLinkDir.LD);
                nodeScripts[ld.y][ld.x].CopyParameter(nodeScripts[ruldTmp.y][ruldTmp.x]);
                
                // はみ出したノードをソートする
                if(slideDir == _eSlideDir.RIGHTUP) {
                    if(nodeScripts[ru.y][ru.x].IsOutScreen) {
                        SortRightUpOutNode(ru);
                    }
                } else {
                    if(nodeScripts[ld.y][ld.x].IsOutScreen) {
                        SortLeftDownOutNode(ld);
                    }
                }

                break;
        }
    }

    // 移動を終了するノードの位置を調整する
    void AdjustNodeStop() {
        Vector2 pos         = Vector2.zero;
        Vector2 standardPos = Vector2.zero;
        Vec2Int nextNodeID  = Vec2Int.zero;

        // ノードIDを調整
        tapNodeID = SearchTapNode(nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position);

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // タップしているノードを移動
                if(tapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[tapNodeID.y + 2][tapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[tapNodeID.y - 2][tapNodeID.x].transform.position.x;
                }
                standardPos.y = nodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y;
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左側のノードを移動
                for(int i = tapNodeID.x - 1, j = 1; i >= 0; --i, ++j) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[tapNodeID.y][i].SlideNode(slideDir, pos);
                }
                // タップしているノードより右側のノードを移動
                for(int i = tapNodeID.x + 1, j = 1; i < AdjustRow(tapNodeID.y); ++i, ++j) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * j;
                    pos.y = standardPos.y;
                    nodeScripts[tapNodeID.y][i].SlideNode(slideDir, pos);
                }
                break;
                
            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // タップしているノードを移動
                if(tapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[tapNodeID.y + 2][tapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[tapNodeID.y - 2][tapNodeID.x].transform.position.x;
                }
                if(tapNodeID.x + 1 < AdjustRow(tapNodeID.y)) {
                    standardPos.y = nodePrefabs[tapNodeID.y][tapNodeID.x + 1].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[tapNodeID.y][tapNodeID.x - 1].transform.position.y;
                }
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);
                
                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
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
                if(tapNodeID.y + 2 < col) {
                    standardPos.x = nodePrefabs[tapNodeID.y + 2][tapNodeID.x].transform.position.x;
                } else {
                    standardPos.x = nodePrefabs[tapNodeID.y - 2][tapNodeID.x].transform.position.x;
                }
                if(tapNodeID.x + 1 < AdjustRow(tapNodeID.y)) {
                    standardPos.y = nodePrefabs[tapNodeID.y][tapNodeID.x + 1].transform.position.y;
                } else {
                    standardPos.y = nodePrefabs[tapNodeID.y][tapNodeID.x - 1].transform.position.y;
                }
                nodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
                for(int i = 1; nextNodeID.x > -1; ++i) {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    nodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
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
        GameObject outNode = nodePrefabs[tapNodeID.y][0];
        Node outNodeScript = nodeScripts[tapNodeID.y][0];
        
        // ノード入れ替え処理
        for (int i = 1; i < AdjustRow(tapNodeID.y); ++i) {
            CopyNodeInfo(i - 1, tapNodeID.y, nodePrefabs[tapNodeID.y][i], nodeScripts[tapNodeID.y][i]);
        }
        CopyNodeInfo(AdjustRow(tapNodeID.y) - 1, tapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[tapNodeID.y][AdjustRow(tapNodeID.y) - 2].transform.position;
        pos.x += moveNodeDistAbs.x;
        nodeScripts[tapNodeID.y][AdjustRow(tapNodeID.y) - 1].StopTween();
        nodePrefabs[tapNodeID.y][AdjustRow(tapNodeID.y) - 1].transform.position = pos;

        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
    }

    // 右にはみ出たノードを逆側に移動し、ノード情報をソートする
    void SortRightOutNode() {
        GameObject outNode = nodePrefabs[tapNodeID.y][AdjustRow(tapNodeID.y) - 1];
        Node outNodeScript = nodeScripts[tapNodeID.y][AdjustRow(tapNodeID.y) - 1];
        
        // ノード入れ替え処理
        for (int i = AdjustRow(tapNodeID.y) - 1; i >= 1; --i) {
            CopyNodeInfo(i, tapNodeID.y, nodePrefabs[tapNodeID.y][i - 1], nodeScripts[tapNodeID.y][i - 1]);
        }
        CopyNodeInfo(0, tapNodeID.y, outNode, outNodeScript);

        // 位置を調整
        Vector3 pos = nodePrefabs[tapNodeID.y][1].transform.position;
        pos.x -= moveNodeDistAbs.x;
        nodeScripts[tapNodeID.y][0].StopTween();
        nodePrefabs[tapNodeID.y][0].transform.position = pos;
        
        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
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
        pos.x += moveNodeDistAbs.x;
        pos.y -= moveNodeDistAbs.y;
        nodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
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
        pos.x -= moveNodeDistAbs.x;
        pos.y += moveNodeDistAbs.y;
        nodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
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
        pos.x -= moveNodeDistAbs.x;
        pos.y -= moveNodeDistAbs.y;
        nodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
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
        pos.x += moveNodeDistAbs.x;
        pos.y += moveNodeDistAbs.y;
        nodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
        nodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;
        
        // 選択中のノードIDを更新
        tapNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
    }

    // タップしているノードのIDを、座標を基準に検索する
    Vec2Int SearchTapNode(Vector3 pos) {
        Vec2Int id = new Vec2Int(-1, -1);

        // タップしているノードのIDを検索(Y方向)
        for(int i = 1; i < col; ++i) {
            if(pos.y < nodePlacePosList[i][0].y) {
                // 上のノードと下のノードで、距離が近い方を選択ノードとして設定する
                if(nodePlacePosList[i][0].y - pos.y < pos.y - nodePlacePosList[i - 1][0].y) {
                    // フレームをタップしていたら -1 を設定
                    if(i >= col - 1) {
                        id.x = -1;
                        id.y = -1;
                        break;
                    }

                    // 上のノードを設定
                    id.y = i;
                } else {
                    // フレームをタップしていたら -1 を設定
                    if(i - 1 <= 0) {
                        id.x = -1;
                        id.y = -1;
                        break;
                    }

                    // 下のノードを設定
                    id.y = i - 1;
                }

                break;
            }
        }
        if(id.y > 0) {
            // タップしているノードのIDを検索(X方向)
            for(int i = 1; i < AdjustRow(id.y); ++i) {
                if(pos.x < nodePlacePosList[id.y][i].x) {
                    // 右のノードと左のノードで、距離が近い方を選択ノードとして設定する
                    if(nodePlacePosList[id.y][i].x - pos.x < pos.x - nodePlacePosList[id.y][i - 1].x) {
                        // フレームをタップしていたら -1 を設定
                        if(i >= AdjustRow(id.y) - 1) {
                            id.x = -1;
                            id.y = -1;
                            break;
                        }

                        // 右のノードを設定
                        id.x = i;
                    } else {
                        // フレームをタップしていたら -1 を設定
                        if(i - 1 <= 0) {
                            id.x = -1;
                            id.y = -1;
                            break;
                        }

                        // 左のノードを設定
                        id.x = i - 1;
                    }

                    break;
                }
            }
        }

        return id;
    }

    // ゲーム画面外にはみ出ているかチェックする
    public void CheckOutScreen(int x, int y) {
        if(nodeScripts[y][x].GetLeftPos()   > gameArea.right  ||
           nodeScripts[y][x].GetRightPos()  < gameArea.left   ||
           nodeScripts[y][x].GetTopPos()    < gameArea.bottom ||
           nodeScripts[y][x].GetBottomPos() > gameArea.top)
            nodeScripts[y][x].IsOutScreen = true;
        else
            nodeScripts[y][x].IsOutScreen = false;
    }

    // ゲーム画面外にはみ出ているかチェックする
    public void CheckOutScreen(Vec2Int id) {
        CheckOutScreen(id.x, id.y);
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
    
    // リンク方向の端のノードIDを算出する
    Vec2Int SearchLimitNode(Vec2Int id, _eLinkDir dir) {
        Vec2Int limitNodeID = id;
        while(GetDirNode(limitNodeID, dir).x > -1) {
            limitNodeID = GetDirNode(limitNodeID, dir);
        }

        return limitNodeID;
    }
    
    // リンク方向の端のノードIDを算出する
    Vec2Int SearchLimitNode(int x, int y, _eLinkDir dir) {
        return SearchLimitNode(new Vec2Int(x, y), dir);
    }
    
    // リンク方向のゲーム画面内の端のノードIDを算出する
    Vec2Int SearchLimitNodeInScreen(Vec2Int id, _eLinkDir dir) {
        Vec2Int limitNodeID = id;
        while(GetDirNode(limitNodeID, dir).x > -1) {
            Vec2Int tmp = GetDirNode(limitNodeID, dir);
            if (nodeScripts[tmp.y][tmp.x].IsOutScreen) {
                break;
            }
            limitNodeID = tmp;
        }

        return limitNodeID;
    }
    
    // リンク方向のゲーム画面内の端のノードIDを算出する
    Vec2Int SearchLimitNodeInScreen(int x, int y, _eLinkDir dir) {
        return SearchLimitNodeInScreen(new Vec2Int(x, y), dir);
    }

    // スライド方向を算出
    _eSlideDir CheckSlideDir(Vector3 pos, Vector3 toPos) {
        float angle = Mathf.Atan2(toPos.y - pos.y, toPos.x - pos.x);
        angle *= 180.0f / Mathf.PI;

        // スライド方向を算出
        _eSlideDir dir = _eSlideDir.NONE;
        if(angle < 30.0f && angle >= -30.0f) {          // 右
            dir = _eSlideDir.RIGHT;
        } else if(angle < 90.0f && angle >= 30.0f) {    // 右上
            dir = _eSlideDir.RIGHTUP;
        } else if(angle < 150.0f && angle >= 90.0f) {   // 左上
            dir = _eSlideDir.LEFTUP;
        } else if(angle < -150.0f || angle > 150.0f) {  // 左
            dir = _eSlideDir.LEFT;
        } else if(angle < -90.0f && angle >= -150.0f) { // 左下
            dir = _eSlideDir.LEFTDOWN;
        } else if(angle < -30.0f && angle >= -90.0f) {  // 右下
            dir = _eSlideDir.RIGHTDOWN;
        }

        return dir;
    }

    // ゲーム画面内に収まるよう座標を調整
    Vector2 AdjustGameScreen(Vector2 pos) {
        Vector2 newPos = pos;

        if(newPos.x < gameArea.left)
            newPos.x = gameArea.left;
        if(newPos.x > gameArea.right)
            newPos.x = gameArea.right;
        if(newPos.y > gameArea.top)
            newPos.y = gameArea.top;
        if(newPos.y < gameArea.bottom)
            newPos.y = gameArea.bottom;

        return newPos;
    }

    public void CheckLink()
    {
        bool bComplete = false; //完成した枝があるか？
        int nodeNum = 0;
        //すべてのノードの根本を見る
        for (int i = 1; i < AdjustRow(1) - 1; i++)
        {
            nodeNum = nodeScripts[1][i].CheckBit(_eLinkDir.NONE,0);
            if(nodeNum >= 3)   //親ノードの方向は↓向きのどちらかにしておく
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
            ReplaceNodeTree();
            print("枝が完成しました！");
        }
        for (int i = 2; i < col - 3; i++  )
        {
            for(int j = 1 ; j < AdjustRow(i) - 1; j++)
            {
                if (!nodeScripts[i][j].ChainFlag)
                    nodeScripts[i][j].ChangeEmissionColor(0);
                    //nodeScripts[i][j].MeshRenderer.material.color = new Color(1.0f, 1.0f, 1.0f);
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
    public Vec2Int GetDirNode(Vec2Int nodeID, _eLinkDir toDir)
    {
        return GetDirNode(nodeID.x, nodeID.y, toDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(int x, int y, _eSlideDir toSlideDir)
    {
        // _eSlideDir と _eLinkDir をコンバート
        _eLinkDir toDir = _eLinkDir.NONE;
        switch(toSlideDir) {
            case _eSlideDir.LEFT:
                toDir = _eLinkDir.L;
                break;
            case _eSlideDir.RIGHT:
                toDir = _eLinkDir.R;
                break;
            case _eSlideDir.LEFTUP:
                toDir = _eLinkDir.LU;
                break;
            case _eSlideDir.LEFTDOWN:
                toDir = _eLinkDir.LD;
                break;
            case _eSlideDir.RIGHTUP:
                toDir = _eLinkDir.RU;
                break;
            case _eSlideDir.RIGHTDOWN:
                toDir = _eLinkDir.RD;
                break;
        }

        return GetDirNode(x, y, toDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(Vec2Int nodeID, _eSlideDir toSlideDir)
    {
        return GetDirNode(nodeID.x, nodeID.y, toSlideDir);
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
    void ReplaceNodeTree()
    {
        int nNode = 0;
        int nCap = 0;
        int nPath2 = 0;
        int nPath3 = 0;

        //完成時演出のためにマテリアルをコピーしてから、
        List<GameObject> treeNodes = new List<GameObject>();
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < AdjustRow(i); j++)
            {
                if (nodeScripts[i][j].CompleteFlag)
                    treeNodes.Add(nodePrefabs[i][j]);
            }

        }
        GameObject newTree = (GameObject)Instantiate(treePrefab, transform.position, transform.rotation);
        newTree.GetComponent<treeController>().SetTree(treeNodes);


        //ノードを再配置
        for (int i = 0; i < col ; i++)
        {
            for (int j = 0; j < AdjustRow(i); j++ )

                if (nodeScripts[i][j].CompleteFlag)
                {
                    nNode++;
                    switch (nodeScripts[i][j].GetLinkNum())
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

                    ReplaceNode(nodeScripts[i][j]);
                }
        }
        scoreScript.PlusScore(nNode, nCap, nPath2, nPath3);
        timeScript.PlusTime(nNode, nCap, nPath2, nPath3);
        feverScript.Gain(nNode,nCap,nPath2,nPath3);


    }

    public void ReplaceNodeAll()
    {
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < AdjustRow(i); j++)
            {
                ReplaceNode(nodeScripts[i][j]);
            }
        }
    }

    //ノードにテーブルもたせたくなかったので
    public Color GetNodeColor(int colorNum)
    {
        return levelTableScript.GetNodeColor(colorNum);
    }

    //ノード全変更時の演出
    public void RotateAllNode()
    {
        for (int i = 0; i < col ; i++)
        {
            for (int j = 0; j < AdjustRow(i); j++ )
            {
                Vector3 angle = nodeScripts[i][j].transform.localEulerAngles;
                angle.y += 90.0f;
                nodeScripts[i][j].transform.DORotate(angle, (repRotateTime / 2.0f));
            }
        }

    }

    //全ノードがくるっと回転して状態遷移するやつ 再配置関数を引数に
    public IEnumerator ReplaceRotate(Replace repMethod)
    {
        //全ノードを90°回転tween
        RotateAllNode();

        yield return new WaitForSeconds(repRotateTime / 2.0f);
        //置き換え処理
        repMethod();
        //全ノードを-90°回転
        for (int i = 0; i < col; i++)
        {
            for (int j = 0; j < AdjustRow(i); j++)
            {
                nodeScripts[i][j].transform.DOKill();
                Vector3 angle = nodeScripts[i][j].transform.localEulerAngles;
                angle.y -= 180.0f;
                nodeScripts[i][j].transform.rotation = Quaternion.identity;
                nodeScripts[i][j].transform.Rotate(angle);
            }
        }

        //全ノードを90°回転
        RotateAllNode();
    }
}
