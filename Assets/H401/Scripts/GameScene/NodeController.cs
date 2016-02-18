using UnityEngine;
using System.Collections;
using UniRx;

using DG.Tweening;
using System.Collections.Generic;
using RandExtension;
using BitArrayExtension;
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

public class NodeController : MonoBehaviour
{
    
    //private static readonly CustomDebug//Log.CDebugLog Log = new CustomDebug//Log.CDebugLog("NodeController");
    
    private const float FRAME_POSZ_MARGIN = -1.0f;              // フレームとノードとの距離(Z座標)
    private const float NODE_EASE_STOP_THRESHOLD = 0.01f;       // ノードの easing を終了するための、タップ位置とノード位置との閾値

    #region // ノードの配置数に関する変数
    [SerializeField]
    private int row = 0;                    // 横配置数 リストIDが奇数の行は＋１とする
    [SerializeField]
    private int col = 0;                    // 縦配置数
    public int Row
    {
        get { return this.row; }
    }
    public int Col
    {
        get { return this.col; }
    }
    #endregion

    #region // ノード系プレハブへのパス、実体
    [SerializeField] private string gameNodePrefabPath = null;          // ノードのプレハブのパス
    [SerializeField] private string frameNodePrefabPath = null;         // フレームノードのプレハブのパス
    [SerializeField] public string gameNodeSpritePath = null;          // ノードのスプライトのパス
    [SerializeField] public string nodeMaskSpritePath = null;          // ノードマスクのスプライトのパス
    [SerializeField] public string frameNodeSpritePath = null;         // フレームノードのスプライトのパス

    private GameObject gameNodePrefab = null;                 // ノードのプレハブ
    private GameObject frameNodePrefab = null;                 // フレームノードのプレハブ

    private Sprite[] gameNodeSprite;    // ノードのスプライトリスト
    public Sprite GetSprite(int Idx) {
        if (gameNodeSprite != null && gameNodeSprite.Length > Idx && Idx > -1)
        {
            return gameNodeSprite[Idx];
        }
        return null;
    }
    private Sprite[] nodeMaskSprite;    // ノードマスクのスプライトリスト
    public Sprite GetMaskSprite(int Idx)
    {
        if (nodeMaskSprite != null && nodeMaskSprite.Length > Idx && Idx > -1)
        {
            return nodeMaskSprite[Idx];
        }
        return null;
    }
    private Sprite[] frameNodeSprite;   // フレームノードのスプライトリスト

    [SerializeField]
    private string unChainControllerPath = null;
    private GameObject unChainControllerPrefab = null;
    
    [SerializeField] public string gameArrowPrefabPath = null;
    private GameObject gameArrowPrefab = null;
    #endregion

    #region // ノードの位置、サイズについての変数
    [SerializeField]
    private float widthMargin = 0.0f;      // ノード位置の左右間隔の調整値
    [SerializeField]
    private float heightMargin = 0.0f;      // ノード位置の上下間隔の調整値
    [SerializeField]
    private float headerHeight = 0.0f;      // ヘッダーの高さ
    private Vector2 nodeSize = Vector2.zero;                    // 描画するノードのサイズ
    public Vector2 NodeSize
    {
        get { return nodeSize; }
    }
    #endregion


    
    private bool _isNodeAction = false;                // ノードがアクション中かフラグ
    private bool IsNodeAction
    {
        get
        {
            if(gameNodeScripts == null)
            {
                return false;
            }
            for (int i = 0; i < col; ++i)
            {
                foreach (var nodes in gameNodeScripts[i])
                {
                    if (nodes.IsAction)
                        return true;
                }
            }
            return false;
        }
    }

    #region // スライドに使用する変数
    private bool isTap = false;                // タップ成功フラグ
    private bool isSlide = false;                // ノードスライドフラグ
    public bool isNodeLock { get { return isSlide; } set{ isSlide = value; } }     //
    private bool isSlideEnd = false;                // ノードがスライド終了処理中かフラグ
    private Vec2Int tapNodeID = Vec2Int.zero;         // タップしているノードのID
    private _eSlideDir slideDir = _eSlideDir.NONE;      // スライド中の方向
    private Vector2 moveNodeInitPos = Vector2.zero;         // 移動中ノードの移動開始位置
    private Vector2 moveNodeDist = Vector2.zero;         // 移動中ノードの基本移動量(移動方向ベクトル)
    private Vector2 moveNodeDistAbs = Vector2.zero;         // 移動中ノードの基本移動量の絶対値
    private Vector2 tapPosMoveNodePosDist = Vector2.zero;   // タップしているノードの位置と、タップしている位置との距離

    private Vector2 startTapPos = Vector2.zero;                 // タップした瞬間の座標
    private Vector2 tapPos = Vector2.zero;                 // タップ中の座標
    private Vector2 prevTapPos = Vector2.zero;                 // 前フレームのタップ座標

    private Vector2 slideLeftUpPerNorm = Vector2.zero;        // 左上ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)
    private Vector2 slideLeftDownPerNorm = Vector2.zero;        // 左下ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)

    private Vec2Int slidingLimitNodeID = Vec2Int.zero;   // スライド方向の端ノードのID
    private Vec2Int slidingReverseLimitNodeID = Vec2Int.zero;   // スライド方向の逆端ノードのID

    [SerializeField] private float slideIntervalPlaySE = 0.0f;  // スライドSEを再生する距離(間隔)
    private float cumSlideIntervalPlaySE = 0.0f;    // SEを再生するまでのスライド量の累計

    public _eSlideDir SlideDir
    {
        get { return slideDir; }
    }
    #endregion
    
    [SerializeField]
    private float repRotateTime = 0;        //ノード再配置時の時間
    [SerializeField]
    private NodeTemplate[] NodeTemp = null;

    private GameObject[][] gameNodePrefabs;             // ノードのプレハブリスト
    private Node[][]       gameNodeScripts;             // ノードのnodeスクリプトリスト
    private Vector3[][]    nodePlacePosList;            // ノードの配置位置リスト
    private GameObject     frameController;             // フレームコントローラープレハブ
    private GameEffect     gameEffect;                  // GameEffect スクリプト
    private GameObject     arrowController;             // GameEffect スクリプト
    
    //private bool _isNodeAction = false;                // ノードがアクション中かフラグ

    //ノードの配置割合を記憶しておく
    private float RatioSum = 0.0f;                              // 合計割合

    #region // レベル、ポーズ、スコア、タイム、フィーバー、アンチェインのスクリプト
    private FieldLevelInfo fieldLevel;
    private LevelTables _levelTableScript = null;
    public LevelTables levelTableScript
    {
        get
        {
            if (!_levelTableScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _levelTableScript = gameScene.levelTables;
            }
            return _levelTableScript;
        }
    }
    private LevelController _levelControllerScript;
    public LevelController levelControllerScript
    {
        get
        {
            if (!_levelControllerScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _levelControllerScript = gameScene.gameUI.levelCotroller;
            }
            return _levelControllerScript;
        }
    }
    private GameOption _pauseScript;
    public GameOption pauseScript
    {
        get
        {
            if (!_pauseScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _pauseScript = gameScene.gameUI.gamePause;
            }
            return _pauseScript;
        }
    }
    private UnChainController _unChainControllerScript;
    public UnChainController unChainController { get { return _unChainControllerScript; } }
    
    private Score _scoreScript = null;          //スコアのスクリプト
    public Score scoreScript
    {
        get
        {
            if (!_scoreScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _scoreScript = gameScene.gameUI.gameInfoCanvas.score;
            }
            return _scoreScript;
        }
    }
    private LimitTime _timeScript = null;             //制限時間のスクリプト
    public LimitTime timeScript
    {
        get
        {
            if (!_timeScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _timeScript = gameScene.gameUI.gameInfoCanvas.limitTime;
            }
            return _timeScript;
        }
    }
    private FeverGauge _feverScript = null;
    public FeverGauge feverScript
    {
        get
        {
            if (!_feverScript)
            {
                GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
                _feverScript = gameScene.gameUI.gameInfoCanvas.feverGauge;
            }
            return _feverScript;
        }
    }
    private int _currentLevel =1;
    public int currentLevel
    {
        get { return _currentLevel; }
        set
        {
            _currentLevel = value;
            fieldLevel = levelTableScript.GetFieldLevel(_currentLevel);
            RatioSum = fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3 + fieldLevel.Ratio_Path4;

            GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
            gameScene.gameUI.gameInfoCanvas.levelImageNum = _currentLevel;
            StartCoroutine(ReplaceRotate(ReplaceNodeAll));
        }
    }
    #endregion

    //音声
    private AudioSource[] audioSources;
    enum _eAudioNumber {
        ROTATE,
        SLIDE,
        TREE,

        MAX,
    }

    public Vector3[][] NodePlacePosList { get { return nodePlacePosList; } }

    void Awake()
    {
        ////Log.Debug("Awake");
        gameNodePrefabs = new GameObject[col][];
        gameNodeScripts = new Node[col][];
        nodePlacePosList = new Vector3[col][];
        for (int i = 0; i < col; ++i)
        {
            int adjustRow = AdjustRow(i);
            gameNodePrefabs[i] = new GameObject[adjustRow];
            gameNodeScripts[i] = new Node[adjustRow];
            nodePlacePosList[i] = new Vector3[adjustRow];
        }
    }

    // Use this for initialization
    void Start()
    {
        ////Log.Debug("Start");

        //音声データを取得 添字をenum化すべきか
        audioSources = GetComponents<AudioSource>();
        /*if (audioSources.Length != (int)_eAudioNumber.MAX)
            print("音声取得に失敗");*/

        // ----- プレハブデータを Resources から取得
        gameNodePrefab = Resources.Load<GameObject>(gameNodePrefabPath);
        frameNodePrefab = Resources.Load<GameObject>(frameNodePrefabPath);
        gameArrowPrefab = Resources.Load<GameObject>(gameArrowPrefabPath);

        // ----- スプライトデータを Resources から取得
        gameNodeSprite = Resources.LoadAll<Sprite>(gameNodeSpritePath);
        nodeMaskSprite = Resources.LoadAll<Sprite>(nodeMaskSpritePath);
        frameNodeSprite = Resources.LoadAll<Sprite>(frameNodeSpritePath);

        // GameEffect スクリプトを取得
        gameEffect = transform.GetComponent<GameEffect>();

        // ----- ノード準備
        NodeTemplate.AllCalc(NodeTemp);
        InitNode();
        //開始演出が終わるまでは操作を受け付けない
        SetSlideAll(true);

        // スライドベクトルの垂線を算出
        Vector3 leftUp = gameNodePrefabs[0][1].transform.position - gameNodePrefabs[0][0].transform.position;
        Vector3 leftDown = gameNodePrefabs[1][1].transform.position - gameNodePrefabs[0][0].transform.position;
        Matrix4x4 mtx = Matrix4x4.identity;
        mtx.SetTRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 90.0f), new Vector3(1.0f, 1.0f, 1.0f));
        leftUp = mtx.MultiplyVector(leftUp).normalized;
        leftDown = mtx.MultiplyVector(leftDown).normalized;
        slideLeftUpPerNorm = new Vector2(leftUp.x, leftUp.y);
        slideLeftDownPerNorm = new Vector2(leftDown.x, leftDown.y);

        // SE準備
        cumSlideIntervalPlaySE = 0.0f;

        // ----- インプット処理
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => {
                // タップに成功していなければ未処理
                if (!isTap)
                {
                    return;
                }
                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // スライド処理
                if (isSlide)
                {
                    prevTapPos = tapPos;
                    tapPos = new Vector2(worldTapPos.x, worldTapPos.y);

                    SlideNodes();
                    CheckSlideOutLimitNode();
                    LoopBackNode();

                    return;
                }

                // スライド判定
                if (tapNodeID.x > -1 && startTapPos != (Vector2)worldTapPos)
                {
                    _eSlideDir dir = CheckSlideDir(startTapPos, new Vector2(worldTapPos.x, worldTapPos.y));
                    Vec2Int nextNodeID = GetDirNode(tapNodeID, dir);
                    if (nextNodeID.x > -1)
                    {
                        if (Vector3.Distance(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position, worldTapPos) >
                            Vector3.Distance(gameNodePrefabs[nextNodeID.y][nextNodeID.x].transform.position, worldTapPos))
                        {
                            isSlide = true;
                            _isNodeAction = true;
                            StartSlideNodes(nextNodeID, dir);
                        }
                    }
                }
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Subscribe(_ => {
                // スライド中なら未処理
                if (isSlide)
                    return;
                if (pauseScript.pauseState == _ePauseState.PAUSE)
                    return;

                if (levelControllerScript.LevelState != _eLevelState.STAND)
                    return;

                // タップ成功
                isTap = true;
                ////Log.Debug("MouseButtonDown");

                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                startTapPos = new Vector2(worldTapPos.x, worldTapPos.y);

                // タップしているノードのIDを検索
                tapNodeID = SearchNearNodeRemoveFrame(worldTapPos);
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => TapRelease()/*{
                // タップに成功していなければ未処理
                if (!isTap)
                    return;

                // タップ終了
                isTap = false;
                //Log.Debug("MouseButtonUp");

                if (isSlide)
                {
                    AdjustNodeStop();
                    isSlideEnd = true;
                    tapNodeID = Vec2Int.zero;
                }
                else {
                    if (tapNodeID.x > -1)
                    {
                        audioSources[(int)_eAudioNumber.ROTATE].Play();
                        gameNodeScripts[tapNodeID.y][tapNodeID.x].RotationNode();
                        _isNodeAction = true;
                    }
                }
            }*/)
            .AddTo(gameObject);
        
        // ノードのアニメーション終了と同時に接続チェック
        Observable
            .EveryUpdate()
            .Select(x =>  !(IsNodeAction) && !(isNodeLock))
            .DistinctUntilChanged()
            .Where(x => x)
            .ThrottleFrame(3)
            .Subscribe(x => {
                CheckLink();
            })
            .AddTo(gameObject);

        // ノードがアクション中かチェック
        Observable
            .EveryUpdate()
            .Where(_ => _isNodeAction)
            .Subscribe(_ => {
                for (int i = 0; i < col; ++i)
                {
                    foreach (var nodes in gameNodeScripts[i])
                    {
                        if (nodes.IsAction)
                            return;
                    }
                }
                _isNodeAction = false;
            })
            .AddTo(gameObject);

        // ノードがスライド終了処理中かチェック
        Observable
            .EveryUpdate()
            .Where(_ => isSlideEnd)
            .Subscribe(_ => {
                for (int i = 0; i < col; ++i)
                {
                    foreach (var nodes in gameNodeScripts[i])
                    {
                        if (nodes.IsSlideEnd)
                        {
                            return;
                        }
                    }
                }
                isSlideEnd = false;
                if (isSlide)
                {
                    isSlide = false;
                    slideDir = _eSlideDir.NONE;

                    cumSlideIntervalPlaySE = 0.0f;
                }
            })
            .AddTo(gameObject);

        // SE再生処理
        Observable
            .EveryUpdate()
            .Where(_ => isSlide)
            .Subscribe(_ => {
                if(cumSlideIntervalPlaySE > slideIntervalPlaySE) {
                    audioSources[(int)_eAudioNumber.SLIDE].Play();

                    cumSlideIntervalPlaySE = 0.0f;
                }
            })
            .AddTo(gameObject);

        Observable
            .NextFrame()
            .Subscribe(_ => CheckLink(true, true))
            .AddTo(this);

    }

    // Update is called once per frame
    void Update()
    {
        // @デバッグ用
        if (Input.GetKeyDown(KeyCode.A)) { StartCoroutine(ReplaceRotate(ReplaceNodeFever)); };
        ////Log.Info("Update");
    }

    public void TapRelease()
    {
        // タップに成功していなければ未処理
        if (!isTap)
            return;

        // タップ終了
        isTap = false;
        ////Log.Debug("MouseButtonUp");

        if (isSlide)
        {
            AdjustNodeStop();
            isSlideEnd = true;
            tapNodeID = Vec2Int.zero;
        }
        else
        {
            if (tapNodeID.x > -1)
            {
                audioSources[(int)_eAudioNumber.ROTATE].Play();
                gameNodeScripts[tapNodeID.y][tapNodeID.x].RotationNode();
                _isNodeAction = true;
            }
        }
    }

    // ----- ノード準備
    void InitNode()
    {
        Vector3 pos = transform.position;

        // 描画するノードの大きさを取得
        SpriteRenderer sr = gameNodePrefab.GetComponent<SpriteRenderer>();
        nodeSize.x = sr.sprite.bounds.size.x * gameNodePrefab.transform.localScale.x;
        nodeSize.y = sr.sprite.bounds.size.y * gameNodePrefab.transform.localScale.y;
        nodeSize.x -= widthMargin;
        nodeSize.y -= heightMargin;

        //途切れ表示用のアレを生成
        unChainControllerPrefab = Instantiate(Resources.Load<GameObject>(unChainControllerPath));
        unChainControllerPrefab.transform.SetParent(transform);
        _unChainControllerScript = unChainControllerPrefab.GetComponent<UnChainController>();

        // フレームを生成
        frameController = new GameObject();
        frameController.transform.SetParent(transform.parent);
        frameController.name = "FrameController";

        // 矢印を生成
        arrowController = new GameObject();
        arrowController.transform.SetParent(transform.parent);
        arrowController.name = "ArrowController";

        Node.SetNodeController(this); //ノードにコントローラーを設定

        fieldLevel = levelTableScript.GetFieldLevel(1);
        RatioSum = fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3 + fieldLevel.Ratio_Path4;  //全体割合を記憶
        // ノードを生成
        for (int i = 0; i < col; ++i)
        {
            GameObject frameObject = null;

            // ノードの配置位置を調整(Y座標)
            pos.y = transform.position.y - headerHeight + nodeSize.y * -(col * 0.5f - (i + 0.5f));
            for (int j = 0; j < AdjustRow(i); ++j)
            {
                // ノードの配置位置を調整(X座標)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (j + 0.5f));
                pos.z = transform.position.z;

                // 生成
                gameNodePrefabs[i][j] = (GameObject)Instantiate(gameNodePrefab, pos, transform.rotation);
                gameNodeScripts[i][j] = gameNodePrefabs[i][j].GetComponent<Node>();
                gameNodePrefabs[i][j].transform.SetParent(transform);
                nodePlacePosList[i][j] = gameNodePrefabs[i][j].transform.position;

                // ノードの位置(リストID)を登録
                gameNodeScripts[i][j].RegistNodeID(j, i);

                //ランダムでノードの種類と回転を設定
                ReplaceNode(gameNodeScripts[i][j],i < 2);

                // フレーム生成(上端)
                if (i >= col - 1)
                {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(frameNodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                    frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.TOP_FRAME];
                    frameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
                }
                // フレーム生成(下端)
                if (i <= 0)
                {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(frameNodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                    frameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
                    // フレームのスプライトを変更
                    frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.GROUND];

                    // 矢印生成
                    GameObject arrowObject = (GameObject)Instantiate(gameArrowPrefab, framePos, transform.rotation);
                    arrowObject.transform.SetParent(arrowController.transform);
                }
            }

            if (i > 0 && i < col - 1)
            {
                // フレーム生成(左端)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (0 + 0.5f));
                pos.z = transform.position.z + FRAME_POSZ_MARGIN;
                frameObject = (GameObject)Instantiate(frameNodePrefab, pos, transform.rotation);
                frameObject.transform.parent = frameController.transform;
                frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.SIDE_FRAME];
                frameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;

                // フレーム生成(右端)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (AdjustRow(i) - 1 + 0.5f));
                frameObject = (GameObject)Instantiate(frameNodePrefab, pos, transform.rotation);
                frameObject.transform.parent = frameController.transform;
                frameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
                frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.SIDE_FRAME];

                frameObject.GetComponent<SpriteRenderer>().sortingOrder = 2;
            }
        }
    }

    // ノードのスライド移動処理
    void SlideNodes()
    {
        ////Log.Debug("SlideNodes");
        // スライド対象となるノードの準備
        Vector2 deltaSlideDist = tapPos - prevTapPos;   // 前回フレームからのスライド量
        float checkDir = 0.0f;                          // スライド方向チェック用

        switch (slideDir)
        {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // スライド方向を再計算
                if (tapPos.x - prevTapPos.x < 0.0f)
                {
                    // スライド方向が前フレームと違ったら更新
                    if (slideDir != _eSlideDir.LEFT)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.LEFT;
                    }
                }
                else if (tapPos.x - prevTapPos.x > 0.0f)
                {
                    if (slideDir != _eSlideDir.RIGHT)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.RIGHT;
                    }
                }

                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftUpPerNorm);
                if (checkDir < 0.0f)
                {
                    if (slideDir != _eSlideDir.LEFTUP)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.LEFTUP;
                    }
                }
                else if (checkDir > 0.0f)
                {
                    if (slideDir != _eSlideDir.RIGHTDOWN)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.RIGHTDOWN;
                    }
                }

                break;

            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // スライド方向を再計算
                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftDownPerNorm);
                if (checkDir < 0.0f)
                {
                    if (slideDir != _eSlideDir.LEFTDOWN)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.LEFTDOWN;
                    }
                }
                else if (checkDir > 0.0f)
                {
                    if (slideDir != _eSlideDir.RIGHTUP)
                    {
                        Vec2Int tmp = slidingLimitNodeID;
                        slidingLimitNodeID = slidingReverseLimitNodeID;
                        slidingReverseLimitNodeID = tmp;

                        slideDir = _eSlideDir.RIGHTUP;
                    }
                }

                break;

            default:
                break;
        }

        // ノードを移動
        AdjustSlideNodePosition();
    }

    //移動したいノードを確定
    //ドラッグを算出し移動したい方向列を確定
    //ドラッグされている間、列ごと移動、
    //タップ点からスワイプ地点まで座標の差分を算出し
    //列のすべてのノードをその位置へ移動させる
    //離すと一番近いノード確定位置まで調整

    public void StartSlideNodes(Vec2Int nextNodeID, _eSlideDir newSlideDir)
    {
        ////Log.Debug("StartSlideNodes : " + nextNodeID + " / " + newSlideDir);
        moveNodeDist = new Vector2(gameNodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.x, gameNodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.y)
                     - new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);   // スライド方向ベクトル兼移動量を算出
        moveNodeInitPos = gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position;      // ノードの移動開始位置を保存

        // スライド方向を設定
        slideDir = newSlideDir;

        // 絶対値を算出
        moveNodeDistAbs.x = Mathf.Abs(moveNodeDist.x);
        moveNodeDistAbs.y = Mathf.Abs(moveNodeDist.y);

        // スライド開始準備
        Vec2Int nextID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
        _eSlideDir reverseDir = ReverseDirection(slideDir);
        while (nextID.x > -1)
        {
            gameNodeScripts[nextID.y][nextID.x].StartSlide();
            nextID = GetDirNode(nextID, reverseDir);
        }

        // スライド方向の端のノードIDを算出
        slidingLimitNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
        slidingReverseLimitNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(ReverseDirection(slideDir)));

        // タップしているノードの位置と、タップしている位置との距離を算出
        tapPosMoveNodePosDist = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, startTapPos - moveNodeInitPos);
    }

    // ゲームの画面外にはみ出したノードを逆側に移動する
    void LoopBackNode()
    {
        ////Log.Debug("LoopBackNode");
        if (gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen)
        {
            gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = false;
            
            // はみ出たノードを逆側に移動し、ノード情報をソートする
            var dir = slideDir;
            var outNodeID = slidingLimitNodeID;
            GameObject outNode = gameNodePrefabs[outNodeID.y][outNodeID.x];
            Node outNodeScript = gameNodeScripts[outNodeID.y][outNodeID.x];
            _eSlideDir reverseDir = ReverseDirection(dir);
            Vector3 pos = Vector3.zero;

            // ノード入れ替え処理(スライド方向に置換していく)
            Vec2Int limitNodeID = outNodeID;
            Vec2Int prevSearchID = outNodeID;
            while (GetDirNode(limitNodeID, reverseDir).x > -1)
            {
                prevSearchID = limitNodeID;
                limitNodeID = GetDirNode(limitNodeID, reverseDir);
                CopyNodeInfo(prevSearchID.x, prevSearchID.y, gameNodePrefabs[limitNodeID.y][limitNodeID.x], gameNodeScripts[limitNodeID.y][limitNodeID.x]);
            }
            CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

            // 位置を調整
            prevSearchID = GetDirNode(limitNodeID, dir);
            pos = gameNodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
            switch (dir)
            {
                case _eSlideDir.LEFT:
                    pos = gameNodePrefabs[tapNodeID.y][AdjustRow(tapNodeID.y) - 2].transform.position;
                    pos.x += moveNodeDistAbs.x;
                    break;

                case _eSlideDir.RIGHT:
                    pos = gameNodePrefabs[tapNodeID.y][1].transform.position;
                    pos.x -= moveNodeDistAbs.x;
                    break;

                case _eSlideDir.LEFTUP:
                    pos.x += moveNodeDistAbs.x;
                    pos.y -= moveNodeDistAbs.y;
                    break;

                case _eSlideDir.LEFTDOWN:
                    pos.x += moveNodeDistAbs.x;
                    pos.y += moveNodeDistAbs.y;
                    break;

                case _eSlideDir.RIGHTUP:
                    pos.x -= moveNodeDistAbs.x;
                    pos.y -= moveNodeDistAbs.y;
                    break;

                case _eSlideDir.RIGHTDOWN:
                    pos.x -= moveNodeDistAbs.x;
                    pos.y += moveNodeDistAbs.y;
                    break;
            }
            gameNodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
            gameNodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;

            // 選択中のノードIDを更新
            tapNodeID = GetDirNode(tapNodeID, dir);

            Vec2Int copyInNodeID = SearchLimitNodeRemoveFrame(tapNodeID, ConvertSlideDirToLinkDir(SlideDir));
            Vec2Int copyOutNodeID = SearchLimitNodeRemoveFrame(tapNodeID, ConvertSlideDirToLinkDir(ReverseDirection(slideDir)));
            copyOutNodeID = GetDirNode(copyOutNodeID, ReverseDirection(slideDir));
            gameNodeScripts[copyOutNodeID.y][copyOutNodeID.x].CopyParameter(gameNodeScripts[copyInNodeID.y][copyInNodeID.x]);
        }
    }

    // 移動を終了するノードの位置を調整する
    void AdjustNodeStop()
    {
        ////Log.Debug("AdjustNodeStop");
        Vec2Int nearNodeID = SearchNearNode(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position);
        Vec2Int nextNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
        _eSlideDir reverseDir = ReverseDirection(slideDir);
        Vector2 pos = new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);
        Vector2 standardPos = new Vector2(nodePlacePosList[nearNodeID.y][nearNodeID.x].x, nodePlacePosList[nearNodeID.y][nearNodeID.x].y);

        // スライド方向を更新
        if (pos.x != standardPos.x || pos.y != standardPos.y)
        {
            _eSlideDir checkDir = CheckSlideDir(pos, standardPos);
            if (slideDir != checkDir)
            {
                slideDir = checkDir;

                Vec2Int tmp = slidingLimitNodeID;
                slidingLimitNodeID = slidingReverseLimitNodeID;
                slidingReverseLimitNodeID = tmp;
            }
        }

        // スライド方向のノードに、スライド終了を通知
        while (nextNodeID.x > -1)
        {
            gameNodeScripts[nextNodeID.y][nextNodeID.x].EndSlide();
            nextNodeID = GetDirNode(nextNodeID, reverseDir);
        }

        // 回り込み処理
        CheckSlideOutLimitNode();
        LoopBackNode();

        // 移動処理
        switch (slideDir)
        {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                pos = standardPos;

                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
                }
                // タップしているノードより右側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.R);
                }

                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
                }
                break;

            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
                }
                break;

            default:
                break;
        }
    }

    // 任意のノード情報をコピーする
    void CopyNodeInfo(int x, int y, GameObject prefab, Node script)
    {
        //Log.Debug("CopyNodeInfo : " + x + "/" + y + "/" + script);
        gameNodePrefabs[y][x] = prefab;
        gameNodeScripts[y][x] = script;
        gameNodeScripts[y][x].RegistNodeID(x, y);
    }

    // タップノードを中心に、スライド方向のノードの位置を設定する
    void AdjustSlideNodePosition()
    {
        //Log.Debug("AdjustSlideNodePosition");
        // スライド対象となるノードの準備
        Vector2 pos = tapPos;      // 移動位置
        Vector2 standardPos = tapPos;
        Vec2Int nextNodeID = Vec2Int.zero;     // 検索用ノードIDテンポラリ
        unChainController.RemoveWithNode(tapNodeID);
        switch (slideDir)
        {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // 移動位置をスライドライン上に調整
                pos = AdjustNodeLinePosition(slideDir);

                // SE再生用にスライド量を計上
                cumSlideIntervalPlaySE += Vector2.Distance(pos, new Vector2(gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.y));

                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, pos);

                // タップしているノードより左側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
                }
                // タップしているノードより右側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.R);
                }

                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // 移動位置をスライドライン上に調整
                standardPos = AdjustNodeLinePosition(slideDir);

                // SE再生用にスライド量を計上
                cumSlideIntervalPlaySE += Vector2.Distance(standardPos, new Vector2(gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.y));

                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより左上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
                }
                // タップしているノードより右下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
                }
                break;

            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // 移動位置をスライドライン上に調整
                standardPos = AdjustNodeLinePosition(slideDir);

                // SE再生用にスライド量を計上
                cumSlideIntervalPlaySE += Vector2.Distance(standardPos, new Vector2(gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodeScripts[tapNodeID.y][tapNodeID.x].transform.position.y));

                // タップしているノードを移動
                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

                // タップしているノードより右上側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
                }
                // タップしているノードより左下側のノードを移動
                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
                for (int i = 1; nextNodeID.x > -1; ++i)
                {
                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
                    unChainController.RemoveWithNode(nextNodeID);
                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
                }
                break;

            default:
                break;
        }
    }

    // タップ位置から、タップしているノードの、移動ライン上の座標を算出する
    Vector2 AdjustNodeLinePosition(_eSlideDir dir)
    {
        //Log.Debug("AdjustNodeLinePosition : " + dir);
        Vector2 adjustPos = tapPos;
        Vector2 slideDist = tapPos - startTapPos;     // スライド量
        Vector2 moveDist = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, slideDist);      // 斜め移動量

        switch (slideDir)
        {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                // タップしているノードの位置を調整
                //adjustPos = AdjustGameScreen(tapPos);
                adjustPos.y = gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y;
                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // タップしているノードの位置を調整
                Vec2Int lu = SearchLimitNode(tapNodeID, _eLinkDir.LU);
                Vec2Int rd = SearchLimitNode(tapNodeID, _eLinkDir.RD);
                adjustPos = moveNodeInitPos + moveDist + tapPosMoveNodePosDist;
                if (adjustPos.x < nodePlacePosList[lu.y][lu.x].x)
                    adjustPos.x = nodePlacePosList[lu.y][lu.x].x;
                if (adjustPos.x > nodePlacePosList[rd.y][rd.x].x)
                    adjustPos.x = nodePlacePosList[rd.y][rd.x].x;
                if (adjustPos.y > nodePlacePosList[lu.y][lu.x].y)
                    adjustPos.y = nodePlacePosList[lu.y][lu.x].y;
                if (adjustPos.y < nodePlacePosList[rd.y][rd.x].y)
                    adjustPos.y = nodePlacePosList[rd.y][rd.x].y;
                break;

            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // タップしているノードの位置を調整
                Vec2Int ru = SearchLimitNode(tapNodeID, _eLinkDir.RU);
                Vec2Int ld = SearchLimitNode(tapNodeID, _eLinkDir.LD);
                adjustPos = moveNodeInitPos + moveDist + tapPosMoveNodePosDist;
                if (adjustPos.x > nodePlacePosList[ru.y][ru.x].x)
                    adjustPos.x = nodePlacePosList[ru.y][ru.x].x;
                if (adjustPos.x < nodePlacePosList[ld.y][ld.x].x)
                    adjustPos.x = nodePlacePosList[ld.y][ld.x].x;
                if (adjustPos.y > nodePlacePosList[ru.y][ru.x].y)
                    adjustPos.y = nodePlacePosList[ru.y][ru.x].y;
                if (adjustPos.y < nodePlacePosList[ld.y][ld.x].y)
                    adjustPos.y = nodePlacePosList[ld.y][ld.x].y;
                break;

            default:
                break;
        }

        return adjustPos;
    }

    #region 各種Search関数
    // 任意の座標に最も近いノードのIDを、座標を基準に検索する
    Vec2Int SearchNearNode(Vector3 pos)
    {
        //Log.Debug("SerchNearNode : " + pos);
        Vec2Int id = new Vec2Int(-1, -1);
        float minDist = 99999.0f;

        // 検索処理
        for (int i = 0; i < col; ++i)
        {
            for (int j = 0; j < AdjustRow(i); ++j)
            {
                float dist = Vector3.Distance(pos, nodePlacePosList[i][j]);
                if (dist < minDist)
                {
                    minDist = dist;
                    id.x = j;
                    id.y = i;
                }
            }
        }

        return id;
    }

    // 任意の座標に最も近いノードのIDを、座標を基準に検索する(フレームノードを除く)
    Vec2Int SearchNearNodeRemoveFrame(Vector3 pos)
    {
        //Log.Debug("SerchNearNodeRemoveFrame");
        Vec2Int id = SearchNearNode(pos);

        // フレームなら -1 にする
        if (id.x <= 0 || id.x >= AdjustRow(id.y)-1 || id.y <= 0 || id.y >= col - 1)
        {
            id.x = -1;
            id.y = -1;
        }

        return id;
    }

    // リンク方向の端のノードIDを算出する
    Vec2Int SearchLimitNode(Vec2Int id, _eLinkDir dir)
    {
        //Log.Debug("SearchLimitNode : " + id + "/" + dir);
        Vec2Int limitNodeID = id;
        Vec2Int limitNodeIDTmp = GetDirNode(limitNodeID, dir);
        while (limitNodeIDTmp.x > -1)
        {
            limitNodeID = limitNodeIDTmp;
            limitNodeIDTmp = GetDirNode(limitNodeID, dir);
        }

        return limitNodeID;
    }

    // リンク方向の端のノードIDを算出する
    Vec2Int SearchLimitNode(int x, int y, _eLinkDir dir)
    {
        //Log.Debug("SerchLimitNode : " + x + "/" + y + "/" + dir);
        return SearchLimitNode(new Vec2Int(x, y), dir);
    }

    // リンク方向の端のノードIDを算出する(フレームノードを除く)
    Vec2Int SearchLimitNodeRemoveFrame(Vec2Int id, _eLinkDir dir)
    {
        //Log.Debug("SearchLimitNodeRemoveFrame : " + id + "/" + dir);
        Vec2Int limitNodeID = id;
        Vec2Int limitNodeIDTmp = GetDirNodeRemoveFrame(limitNodeID, dir);
        while (limitNodeIDTmp.x > -1)
        {
            limitNodeID = limitNodeIDTmp;
            limitNodeIDTmp = GetDirNodeRemoveFrame(limitNodeID, dir);
        }

        return limitNodeID;
    }

    // リンク方向の端のノードIDを算出する(フレームノードを除く)
    Vec2Int SearchLimitNodeRemoveFrame(int x, int y, _eLinkDir dir)
    {
        //Log.Debug("SearchLimitNodeRemoveFrame : " + x + "/" + y);
        return SearchLimitNodeRemoveFrame(new Vec2Int(x, y), dir);
    }
    #endregion

    // スライドしている列が画面外にはみ出ているかチェックする(フレームノードより外側にいるかどうか)
    void CheckSlideOutLimitNode()
    {
        //Log.Debug("CheckSlideOutLimitNode");
        switch (slideDir)
        {
            case _eSlideDir.LEFT:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            case _eSlideDir.RIGHT:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            case _eSlideDir.LEFTUP:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            case _eSlideDir.LEFTDOWN:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            case _eSlideDir.RIGHTUP:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            case _eSlideDir.RIGHTDOWN:
                if (gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y)
                {
                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
                }
                break;
            default:
                break;
        }
    }
    // スライド方向を算出
    _eSlideDir CheckSlideDir(Vector2 pos, Vector2 toPos)
    {
        //Log.Debug("CheckSlideDir : " + pos + "/" + toPos);
        float angle = Mathf.Atan2(toPos.y - pos.y, toPos.x - pos.x);
        angle *= 180.0f / Mathf.PI;

        // スライド方向を算出
        _eSlideDir dir = _eSlideDir.NONE;
        if (angle < 30.0f && angle >= -30.0f)
        {          // 右
            dir = _eSlideDir.RIGHT;
        }
        else if (angle < 90.0f && angle >= 30.0f)
        {    // 右上
            dir = _eSlideDir.RIGHTUP;
        }
        else if (angle < 150.0f && angle >= 90.0f)
        {   // 左上
            dir = _eSlideDir.LEFTUP;
        }
        else if (angle < -150.0f || angle > 150.0f)
        {  // 左
            dir = _eSlideDir.LEFT;
        }
        else if (angle < -90.0f && angle >= -150.0f)
        { // 左下
            dir = _eSlideDir.LEFTDOWN;
        }
        else if (angle < -30.0f && angle >= -90.0f)
        {  // 右下
            dir = _eSlideDir.RIGHTDOWN;
        }

        return dir;
    }

    #region // ノードとノードが繋がっているかを確認する
    // 接続に関するデバックログのON/OFF
    static bool bNodeLinkDebugLog = false;

    // ノードの接続を確認するチェッカー
    public class NodeLinkTaskChecker
    {
        static int IDCnt = 0;           // 管理用IDの発行に使用
        static public List<NodeLinkTaskChecker> Collector = new List<NodeLinkTaskChecker>();    // 動いているチェッカをしまっておくリスト
        static private List<NodeLinkTaskChecker> heap = new List<NodeLinkTaskChecker>();        // チェッカーヒープ

        public int ID = 0;              // 管理用ID
        public int Branch = 0;          // 枝の数
        public bool NotFin = false;     // 枝の"非"完成フラグ
        public int SumNode = 0;         // 合計ノード数(下の数取得で良いような。)
        public List<Node> NodeList = new List<Node>();    // 枝に含まれるノード。これを永続させて、クリック判定と組めば大幅な負荷軽減できるかも、
        private string Log = "";
        private bool Use = false;

        // コンストラクタ
        private NodeLinkTaskChecker() { }

        IObservable<int> CheckerRx;
        public void Init()
        {
            // IDを発行し、コレクタに格納
            ID = ++IDCnt;
            Collector.Add(this);
            Log = "";
            Log += "ID : " + ID.ToString() + "\n";
            Use = true;
            Branch = 0;
            NotFin = false;
            SumNode = 0;
            NodeList.Clear();
        }



        static public NodeLinkTaskChecker GetChecker()
        {
            foreach(var it in heap)
            {
                if (!it.Use)
                {
                    //Debug.Log("<color=green>HitHeap</color>");
                    it.Init();
                    return it;
                }
            }
            //Debug.Log("<color=orange>Heap Grow</color>");
            var ret = new NodeLinkTaskChecker();
            heap.Add(ret);
            ret.Init();
            return ret;
        }

        public void Dispose()
        {
            Collector.Remove(this);     // コレクタから削除
            Use = false;
        }

        // デバック用ToString
        public override string ToString()
        {
            string str = "";
            if (bNodeLinkDebugLog)
            {
                str += Log + "\n--------\n";
            }
            str +=
                "ID : " + ID.ToString() + "  " + NotFin + "\n" +
                " Branch : " + Branch.ToString() + "\n" +
                "SumNode : " + SumNode.ToString() + "\n";
            foreach (var it in NodeList)
            {
                str += it.ToString() + "\n";
            }
            return str;
        }

        // デバック用ログに書き出す
        static public NodeLinkTaskChecker operator +(NodeLinkTaskChecker Ins, string str)
        {
            Ins.Log += str + "\n";
            return Ins;
        }
    }

    // 接続をチェックする関数
    public void CheckLink(bool NoCheckLeftCallback = false,bool Unlinkmode = false) { 
        //Log.Debug("CheckLink");

        // ノードチェッカが帰ってきてないかチェック。これは結構クリティカルなんでログOFFでも出る仕様。
        if (NodeLinkTaskChecker.Collector.Count != 0 && Debug.isDebugBuild && !NoCheckLeftCallback)
        {
            string str = "Left Callback :" + NodeLinkTaskChecker.Collector.Count.ToString() + "\n";
            foreach (var it in NodeLinkTaskChecker.Collector)
            {
                str += it.ToString();
            }
            Debug.LogWarning(str);
        }
        ResetCheckedFragAll();          // 接続フラグを一度クリア

        // 根っこ分繰り返し
        for (int i = 1; i < row - 1; i++)
        {
            // チェッカを初期化
            var Checker = NodeLinkTaskChecker.GetChecker();

            // 根っこを叩いて処理スタート
            Observable
                .Return(i)
                .Subscribe(x => Seacher(Checker, x)).AddTo(this);                

            // キャッチャを起動
            Observable
                .NextFrame()
                .Repeat()
                .First(_ => Checker.Branch == 0)    // 処理中の枝が0なら終了
                .Subscribe(_ =>  Catcher(Checker, Unlinkmode) ).AddTo(this);
        }
        unChainController.Remove();

    }

    void Seacher(NodeLinkTaskChecker Checker, int x)
    {
        Checker += "firstNodeAct_Subscribe [" + Checker.ID + "]";
        Checker.Branch++;                                                   // 最初に枝カウンタを1にしておく(規定値が0なので+でいいはず)
        gameNodeScripts[1][x].NodeCheckAction(Checker, _eLinkDir.NONE);     // 下から順にチェックスタート。来た方向はNONEにしておいて根っこを識別。
    }

    void Catcher(NodeLinkTaskChecker Checker, bool Unlinkmode)
    {
        if (Debug.isDebugBuild && bNodeLinkDebugLog)
            Debug.Log("CheckedCallback_Subscribe [" + Checker.ID + "]" + Checker.SumNode.ToString() + "/" + (Checker.NotFin ? "" : "Fin") + "\n" + Checker.ToString());
        // ノード数1以上、非完成フラグが立ってないなら
        if (Checker.SumNode >= 1 && Checker.NotFin == false)
        {
            if (Unlinkmode)
            {
                UnlinkNodeTree(Checker.NodeList);
                Observable
                    .NextFrame()
                    //.ThrottleFrame(3)
                    .Subscribe(x => CheckLink(true, true)).AddTo(this);
            }
            else {
                ReplaceNodeTree(Checker.NodeList);   // 消去処理
            }
        }
        Checker.Dispose();      // チェッカは役目を終えたので消す
    }

    //閲覧済みフラグを戻す処理
    public void ResetCheckedFragAll()
    {
        //Log.Debug("ResetCheckedFragAll");
        for (int i = 0; i < col; ++i)
        {
            foreach (var nodes in gameNodeScripts[i])
            {
                nodes.ChainFlag = false;
                nodes.CheckFlag = false;
            }
        }
    }
    #endregion
    
    #region // 各種Get関数
    //ノードにテーブルもたせたくなかったので
    public Color GetNodeColor(int colorNum)
    {
        return levelTableScript.GetNodeColor(colorNum);
    }

    // 検索したい col に合わせた row を返す
    public int AdjustRow(int col)
    {
        return col % 2 == 0 ? row + 1 : row;
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(int x, int y, _eLinkDir toDir)
    {
        //走査方向のノードのcolとrow
        Vec2Int nextNodeID;

        nextNodeID.x = x;
        nextNodeID.y = y;

        bool Odd = ((y % 2) == 0) ? true : false;

        //次のノード番号の計算
        switch (toDir)
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
                if (Odd)
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

        if (nextNodeID.x < 0 || nextNodeID.x > AdjustRow(nextNodeID.y) - 1 || nextNodeID.y < 0 || nextNodeID.y > col - 1)
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
        _eLinkDir toDir = ConvertSlideDirToLinkDir(toSlideDir);

        return GetDirNode(x, y, toDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    public Vec2Int GetDirNode(Vec2Int nodeID, _eSlideDir toSlideDir)
    {
        return GetDirNode(nodeID.x, nodeID.y, toSlideDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    // フレームノードを除く
    public Vec2Int GetDirNodeRemoveFrame(int x, int y, _eLinkDir toDir)
    {
        Vec2Int id = GetDirNode(x, y, toDir);

        if (id.x <= 0 || id.x >= AdjustRow(id.y) - 1 || id.y <= 0 || id.y >= col - 1)
        {
            id.x = -1;
            id.y = -1;
        }

        return id;
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    // フレームノードを除く
    public Vec2Int GetDirNodeRemoveFrame(Vec2Int nodeID, _eLinkDir toDir)
    {
        return GetDirNodeRemoveFrame(nodeID.x, nodeID.y, toDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    // フレームノードを除く
    public Vec2Int GetDirNodeRemoveFrame(int x, int y, _eSlideDir toSlideDir)
    {
        _eLinkDir toDir = ConvertSlideDirToLinkDir(toSlideDir);

        return GetDirNodeRemoveFrame(x, y, toDir);
    }

    //位置と方向から、指定ノードに隣り合うノードのrowとcolを返す
    //なければ、(-1,-1)を返す
    // フレームノードを除く
    public Vec2Int GetDirNodeRemoveFrame(Vec2Int nodeID, _eSlideDir toSlideDir)
    {
        return GetDirNodeRemoveFrame(nodeID.x, nodeID.y, toSlideDir);
    }

    //指定した位置のノードのスクリプトをもらう
    public Node GetNodeScript(Vec2Int nodeID)
    {
        return gameNodeScripts[nodeID.y][nodeID.x];
    }
    #endregion

    #region // ノード配置、再配置
    public delegate void Replace();             //回転再配置用のデリゲート

    //ノードの配置 割合は指定できるが完全ランダムなので再考の余地あり
    void ReplaceNode(Node node,bool force1Exclude)
    {
        //Log.Debug("ReplaceNode : " + node);
        node.CheckFlag = false;
        node.ChainFlag = false;

        float rand;
        rand = Random.Range(0.0f, RatioSum);

        int branchNum =
            (rand <= fieldLevel.Ratio_Cap) ? 1 :
            (rand <= fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2) ? 2 :
            (rand <= fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3) ? 3 :
            (rand <= fieldLevel.Ratio_Cap + fieldLevel.Ratio_Path2 + fieldLevel.Ratio_Path3 + fieldLevel.Ratio_Path4) ? 4 :
            1;
        if(force1Exclude && branchNum == 1) { branchNum = 2; }
        node.SetNodeType(NodeTemplate.GetTempFromBranchRandom(NodeTemp, branchNum));
    }

    /// <summary>
    ///  完成した枝情報を保持するクラス
    /// </summary>
    public class FinNode
    {
        public Vec2Int ID;
        public int Rot;
        public NodeTemplate Temp;

        FinNode(Node tgt)
        {
            ID = tgt.NodeID;
            Rot = tgt.RotCounter;
            Temp = tgt.Temp;
        }
        public static List<FinNode> Convert(List<Node> Node)
        {
            var Val = new List<FinNode>();
            foreach (var it in Node)
            {
                Val.Add(new FinNode(it));
            }
            return Val;
        }

        public override string ToString()
        {
            return ID.x.ToString() + "," + ID.y.ToString() + "," + Rot.ToString() + "," + Temp.ID.ToString();
        }
    }
    private List<List<FinNode>> FinishNodeList = new List<List<FinNode>>();

    /// <summary>
    /// 繋がっているノードを切ります。
    /// </summary>
    /// <param name="List"></param>
    void UnlinkNodeTree(List<Node> List)
    {
        int mut = RandomEx.RangeforInt(0, List.Count);
        if (List.Count == 1)
        {
            gameNodeScripts[List[0].NodeID.y][List[0].NodeID.x].SetNodeType(NodeTemp[1], 0);
            //gameNodeScripts[List[0].NodeID.y][List[0].NodeID.x].ForceRotateWithBit();
        }
        else {
            gameNodeScripts[List[mut].NodeID.y][List[mut].NodeID.x].RotationNode(true, true);
        }
    }

    //完成した枝に使用しているノードを再配置する
    void ReplaceNodeTree(List<Node> List)
    {
        //Log.Debug("ReplaceNodeTree : " + List);
        if (List.Count > 2)
        {
            FinishNodeList.Add(FinNode.Convert(List));
//#if UNITY_EDITOR
//            if(List.Count > 3)
//            {
//                NodeSaver.Write(FinNode.Convert(List));
//            }
//#endif
        }

        NodeCountInfo nodeCount = new NodeCountInfo();

        foreach (Node obj in List)
        {
            switch (obj.GetLinkNum())
            {
                case 1:
                    nodeCount.cap++;
                    break;
                case 2:
                    nodeCount.path2++;
                    break;
                case 3:
                    nodeCount.path3++;
                    break;
                case 4:
                    nodeCount.path4++;
                    break;
            }
        }

        int curScore = 0;
        nodeCount.nodes = List.Count;
        curScore = scoreScript.PlusScore(nodeCount);
        timeScript.PlusTime(nodeCount);
        feverScript.Gain(nodeCount);
        
        // 完成時演出
        gameEffect.TreeCompleteEffect(List, curScore);
        audioSources[(int)_eAudioNumber.TREE].Play();

        //ノードを再配置
        foreach (Node obj in List)
        {
            ReplaceNode(obj, obj.NodeID.y < 2);
        }
        foreach (Node obj in List)
        {
            obj.UpdateNegibor();
        }
        CheckLink(true, true);
    }

    public void ReplaceNodeAll()
    {
        //Log.Debug("ReplaceNodeAll");
        foreach (var xList in gameNodeScripts)
        {
            foreach (var it in xList)
            {
                ReplaceNode(it, it.NodeID.y < 2);
            }
        }
        CheckLink(true, true);
    }

    public void ReplaceNodeFever()
    {
        //Log.Debug("ReplaceNodeFever");
        //foreach (var xList in gameNodeScripts)
        //{
        //    foreach (var it in xList)
        //    {
        //        it.SetNodeType(NodeTemplate.GetTempFromBranchRandom(NodeTemp, 1), 0);
        //    }
        //}

        if (FinishNodeList != null && FinishNodeList.Count > 0)
        {
            #region // 過去記憶版
            var UseTree = FinishNodeList[RandomEx.RangeforInt(0, FinishNodeList.Count)];
            foreach (var it in UseTree)
            {
                gameNodeScripts[it.ID.y][it.ID.x].SetNodeType(it.Temp, it.Rot);

            };
            int mut = RandomEx.RangeforInt(0, UseTree.Count);
            gameNodeScripts[UseTree[mut].ID.y][UseTree[mut].ID.x].RotationNode(true, true);
            if (FinishNodeList.Count > 15)
            {
                FinishNodeList.RemoveRange(0, 10);
            }
            #endregion
        }
        else {
            #region // 自己生成版
            string str = "ReplaceNodeFeverLog\n";
            int targetNum = RandomEx.RangeforInt(3, 9);
            str += "Target : " + targetNum;
            
            List<Node> lstTree = new List<Node>();
            int x = RandomEx.RangeforInt(2, 5);
            int y = 1;
            _eLinkDir Dir = _eLinkDir.LD;
            int RotationNum;
            while (lstTree.Count < targetNum)
            {
                str += "\nCheck" + gameNodeScripts[y][x].ToString() + "\n";

                gameNodeScripts[y][x].SetNodeType(NodeTemplate.GetTempFromBranchRandom(NodeTemp, 2), 0);
                str += "SetNode : " + gameNodeScripts[y][x].bitLink.ToStringEx() + " / " + gameNodeScripts[y][x].Temp.ToString() + "\n";

                int CheckLinkNum = 0;
                bool Decide = false;
                ///<summary>
                /// 不正な回転 or 回転できなかった処理が存在  
                ///</summary>  
                RotationNum = 0;
                while (!Decide && gameNodeScripts[y][x].bitLink.Count > CheckLinkNum)
                {
                    while (!gameNodeScripts[y][x].bitLink[(int)Dir])
                    {
                        gameNodeScripts[y][x].RotationNode(true);
                        RotationNum++;
                    }
                    str += "Rot : " + RotationNum + " / 　" + gameNodeScripts[y][x].bitLink.ToStringEx() + "\n";

                    Vec2Int nextNode = new Vec2Int(-1, -1);
                    _eLinkDir NextDir = Dir;
                    for (int n = 0; n < 6; n++)
                    {
                        if (n == (int)Dir) { continue; }
                        if (gameNodeScripts[y][x].bitLink[n])
                        {
                            nextNode = GetDirNode(x, y, (_eLinkDir)n);
                            if (nextNode.x == -1 || gameNodeScripts[nextNode.y][nextNode.x].IsOutPuzzle)
                            {
                                nextNode.x = x;
                                nextNode.y = y;
                                CheckLinkNum++;
                                if (n == 5)
                                {
                                    str += "Failed";
                                    break;
                                }
                            }
                            str += "Find" + ((_eLinkDir)n).ToString();
                            Decide = true;
                            NextDir = ReverseDirection((_eLinkDir)n);
                        }
                    }
                    gameNodeScripts[y][x].SetNodeType(gameNodeScripts[y][x].Temp, RotationNum);
                    x = nextNode.x;
                    y = nextNode.y;
                    Dir = NextDir;
                    lstTree.Add(gameNodeScripts[y][x]);
                }
            }
            // 枝に蓋をする
            gameNodeScripts[y][x].SetNodeType(NodeTemplate.GetTempFromBranchRandom(NodeTemp, 1), 0);
            RotationNum = 0;
            while (!gameNodeScripts[y][x].bitLink[(int)Dir])
            {
                gameNodeScripts[y][x].RotationNode(true);
                RotationNum++;
            }
            gameNodeScripts[y][x].RotationNode(true, true);
            gameNodeScripts[y][x].SetNodeType(gameNodeScripts[y][x].Temp, RotationNum == 0 ? 5 : RotationNum - 1);
            lstTree.Add(gameNodeScripts[y][x]);
            Debug.Log(str);
            #endregion
        }

        CheckLink();
    }

    //ノード全変更時の演出
    public void RotateAllNode(float movedAngle, Ease easeType)
    {
        //Log.Debug("RotateAllNode : " + movedAngle + "/" + easeType);
        foreach (var xList in gameNodeScripts)
        {
            foreach (var it in xList)
            {
                it.FlipNode(repRotateTime, easeType);
            }
        }
    }
    #endregion

    public void SetSlideAll(bool slide)
    {
        //Log.Debug("SetSlideAll : " + slide);

        isSlide = slide;
    }

    //全ノードがくるっと回転して状態遷移するやつ 再配置関数を引数に
    public IEnumerator ReplaceRotate(Replace repMethod)
    {
        //Log.Debug("ReplaceRotate : " + repMethod);
        isSlide = true;     //この回転中に操作できないように
        //全ノードを90°回転tween
        RotateAllNode(90.0f, Ease.InSine);
        yield return new WaitForSeconds(repRotateTime / 2.0f);
        //全ノードを-180°回転
        foreach (var xList in gameNodeScripts)
        {
            foreach (var it in xList)
            {
                it.transform.Rotate(0, -180, 0);
            }
        }

        //置き換え処理
        repMethod();

        //若干遅らせることで再生成時に完成した枝ができてた時の処理を先にさせる
        yield return new WaitForSeconds(0.01f);

        ForceRotateZ();

        //全ノードを90°回転
        RotateAllNode(0.0f, Ease.OutSine);
        yield return new WaitForSeconds(repRotateTime / 2.0f);
        isSlide = false;    //終わったら操作できるように
        CheckLink();


    }

    //操作終了時の処理をここで
    public void TouchEnd()
    {
        //Log.Debug("TouchEnd");
        //状況に応じて別の処理をする
        switch (feverScript.feverState)
        {
            case _eFeverState.NORMAL:
                break;

            case _eFeverState.FEVER:
                break;
        }
    }

    #region enum関連
    // _eSlideDir を _eLinkDir に変換する
    _eLinkDir ConvertSlideDirToLinkDir(_eSlideDir dir)
    {
        _eLinkDir convert = _eLinkDir.NONE;

        switch (dir)
        {
            case _eSlideDir.LEFT:
                convert = _eLinkDir.L;
                break;
            case _eSlideDir.RIGHT:
                convert = _eLinkDir.R;
                break;
            case _eSlideDir.LEFTUP:
                convert = _eLinkDir.LU;
                break;
            case _eSlideDir.LEFTDOWN:
                convert = _eLinkDir.LD;
                break;
            case _eSlideDir.RIGHTUP:
                convert = _eLinkDir.RU;
                break;
            case _eSlideDir.RIGHTDOWN:
                convert = _eLinkDir.RD;
                break;
        }

        return convert;
    }

    // _eLinkDir を _eSlideDir に変換する
    _eSlideDir ConvertLinkDirToSlideDir(_eLinkDir dir)
    {
        _eSlideDir convert = _eSlideDir.NONE;

        switch (dir)
        {
            case _eLinkDir.L:
                convert = _eSlideDir.LEFT;
                break;
            case _eLinkDir.R:
                convert = _eSlideDir.RIGHT;
                break;
            case _eLinkDir.LU:
                convert = _eSlideDir.LEFTUP;
                break;
            case _eLinkDir.LD:
                convert = _eSlideDir.LEFTDOWN;
                break;
            case _eLinkDir.RU:
                convert = _eSlideDir.RIGHTUP;
                break;
            case _eLinkDir.RD:
                convert = _eSlideDir.RIGHTDOWN;
                break;
        }

        return convert;
    }

    // スライド方向の逆方向を求める
    _eSlideDir ReverseDirection(_eSlideDir dir)
    {
        _eSlideDir reverse = _eSlideDir.NONE;

        switch (dir)
        {
            case _eSlideDir.LEFT:
                reverse = _eSlideDir.RIGHT;
                break;
            case _eSlideDir.RIGHT:
                reverse = _eSlideDir.LEFT;
                break;
            case _eSlideDir.LEFTUP:
                reverse = _eSlideDir.RIGHTDOWN;
                break;
            case _eSlideDir.LEFTDOWN:
                reverse = _eSlideDir.RIGHTUP;
                break;
            case _eSlideDir.RIGHTUP:
                reverse = _eSlideDir.LEFTDOWN;
                break;
            case _eSlideDir.RIGHTDOWN:
                reverse = _eSlideDir.LEFTUP;
                break;
        }

        return reverse;
    }

    // リンク方向の逆方向を求める
    _eLinkDir ReverseDirection(_eLinkDir dir)
    {
        _eLinkDir reverse = _eLinkDir.NONE;

        switch (dir)
        {
            case _eLinkDir.L:
                reverse = _eLinkDir.R;
                break;
            case _eLinkDir.R:
                reverse = _eLinkDir.L;
                break;
            case _eLinkDir.LU:
                reverse = _eLinkDir.RD;
                break;
            case _eLinkDir.LD:
                reverse = _eLinkDir.RU;
                break;
            case _eLinkDir.RU:
                reverse = _eLinkDir.LD;
                break;
            case _eLinkDir.RD:
                reverse = _eLinkDir.LU;
                break;
        }

        return reverse;
    }
    #endregion[

    // スライド開始時の easing をストップする
    void StopSlideStartEasing()
    {
        //Log.Debug("StopSlideStartEasing");
        if (gameNodeScripts[tapNodeID.y][tapNodeID.x].IsSlideStart)
        {
            Vector2 nodePos2D = new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);

            // タップ位置とノードとの距離が閾値を下回っていたらストップする
            if (Vector2.Distance(nodePos2D, AdjustNodeLinePosition(slideDir)) < NODE_EASE_STOP_THRESHOLD)
            {
                Vec2Int nextID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
                _eSlideDir reverseDir = ReverseDirection(slideDir);

                while (nextID.x > -1)
                {
                    gameNodeScripts[nextID.y][nextID.x].IsSlideStart = false;
                    nextID = GetDirNode(nextID, reverseDir);
                }
            }
        }
    }
    //ビットに合わせてｚ軸回転を強制する
    void ForceRotateZ()
    {
        foreach (var xList in gameNodeScripts)
        {
            foreach (var it in xList)
            {
                it.ForceRotateWithBit();
            }
        }
    }

    void ChangeLitColor(Color col)
    {
        gameNodePrefabs[0][0].GetComponent<SpriteRenderer>().sharedMaterial.SetColor("Tint",col);
    }

    public void DoKillAllNode()
    {
        foreach (var xList in gameNodePrefabs)
        {
            foreach (var it in xList)
            {
                it.transform.DOKill();
            }
        }
        foreach (var xList in gameNodeScripts)
        {
            foreach (var it in xList)
            {
                it.ForceFragClear();
            }
        }
    } 
}

