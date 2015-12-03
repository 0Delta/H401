//===============================================================
// AppliController
// Author : Kei Hashimoto
//===============================================================
using UnityEngine;
using System.Collections;

public class AppliController : MonoBehaviour {
//===============================================================
// メンバ定数
//===============================================================
// ----- public:
	public enum _eSceneID : int {
		TITLE,              // タイトル
		GAME,               // ゲーム本編
		RESULT,             // リザルト
		OFFLINE_RANKING,	// オフラインランキング
		ONLINE_RANKING,		// オンラインランキング
        OPTION,             // オプション

		MAX_NUM
	}

//===============================================================
// メンバ変数
//===============================================================
// ----- public:
    public _eSceneID startSceneID;      // ゲーム開始時のシーンID

// ----- private:
    [SerializeField]private string[] scenePaths;//GameObject[] scenePrefabs;		// シーン Prefab リスト

    private GameObject  currentScenePrefab;       // 現在のシーンの Prefab

    private GameScene _gameScene = null;
    public GameScene gameScene { get { return _gameScene; } }

//===============================================================
// メンバ関数
//===============================================================
// ----- private:
	//---------------------------------------------------------------
	// ゼロクリア
	//---------------------------------------------------------------
	void Awake() {
        currentScenePrefab = new GameObject();
	}
    
	//---------------------------------------------------------------
	// 初期化
	//---------------------------------------------------------------
	void Start() {
		// 次のシーンへ
		ChangeScene(startSceneID);
	}
	
	//---------------------------------------------------------------
	// シーン切り替え
	//---------------------------------------------------------------
	public void ChangeScene(_eSceneID id) {
        // 現在のシーンを削除
        Destroy(currentScenePrefab);

        // 新しいシーンを生成
        currentScenePrefab = Instantiate(Resources.Load<GameObject>(scenePaths[(int)id]));
        currentScenePrefab.transform.SetParent(transform);
        //currentScenePrefab.transform.position = transform.position;
        //currentScenePrefab.transform.rotation = transform.rotation;

            //(GameObject)Instantiate(scenePrefabs[(int)id], transform.position, transform.rotation);
        switch (id)
        {
            case _eSceneID.GAME:
                _gameScene = currentScenePrefab.GetComponent<GameScene>();
                break;
        }


 


	}
}
