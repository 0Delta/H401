using UnityEngine;
using System.Collections;
using UniRx;

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

    [SerializeField] private int row = 0;       // 横配置数
    [SerializeField] private int col = 0;       // 縦配置数
    [SerializeField] private GameObject nodePrefab = null;       // パネルのプレハブ
    [SerializeField] private float widthMargin  = 0.0f;  // パネル位置の左右間隔の調整値
    [SerializeField] private float heightMargin = 0.0f;  // パネル位置の上下間隔の調整値

    private GameObject[,]   nodePrefabs;     // パネルのプレハブリスト
    private Node[,]         nodeScripts;     // パネルのnodeスクリプトリスト

    private Vector2 nodeSize = Vector2.zero;    // 描画するパネルのサイズ

    private bool    isDrag = false;                     // マウスドラッグフラグ
    private Vector2 beforeTapNodeID = Vector2.zero;     // 移動させたいノードのID
    private Vector2 afterTapNodeID  = Vector2.zero;     // 移動させられるノードのID(移動方向を判定するため)

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

    public Vector2 BeforeTapNodeID {
        set { this.beforeTapNodeID = value; }
        get { return this.beforeTapNodeID; }
    }

    public Vector2 AfterTapNodeID {
        set { this.afterTapNodeID = value; }
        get { return this.afterTapNodeID; }
    }

    void Awake() {
        nodePrefabs = new GameObject[row, col];
        nodeScripts = new Node[row, col];
    }

	// Use this for initialization
	void Start () {
        // ----- パネル準備
        // 描画するパネルの大きさを取得
        Vector3 pos = transform.position;
        nodeSize.x = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.width * nodePrefab.transform.localScale.x * ADJUST_PIXELS_PER_UNIT;
        nodeSize.y = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.height * nodePrefab.transform.localScale.y * ADJUST_PIXELS_PER_UNIT;
        nodeSize.x -= widthMargin * ADJUST_PIXELS_PER_UNIT;
        nodeSize.y -= heightMargin * ADJUST_PIXELS_PER_UNIT;

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
            }
        }

        // パネルに情報を登録
        nodeScripts[0,0].SetNodeController(this);

        // ----- ドラッグ処理準備
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButton(0))
            .Subscribe(_ => {
                isDrag = true;

                //print("clicling");
            })
            .AddTo(this.gameObject);
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => {
                isDrag = false;

                print("drag end");
            })
            .AddTo(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    
    void SlantMove(_eSlideDir slideDir, Vector2 dir) {
        // スライド対象となるノードの準備
        Vector2 upNodeID   = afterTapNodeID;    // 上方向への探索ノードID
        Vector2 downNodeID = beforeTapNodeID;   // 下方向への探索ノードID

        switch (slideDir) {
            case _eSlideDir.LEFT:
            case _eSlideDir.RIGHT:
                for(int i = 0; i < row; ++i) {
                    nodeScripts[i,(int)beforeTapNodeID.y].SlideNode(slideDir, dir);
                }
                break;

            case _eSlideDir.LEFTUP:
            case _eSlideDir.RIGHTDOWN:
                // 移動させられるノードより、上に位置するノードを移動
                while((int)upNodeID.x >= 0 && (int)upNodeID.y < col) {
                    nodeScripts[(int)upNodeID.x,(int)upNodeID.y].SlideNode(slideDir, dir);

                    if((int)upNodeID.y % 2 == 0)
                        --upNodeID.x;
                    ++upNodeID.y;
                }
                // 移動させられるノードより、下に位置するノードを移動
                while((int)downNodeID.x < row && (int)downNodeID.y >= 0) {
                    nodeScripts[(int)downNodeID.x,(int)downNodeID.y].SlideNode(slideDir, dir);

                    if((int)downNodeID.y % 2 != 0)
                        ++downNodeID.x;
                    --downNodeID.y;
                }
                break;
                
            case _eSlideDir.RIGHTUP:
            case _eSlideDir.LEFTDOWN:
                // 移動させられるノードより、上に位置するノードを移動
                while((int)upNodeID.x < row && (int)upNodeID.y < col) {
                    nodeScripts[(int)upNodeID.x,(int)upNodeID.y].SlideNode(slideDir, dir);

                    if((int)upNodeID.y % 2 != 0)
                        ++upNodeID.x;
                    ++upNodeID.y;
                }
                // 移動させられるノードより、下に位置するノードを移動
                while((int)downNodeID.x >= 0 && (int)downNodeID.y >= 0) {
                    nodeScripts[(int)downNodeID.x,(int)downNodeID.y].SlideNode(slideDir, dir);

                    if((int)downNodeID.y % 2 == 0)
                        --downNodeID.x;
                    --downNodeID.y;
                }
                break;

            default:
                break;
        }
    }
    
    public void SlideNodes() {
        int subRowID = (int)afterTapNodeID.x - (int)beforeTapNodeID.x;   // ノードIDの差分(横方向)
        int subColID = (int)afterTapNodeID.y - (int)beforeTapNodeID.y;   // ノードIDの差分(縦方向)
        Vector2 dir   = (Vector2)nodePrefabs[(int)afterTapNodeID.x, (int)afterTapNodeID.y].transform.position       // スライド方向ベクトルを算出
            - (Vector2)nodePrefabs[(int)beforeTapNodeID.x, (int)beforeTapNodeID.y].transform.position;
        
        // 左にスライド
        if(subRowID == -1 && subColID == 0) {
            SlantMove(_eSlideDir.LEFT, dir);
        }
        // 右にスライド
        if(subRowID == 1 && subColID == 0) {
            SlantMove(_eSlideDir.RIGHT, dir);
        }
        // 左上にスライド
        if(subColID == 1 && dir.x < 0.0f && dir.y > 0.0f) {
            SlantMove(_eSlideDir.LEFTUP, dir);
        }
        // 左下にスライド
        if(subColID == -1 && dir.x < 0.0f && dir.y < 0.0f) {
            SlantMove(_eSlideDir.LEFTDOWN, dir);
        }
        // 右上にスライド
        if(subColID == 1 && dir.x > 0.0f && dir.y > 0.0f) {
            SlantMove(_eSlideDir.RIGHTUP, dir);
        }
        // 右下にスライド
        if(subColID == -1 && dir.x > 0.0f && dir.y < 0.0f) {
            SlantMove(_eSlideDir.RIGHTDOWN, dir);
        }
    }

    //public void LoopBackNode(Vector2 id, _eSlideDir slideDir) {
    //    switch(slideDir) {
    //        case _eSlideDir.LEFT:
    //            GameObject outNode = nodePrefabs[(int)id.x,(int)id.y];
    //            Node outNodeScript = nodeScripts[(int)id.x,(int)id.y];
            
    //            // 画面外に出たパネルの位置を調整
    //            Vector3 pos = nodePrefabs[row - 1,(int)id.y].transform.position;
    //            pos.x += nodeSpriteSize.x;
    //            outNode.transform.position = pos;

    //            // 画面内のパネルデータをソート
    //            for(int i = 1; i < row; ++i) {
    //                nodePrefabs[i - 1, (int)id.y] = nodePrefabs[i,(int)id.y];
    //                nodeScripts[i - 1, (int)id.y] = nodeScripts[i,(int)id.y];
    //            }
    //            nodePrefabs[row - 1,(int)id.y] = outNode;
    //            nodeScripts[row - 1,(int)id.y] = outNodeScript;

    //            break;
    //    }
    //}
}
