using UnityEngine;
using System.Collections;

public class TitleNodeController : MonoBehaviour {

    private const float FRAME_POSZ_MARGIN = -1.0f;          // フレームとノードとの距離(Z座標)

    [SerializeField] private int row = 0;       // 横配置数 リストIDが奇数の行は＋１とする
    [SerializeField] private int col = 0;       // 縦配置数
    [SerializeField] private string nodePrefabPath      = null;     // ノードのプレハブのパス
    [SerializeField] private string frameNodeSpritePath = null;     // フレームノードのスプライトのパス
    [SerializeField] private float widthMargin  = 0.0f;  // ノード位置の左右間隔の調整値
    [SerializeField] private float heightMargin = 0.0f;  // ノード位置の上下間隔の調整値

    [SerializeField] private int[]   moveCol;   // 移動するノードの行番号
    [SerializeField] private int[]   moveDir;   // 移動するノードの移動方向(0:左, 1:右)
    [SerializeField] private float[] moveSpd;   // 移動するノードの移動速度
    
    private GameObject nodePrefab       = null;     // ノードのプレハブ
    
    private GameObject[][]  nodePrefabs;        // ノードのプレハブリスト
    private Vector3[][]     nodePlacePosList;   // ノードの配置位置リスト
    private GameObject      frameController;    // フレームコントローラープレハブ
    
    private Sprite[] frameNodeSprite;   // フレームノードのスプライトリスト

	private Vector2 nodeSize = Vector2.zero;    // 描画するノードのサイズ

    void Awake() {
        nodePrefabs  = new GameObject[col][];
        nodePlacePosList = new Vector3[col][];
        for(int i = 0; i < col; ++i) {
            int adjustRow = AdjustRow(i);
            nodePrefabs[i]  = new GameObject[adjustRow];
            nodePlacePosList[i] = new Vector3[adjustRow];
        }
    }

	// Use this for initialization
	void Start () {
        Vector3 pos = transform.position;

		// ----- プレハブデータを Resources から取得
        nodePrefab =  Resources.Load<GameObject>(nodePrefabPath);
        
        // ----- スプライトデータを Resources から取得
        frameNodeSprite = Resources.LoadAll<Sprite>(frameNodeSpritePath);

        // 描画するノードの大きさを取得
        SpriteRenderer sr = nodePrefab.GetComponent<SpriteRenderer>();
        nodeSize.x = sr.sprite.bounds.size.x;
        nodeSize.y = sr.sprite.bounds.size.y;
        nodeSize.x -= widthMargin;
        nodeSize.y -= heightMargin;
        
        // フレームを生成
        frameController = new GameObject();
        frameController.transform.parent = transform.parent;
        frameController.name = "FrameController";

        // ノードを生成
        for(int i = 0; i < col; ++i) {
            GameObject  frameObject     = null;

            // ノードの配置位置を調整(Y座標)
            pos.y = transform.position.y + nodeSize.y * -(col * 0.5f - (i + 0.5f));
            for (int j = 0; j < AdjustRow(i); ++j) {
                // ノードの配置位置を調整(X座標)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (j + 0.5f));
                pos.z = transform.position.z;

                // 生成
        	    nodePrefabs[i][j] = (GameObject)Instantiate(nodePrefab, pos, transform.rotation);
                nodePrefabs[i][j].transform.SetParent(transform);
                nodePlacePosList[i][j] = nodePrefabs[i][j].transform.position;
                
                // フレーム生成(上端)
                if(i >= col - 1) {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(nodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                    frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.FRAME];
                }
                // フレーム生成(下端)
                if(i <= 0) {
                    Vector3 framePos = pos;
                    framePos.z = transform.position.z + FRAME_POSZ_MARGIN;
                    frameObject = (GameObject)Instantiate(nodePrefab, framePos, transform.rotation);
                    frameObject.transform.parent = frameController.transform;
                    frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.FRAME];
                }
            }
           
            if(i > 0 && i < col - 1) {
                // フレーム生成(左端)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (0 + 0.5f));
                pos.z = transform.position.z + FRAME_POSZ_MARGIN;
                frameObject = (GameObject)Instantiate(nodePrefab, pos, transform.rotation);
                frameObject.transform.parent = frameController.transform;
                frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.FRAME];
                // フレーム生成(右端)
                pos.x = transform.position.x + nodeSize.x * -(AdjustRow(i) * 0.5f - (AdjustRow(i) - 1 + 0.5f));
                frameObject = (GameObject)Instantiate(nodePrefab, pos, transform.rotation);
                frameObject.transform.parent = frameController.transform;
                frameObject.GetComponent<SpriteRenderer>().sprite = frameNodeSprite[(int)_eFrameNodeSpriteIndex.FRAME];
            }
        }

        // Z座標を調整
	    for(int i = 0; i < moveCol.Length; ++i) {
            int k = moveCol[i];
            
            for(int j = 0; j < AdjustRow(k); ++j) {
                Vector3 tmp = nodePrefabs[k][j].transform.position;
                tmp.z -= 0.5f;
                nodePrefabs[k][j].transform.position = tmp;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	    for(int i = 0; i < moveCol.Length; ++i) {
            int k = moveCol[i];

            for(int j = 0; j < AdjustRow(k); ++j) {
                // 左移動
                if(moveDir[i] <= 0) {
                    Vector3 pos = nodePrefabs[k][j].transform.position;
                    pos.x -= moveSpd[i];

                    if (pos.x < nodePlacePosList[k][0].x) {
                        pos = nodePlacePosList[k][AdjustRow(k) - 1];
                        pos.z -= 0.5f;
                    }

                    nodePrefabs[k][j].transform.position = pos;
                } else {
                // 右移動
                    Vector3 pos = nodePrefabs[k][j].transform.position;
                    pos.x += moveSpd[i];

                    if (pos.x > nodePlacePosList[k][AdjustRow(k) - 1].x) {
                        pos = nodePlacePosList[k][0];
                        pos.z -= 0.5f;
                    }
                
                    nodePrefabs[k][j].transform.position = pos;
                }
            }
        }
	}
    
	// 検索したい col に合わせた row を返す
	public int AdjustRow(int col) {
		return col % 2 == 0 ? row + 1 : row;
	}
}
