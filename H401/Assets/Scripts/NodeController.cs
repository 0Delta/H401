using UnityEngine;
using System.Collections;

public class NodeController : MonoBehaviour {

    private const float ADJUST_PIXELS_PER_UNIT = 0.01f;     // Pixels Per Unit の調整値

    [SerializeField] private int row = 0;       // 横配置数
    [SerializeField] private int col = 0;       // 縦配置数
    [SerializeField] private GameObject nodePrefab = null;       // パネルのプレハブ

	// Use this for initialization
	void Start () {
        // 描画するパネルの大きさを取得
        float width  = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.width * nodePrefab.transform.localScale.x * ADJUST_PIXELS_PER_UNIT;
        float height = nodePrefab.GetComponent<SpriteRenderer>().sprite.texture.height * nodePrefab.transform.localScale.y * ADJUST_PIXELS_PER_UNIT;
        Vector3 pos = transform.position;

        // パネルを生成
        for(int i = 0; i < col; ++i) {
            // パネルの配置位置を調整(Y座標)
            pos.y = transform.position.y + height * (col * 0.5f - (i + 0.5f));

            for (int j = 0; j < row; ++j) {
                // パネルの配置位置を調整(X座標)
                pos.x = transform.position.x + width * -(row * 0.5f - (j + 0.5f));

                // 生成
        	    Instantiate(nodePrefab, pos, transform.rotation);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
