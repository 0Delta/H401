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
// ----- private:
	[SerializeField] private GameObject[] scenePrefabs;		// シーン Prefab リスト

    private GameObject  currentScenePrefab;       // 現在のシーンの Prefab

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
		ChangeScene(_eSceneID.TITLE);	// タイトルへ
	}
	
	//---------------------------------------------------------------
	// シーン切り替え
	//---------------------------------------------------------------
	public void ChangeScene(_eSceneID id) {
        // 現在のシーンを削除
        Destroy(currentScenePrefab);

        // 新しいシーンを生成
        currentScenePrefab = (GameObject)Instantiate(scenePrefabs[(int)id], transform.position, transform.rotation);
	}
}
