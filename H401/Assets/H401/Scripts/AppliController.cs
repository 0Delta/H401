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
    [SerializeField]int gameFrameRate = 0;
    [SerializeField] private string[] scenePrefabPaths;     // シーン Prefab の Path リスト


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
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = gameFrameRate;  //シーンごとにFPSを設定するべきらしい

		ChangeScene(startSceneID);
	}
	
	//---------------------------------------------------------------
	// シーン切り替え
	//---------------------------------------------------------------
	public void ChangeScene(_eSceneID id) {
        // 現在のシーンを削除
        Destroy(currentScenePrefab);

        // 新しいシーンを生成
        currentScenePrefab = Instantiate(Resources.Load<GameObject>(scenePrefabPaths[(int)id]));
        currentScenePrefab.transform.SetParent(transform);
	}

	//---------------------------------------------------------------
	// 現在のシーンを取得
	//---------------------------------------------------------------
    public GameObject GetCurrentScene() {
        return currentScenePrefab;
    }
}
