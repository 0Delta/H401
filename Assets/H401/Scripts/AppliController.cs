//===============================================================
// AppliController
// Author : Kei Hashimoto
//===============================================================
using UnityEngine;
using System.Collections;
using UniRx;

public class AppliController : MonoBehaviour {
//===============================================================
// メンバ定数
//===============================================================
// ----- public:
	public enum _eSceneID : int {
		TITLE,              // タイトル
		GAME,               // ゲーム本編
		RESULT,             // リザルト
		RANKING,	        // ランキング
		OPTION,             // オプション
		HOWTOPLAY,          // 遊び方
		CREDIT,             // クレジット

		MAX_NUM
	}

//===============================================================
// メンバ変数
//===============================================================
// ----- public:
    public _eSceneID startSceneID;      // ゲーム開始時のシーンID

// ----- private:
    [SerializeField] int gameFrameRate = 0;
    [SerializeField] private string[] scenePrefabPaths;         // シーン Prefab の Path リスト
    [SerializeField] private string fadeCanvasName = null;      // シーン切り替え時の演出用 Canvas の名前
    [SerializeField] private FadeTime gameStartFadeTime;        // ゲーム開始時のフェード演出にかかる時間
    [SerializeField] private string tapKirakiraPath;            // タップ時のキラキラエフェクト Prefab の Path
    [SerializeField] private float effectPosZ;                  // タップ時のキラキラエフェクトの出現位置(Z座標)

    private GameObject currentScenePrefab;      // 現在のシーンの Prefab
    private Fade       fade;                    // シーン切り替え時の演出用 Script
    private GameObject tapKirakiraPrefab;       // タップ時のキラキラエフェクトの Prefab


//===============================================================
// メンバ関数
//===============================================================
// ----- private:
	//---------------------------------------------------------------
	// ゼロクリア
	//---------------------------------------------------------------
	void Awake() {
        currentScenePrefab = null;
        fade = null;
	}
    
	//---------------------------------------------------------------
	// 初期化
	//---------------------------------------------------------------
	void Start() {
        fade = transform.FindChild(fadeCanvasName).gameObject.GetComponentInChildren<Fade>();

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = gameFrameRate;  //シーンごとにFPSを設定するべきらしい

        ScoreManager.Load();

        // プレハブをロード
        tapKirakiraPrefab = Resources.Load<GameObject>(tapKirakiraPath);
		
        // 次のシーンへ
		ChangeScene(startSceneID, gameStartFadeTime.inTime, gameStartFadeTime.outTime);
        
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonUp(0))
            .Subscribe(_ => {
                Vector3 worldTapPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                worldTapPos.z = effectPosZ;
                Instantiate(tapKirakiraPrefab, worldTapPos, tapKirakiraPrefab.transform.rotation);
            })
            .AddTo(gameObject);
	}
	
	//---------------------------------------------------------------
	// シーン切り替え
	//---------------------------------------------------------------
	public void ChangeScene(_eSceneID id, float fadeInTime = 0.0f, float fadeOutTime = 0.0f) {
        // フェードアウト
        fade.FadeIn(fadeOutTime, () => {
            // 現在のシーンを削除
            Destroy(currentScenePrefab);
            
            // 新しいシーンを生成
            GameObject Obj = Resources.Load<GameObject>(scenePrefabPaths[(int)id]);
            if(Obj == null) {
                Debug.LogError("[" + scenePrefabPaths[(int)id] + "] is Missing !! \n Check [AppliController.Start Scene ID]");
                if(id == (_eSceneID)0) {
                    throw (new MissingReferenceException());
                } else {
                    Obj = Resources.Load<GameObject>(scenePrefabPaths[0]);
                }
            }
            currentScenePrefab = Instantiate(Obj);
            currentScenePrefab.transform.SetParent(transform);

            // ガーベジコレクション
            System.GC.Collect();
            Resources.UnloadUnusedAssets();     //使ってないアセットをアンロード

            fade.FadeOut(fadeInTime);
        });
	}
   
	//---------------------------------------------------------------
	// フェードイン・アウト(フェード中に任意の処理を行う)
	//---------------------------------------------------------------
    public void FadeInOut(float fadeInTime, float fadeOutTime, System.Action action) {
        fade.FadeIn(fadeOutTime, () => {
            action();
            System.GC.Collect();                //GC
            Resources.UnloadUnusedAssets();     //使ってないアセットをアンロード

            fade.FadeOut(fadeInTime);
        });
    }

	//---------------------------------------------------------------
	// 現在のシーンを取得
	//---------------------------------------------------------------
    public GameObject GetCurrentScene() {
        return currentScenePrefab;
    }
}
