//using UnityEngine;
//using System.Collections;

//public class SlidingManager : MonoBehaviour {
//    private static readonly CustomDebugLog.CDebugLog Log = new CustomDebugLog.CDebugLog("NodeController");


//    private Vec2Int     tapNodeID       = Vec2Int.zero;         // タップしているノードのID
//    private _eSlideDir  slideDir        = _eSlideDir.NONE;      // スライド中の方向
//    private Vector2     moveNodeInitPos = Vector2.zero;         // 移動中ノードの移動開始位置
//    private Vector2     moveNodeDist    = Vector2.zero;         // 移動中ノードの基本移動量(移動方向ベクトル)
//    private Vector2     moveNodeDistAbs = Vector2.zero;         // 移動中ノードの基本移動量の絶対値
//    private Vector2     tapPosMoveNodePosDist = Vector2.zero;   // タップしているノードの位置と、タップしている位置との距離

//    private Vector2 startTapPos = Vector2.zero;     // タップした瞬間の座標
//    private Vector2 tapPos      = Vector2.zero;     // タップ中の座標
//    private Vector2 prevTapPos  = Vector2.zero;     // 前フレームのタップ座標

//    private Vector2 slideLeftUpPerNorm   = Vector2.zero;     // 左上ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)
//    private Vector2 slideLeftDownPerNorm = Vector2.zero;     // 左下ベクトルの垂線の単位ベクトル(Z軸を90度回転済み)

//    private Vec2Int slidingLimitNodeID        = Vec2Int.zero;     // スライド方向の端ノードのID
//    private Vec2Int slidingReverseLimitNodeID = Vec2Int.zero;     // スライド方向の逆端ノードのID


//    public _eSlideDir SlideDir
//    {
//        get { return slideDir; }
//    }

//    // Use this for initialization
//    void Start () {
//        // スライドベクトルの垂線を算出
//        Vector3 leftUp = gameNodePrefabs[0][1].transform.position - gameNodePrefabs[0][0].transform.position;
//        Vector3 leftDown = gameNodePrefabs[1][1].transform.position - gameNodePrefabs[0][0].transform.position;
//        Matrix4x4 mtx = Matrix4x4.identity;
//        mtx.SetTRS(new Vector3(0.0f, 0.0f, 0.0f), Quaternion.Euler(0.0f, 0.0f, 90.0f), new Vector3(1.0f, 1.0f, 1.0f));
//        leftUp = mtx.MultiplyVector(leftUp).normalized;
//        leftDown = mtx.MultiplyVector(leftDown).normalized;
//        slideLeftUpPerNorm = new Vector2(leftUp.x, leftUp.y);
//        slideLeftDownPerNorm = new Vector2(leftDown.x, leftDown.y);
//    }

//    // Update is called once per frame
//    void Update () {
	
//	}

//    //移動したいノードを確定
//    //ドラッグを算出し移動したい方向列を確定
//    //ドラッグされている間、列ごと移動、
//    //タップ点からスワイプ地点まで座標の差分を算出し
//    //列のすべてのノードをその位置へ移動させる
//    //離すと一番近いノード確定位置まで調整

//    public void StartSlideNodes(Vec2Int nextNodeID, _eSlideDir newSlideDir) {
//        Log.Debug("StartSlideNodes : " + nextNodeID + " / " + newSlideDir);
//        moveNodeDist = new Vector2(gameNodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.x, gameNodePrefabs[nextNodeID.y][nextNodeID.x].transform.position.y)
//                     - new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);   // スライド方向ベクトル兼移動量を算出
//        moveNodeInitPos = gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position;      // ノードの移動開始位置を保存

//        // スライド方向を設定
//        slideDir = newSlideDir;

//        // 絶対値を算出
//        moveNodeDistAbs.x = Mathf.Abs(moveNodeDist.x);
//        moveNodeDistAbs.y = Mathf.Abs(moveNodeDist.y);

//        // スライド開始準備
//        Vec2Int nextID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
//        _eSlideDir reverseDir = ReverseDirection(slideDir);
//        while(nextID.x > -1) {
//            gameNodeScripts[nextID.y][nextID.x].StartSlide();
//            nextID = GetDirNode(nextID, reverseDir);
//        }

//        // スライド方向の端のノードIDを算出
//        slidingLimitNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
//        slidingReverseLimitNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(ReverseDirection(slideDir)));

//        // タップしているノードの位置と、タップしている位置との距離を算出
//        tapPosMoveNodePosDist = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, startTapPos - moveNodeInitPos);
//    }

//    // ゲームの画面外にはみ出したノードを逆側に移動する
//    void LoopBackNode() {
//        Log.Debug("LoopBackNode");
//        if(gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen) {
//            gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = false;

//            SortOutNode(slideDir, slidingLimitNodeID);

//            Vec2Int copyInNodeID = SearchLimitNodeRemoveFrame(tapNodeID, ConvertSlideDirToLinkDir(SlideDir));
//            Vec2Int copyOutNodeID = SearchLimitNodeRemoveFrame(tapNodeID, ConvertSlideDirToLinkDir(ReverseDirection(slideDir)));
//            copyOutNodeID = GetDirNode(copyOutNodeID, ReverseDirection(slideDir));
//            gameNodeScripts[copyOutNodeID.y][copyOutNodeID.x].CopyParameter(gameNodeScripts[copyInNodeID.y][copyInNodeID.x]);
//        }
//    }

//    // 移動を終了するノードの位置を調整する
//    void AdjustNodeStop() {
//        Log.Debug("AdjustNodeStop");
//        Vec2Int nearNodeID = SearchNearNode(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position);
//        Vec2Int nextNodeID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
//        _eSlideDir reverseDir = ReverseDirection(slideDir);
//        Vector2 pos = new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);
//        Vector2 standardPos = new Vector2(nodePlacePosList[nearNodeID.y][nearNodeID.x].x, nodePlacePosList[nearNodeID.y][nearNodeID.x].y);

//        // スライド方向を更新
//        if(pos.x != standardPos.x || pos.y != standardPos.y) {
//            _eSlideDir checkDir = CheckSlideDir(pos, standardPos);
//            if(slideDir != checkDir) {
//                slideDir = checkDir;

//                Vec2Int tmp = slidingLimitNodeID;
//                slidingLimitNodeID = slidingReverseLimitNodeID;
//                slidingReverseLimitNodeID = tmp;
//            }
//        }

//        // スライド方向のノードに、スライド終了を通知
//        while(nextNodeID.x > -1) {
//            gameNodeScripts[nextNodeID.y][nextNodeID.x].EndSlide();
//            nextNodeID = GetDirNode(nextNodeID, reverseDir);
//        }

//        // 回り込み処理
//        CheckSlideOutLimitNode();
//        LoopBackNode();

//        // 移動処理
//        switch(slideDir) {
//            case _eSlideDir.LEFT:
//            case _eSlideDir.RIGHT:
//                pos = standardPos;

//                // タップしているノードを移動
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

//                // タップしているノードより左側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
//                }
//                // タップしているノードより右側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.R);
//                }

//                break;

//            case _eSlideDir.LEFTUP:
//            case _eSlideDir.RIGHTDOWN:
//                // タップしているノードを移動
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

//                // タップしているノードより左上側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
//                }
//                // タップしているノードより右下側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
//                }
//                break;

//            case _eSlideDir.RIGHTUP:
//            case _eSlideDir.LEFTDOWN:
//                // タップしているノードを移動
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

//                // タップしているノードより右上側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
//                }
//                // タップしているノードより左下側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
//                }
//                break;

//            default:
//                break;
//        }
//    }

//    // 任意のノード情報をコピーする
//    void CopyNodeInfo(int x, int y, GameObject prefab, Node script) {
//        Log.Debug("CopyNodeInfo : " + x + "/" + y + "/" + script);
//        gameNodePrefabs[y][x] = prefab;
//        gameNodeScripts[y][x] = script;
//        gameNodeScripts[y][x].RegistNodeID(x, y);
//    }

//    // はみ出たノードを逆側に移動し、ノード情報をソートする
//    void SortOutNode(_eSlideDir dir, Vec2Int outNodeID) {
//        Log.Debug("SortOutNode : " + dir + "/" + outNodeID);
//        GameObject outNode = gameNodePrefabs[outNodeID.y][outNodeID.x];
//        Node outNodeScript = gameNodeScripts[outNodeID.y][outNodeID.x];
//        _eSlideDir reverseDir = ReverseDirection(dir);
//        Vector3 pos = Vector3.zero;

//        // ノード入れ替え処理(スライド方向に置換していく)
//        Vec2Int limitNodeID = outNodeID;
//        Vec2Int prevSearchID = outNodeID;
//        while(GetDirNode(limitNodeID, reverseDir).x > -1) {
//            prevSearchID = limitNodeID;
//            limitNodeID = GetDirNode(limitNodeID, reverseDir);
//            CopyNodeInfo(prevSearchID.x, prevSearchID.y, gameNodePrefabs[limitNodeID.y][limitNodeID.x], gameNodeScripts[limitNodeID.y][limitNodeID.x]);
//        }
//        CopyNodeInfo(limitNodeID.x, limitNodeID.y, outNode, outNodeScript);

//        // 位置を調整
//        prevSearchID = GetDirNode(limitNodeID, dir);
//        pos = gameNodePrefabs[prevSearchID.y][prevSearchID.x].transform.position;
//        switch(dir) {
//            case _eSlideDir.LEFT:
//                pos = gameNodePrefabs[tapNodeID.y][AdjustRow(tapNodeID.y) - 2].transform.position;
//                pos.x += moveNodeDistAbs.x;
//                break;

//            case _eSlideDir.RIGHT:
//                pos = gameNodePrefabs[tapNodeID.y][1].transform.position;
//                pos.x -= moveNodeDistAbs.x;
//                break;

//            case _eSlideDir.LEFTUP:
//                pos.x += moveNodeDistAbs.x;
//                pos.y -= moveNodeDistAbs.y;
//                break;

//            case _eSlideDir.LEFTDOWN:
//                pos.x += moveNodeDistAbs.x;
//                pos.y += moveNodeDistAbs.y;
//                break;

//            case _eSlideDir.RIGHTUP:
//                pos.x -= moveNodeDistAbs.x;
//                pos.y -= moveNodeDistAbs.y;
//                break;

//            case _eSlideDir.RIGHTDOWN:
//                pos.x -= moveNodeDistAbs.x;
//                pos.y += moveNodeDistAbs.y;
//                break;
//        }
//        gameNodeScripts[limitNodeID.y][limitNodeID.x].StopTween();
//        gameNodePrefabs[limitNodeID.y][limitNodeID.x].transform.position = pos;

//        // 選択中のノードIDを更新
//        tapNodeID = GetDirNode(tapNodeID, dir);
//    }

//    // タップノードを中心に、スライド方向のノードの位置を設定する
//    void AdjustSlideNodePosition() {
//        Log.Debug("AdjustSlideNodePosition");
//        // スライド対象となるノードの準備
//        Vector2 pos = tapPos;      // 移動位置
//        Vector2 standardPos = tapPos;
//        Vec2Int nextNodeID = Vec2Int.zero;     // 検索用ノードIDテンポラリ

//        switch(slideDir) {
//            case _eSlideDir.LEFT:
//            case _eSlideDir.RIGHT:
//                // タップしているノードを移動
//                pos = AdjustNodeLinePosition(slideDir);
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, pos);

//                // タップしているノードより左側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.L);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.L);
//                }
//                // タップしているノードより右側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.R);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.R);
//                }

//                break;

//            case _eSlideDir.LEFTUP:
//            case _eSlideDir.RIGHTDOWN:
//                // タップしているノードを移動
//                standardPos = AdjustNodeLinePosition(slideDir);
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

//                // タップしているノードより左上側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LU);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LU);
//                }
//                // タップしているノードより右下側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RD);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RD);
//                }
//                break;

//            case _eSlideDir.RIGHTUP:
//            case _eSlideDir.LEFTDOWN:
//                // タップしているノードを移動
//                standardPos = AdjustNodeLinePosition(slideDir);
//                gameNodeScripts[tapNodeID.y][tapNodeID.x].SlideNode(slideDir, standardPos);

//                // タップしているノードより右上側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.RU);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x + moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y + moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.RU);
//                }
//                // タップしているノードより左下側のノードを移動
//                nextNodeID = GetDirNode(tapNodeID, _eLinkDir.LD);
//                for(int i = 1; nextNodeID.x > -1; ++i) {
//                    pos.x = standardPos.x - moveNodeDistAbs.x * i;
//                    pos.y = standardPos.y - moveNodeDistAbs.y * i;
//                    gameNodeScripts[nextNodeID.y][nextNodeID.x].SlideNode(slideDir, pos);
//                    nextNodeID = GetDirNode(nextNodeID, _eLinkDir.LD);
//                }
//                break;

//            default:
//                break;
//        }
//    }

//    // タップ位置から、タップしているノードの、移動ライン上の座標を算出する
//    Vector2 AdjustNodeLinePosition(_eSlideDir dir) {
//        Log.Debug("AdjustNodeLinePosition : " + dir);
//        Vector2 adjustPos = tapPos;
//        Vector2 slideDist = tapPos - startTapPos;     // スライド量
//        Vector2 moveDist = moveNodeDist.normalized * Vector2.Dot(moveNodeDist.normalized, slideDist);      // 斜め移動量

//        switch(slideDir) {
//            case _eSlideDir.LEFT:
//            case _eSlideDir.RIGHT:
//                // タップしているノードの位置を調整
//                adjustPos = AdjustGameScreen(tapPos);
//                adjustPos.y = gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y;
//                break;

//            case _eSlideDir.LEFTUP:
//            case _eSlideDir.RIGHTDOWN:
//                // タップしているノードの位置を調整
//                Vec2Int lu = SearchLimitNode(tapNodeID, _eLinkDir.LU);
//                Vec2Int rd = SearchLimitNode(tapNodeID, _eLinkDir.RD);
//                adjustPos = moveNodeInitPos + moveDist + tapPosMoveNodePosDist;
//                if(adjustPos.x < nodePlacePosList[lu.y][lu.x].x)
//                    adjustPos.x = nodePlacePosList[lu.y][lu.x].x;
//                if(adjustPos.x > nodePlacePosList[rd.y][rd.x].x)
//                    adjustPos.x = nodePlacePosList[rd.y][rd.x].x;
//                if(adjustPos.y > nodePlacePosList[lu.y][lu.x].y)
//                    adjustPos.y = nodePlacePosList[lu.y][lu.x].y;
//                if(adjustPos.y < nodePlacePosList[rd.y][rd.x].y)
//                    adjustPos.y = nodePlacePosList[rd.y][rd.x].y;
//                break;

//            case _eSlideDir.RIGHTUP:
//            case _eSlideDir.LEFTDOWN:
//                // タップしているノードの位置を調整
//                Vec2Int ru = SearchLimitNode(tapNodeID, _eLinkDir.RU);
//                Vec2Int ld = SearchLimitNode(tapNodeID, _eLinkDir.LD);
//                adjustPos = moveNodeInitPos + moveDist + tapPosMoveNodePosDist;
//                if(adjustPos.x > nodePlacePosList[ru.y][ru.x].x)
//                    adjustPos.x = nodePlacePosList[ru.y][ru.x].x;
//                if(adjustPos.x < nodePlacePosList[ld.y][ld.x].x)
//                    adjustPos.x = nodePlacePosList[ld.y][ld.x].x;
//                if(adjustPos.y > nodePlacePosList[ru.y][ru.x].y)
//                    adjustPos.y = nodePlacePosList[ru.y][ru.x].y;
//                if(adjustPos.y < nodePlacePosList[ld.y][ld.x].y)
//                    adjustPos.y = nodePlacePosList[ld.y][ld.x].y;
//                break;

//            default:
//                break;
//        }

//        return adjustPos;
//    }

//    // スライドしている列が画面外にはみ出ているかチェックする(フレームノードより外側にいるかどうか)
//    void CheckSlideOutLimitNode() {
//        Log.Debug("CheckSlideOutLimitNode");
//        switch(slideDir) {
//            case _eSlideDir.LEFT:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            case _eSlideDir.RIGHT:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            case _eSlideDir.LEFTUP:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
//                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            case _eSlideDir.LEFTDOWN:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
//                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            case _eSlideDir.RIGHTUP:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
//                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            case _eSlideDir.RIGHTDOWN:
//                if(gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.x > nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].x &&
//                    gameNodePrefabs[slidingLimitNodeID.y][slidingLimitNodeID.x].transform.position.y < nodePlacePosList[slidingLimitNodeID.y][slidingLimitNodeID.x].y) {
//                    gameNodeScripts[slidingLimitNodeID.y][slidingLimitNodeID.x].IsOutScreen = true;
//                }
//                break;
//            default:
//                break;
//        }
//    }
//    // スライド方向を算出
//    _eSlideDir CheckSlideDir(Vector2 pos, Vector2 toPos) {
//        Log.Debug("CheckSlideDir : " + pos + "/" + toPos);
//        float angle = Mathf.Atan2(toPos.y - pos.y, toPos.x - pos.x);
//        angle *= 180.0f / Mathf.PI;

//        // スライド方向を算出
//        _eSlideDir dir = _eSlideDir.NONE;
//        if(angle < 30.0f && angle >= -30.0f) {          // 右
//            dir = _eSlideDir.RIGHT;
//        } else if(angle < 90.0f && angle >= 30.0f) {    // 右上
//            dir = _eSlideDir.RIGHTUP;
//        } else if(angle < 150.0f && angle >= 90.0f) {   // 左上
//            dir = _eSlideDir.LEFTUP;
//        } else if(angle < -150.0f || angle > 150.0f) {  // 左
//            dir = _eSlideDir.LEFT;
//        } else if(angle < -90.0f && angle >= -150.0f) { // 左下
//            dir = _eSlideDir.LEFTDOWN;
//        } else if(angle < -30.0f && angle >= -90.0f) {  // 右下
//            dir = _eSlideDir.RIGHTDOWN;
//        }

//        return dir;
//    }

//    // ノードのスライド移動処理
//    void SlideNodes() {
//        Log.Debug("SlideNodes");
//        // スライド対象となるノードの準備
//        Vector2 deltaSlideDist = tapPos - prevTapPos;   // 前回フレームからのスライド量
//        float checkDir = 0.0f;                          // スライド方向チェック用

//        switch(slideDir) {
//            case _eSlideDir.LEFT:
//            case _eSlideDir.RIGHT:
//                // スライド方向を再計算
//                if(tapPos.x - prevTapPos.x < 0.0f) {
//                    // スライド方向が前フレームと違ったら更新
//                    if(slideDir != _eSlideDir.LEFT) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.LEFT;
//                    }
//                } else if(tapPos.x - prevTapPos.x > 0.0f) {
//                    if(slideDir != _eSlideDir.RIGHT) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.RIGHT;
//                    }
//                }
//                break;

//            case _eSlideDir.LEFTUP:
//            case _eSlideDir.RIGHTDOWN:
//                // スライド方向を再計算
//                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftUpPerNorm);
//                if(checkDir < 0.0f) {
//                    if(slideDir != _eSlideDir.LEFTUP) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.LEFTUP;
//                    }
//                } else if(checkDir > 0.0f) {
//                    if(slideDir != _eSlideDir.RIGHTDOWN) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.RIGHTDOWN;
//                    }
//                }
//                break;

//            case _eSlideDir.RIGHTUP:
//            case _eSlideDir.LEFTDOWN:
//                // スライド方向を再計算
//                checkDir = AddVectorFunctions.Vec2Cross(deltaSlideDist, slideLeftDownPerNorm);
//                if(checkDir < 0.0f) {
//                    if(slideDir != _eSlideDir.LEFTDOWN) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.LEFTDOWN;
//                    }
//                } else if(checkDir > 0.0f) {
//                    if(slideDir != _eSlideDir.RIGHTUP) {
//                        Vec2Int tmp = slidingLimitNodeID;
//                        slidingLimitNodeID = slidingReverseLimitNodeID;
//                        slidingReverseLimitNodeID = tmp;

//                        slideDir = _eSlideDir.RIGHTUP;
//                    }
//                }
//                break;

//            default:
//                break;
//        }
//        // ノードを移動
//        AdjustSlideNodePosition();

//    }

//    // スライド開始時の easing をストップする
//    void StopSlideStartEasing() {
//        Log.Debug("StopSlideStartEasing");
//        if(gameNodeScripts[tapNodeID.y][tapNodeID.x].isSlideStart) {
//            Vector2 nodePos2D = new Vector2(gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.x, gameNodePrefabs[tapNodeID.y][tapNodeID.x].transform.position.y);

//            // タップ位置とノードとの距離が閾値を下回っていたらストップする
//            if(Vector2.Distance(nodePos2D, AdjustNodeLinePosition(slideDir)) < NODE_EASE_STOP_THRESHOLD) {
//                Vec2Int nextID = SearchLimitNode(tapNodeID, ConvertSlideDirToLinkDir(slideDir));
//                _eSlideDir reverseDir = ReverseDirection(slideDir);

//                while(nextID.x > -1) {
//                    gameNodeScripts[nextID.y][nextID.x].isSlideStart = false;
//                    nextID = GetDirNode(nextID, reverseDir);
//                }
//            }
//        }
//    }
//}
