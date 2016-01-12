using UnityEngine;
using System.Collections;

public class Touch : MonoBehaviour {

    private Camera c;

    public float distance = 100f;

	// Use this for initialization
	void Start () {
        c = GetComponent<Camera>();
	}

    void Update()
    {
        // 左クリックを取得
        if (Input.GetMouseButtonDown(0))
        {
            // クリックしたスクリーン座標をrayに変換
            Ray ray = c.ScreenPointToRay(Input.mousePosition);
            // Rayの当たったオブジェクトの情報を格納する
            RaycastHit hit = new RaycastHit();
            // オブジェクトにrayが当たった時
            if (Physics.Raycast(ray, out hit, distance))
            {
                // rayが当たったオブジェクトの名前を取得
                MapField mf = hit.collider.gameObject.GetComponent<MapField>();
                if(mf)
                    mf.SetLevel();
            }
        }
    }
}
