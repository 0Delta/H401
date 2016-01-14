using UnityEngine;
using System.Collections;

public class TitleScene : MonoBehaviour {
    
    [SerializeField] private string mainCameraPath;
    [SerializeField] private string subCameraPath;
    [SerializeField] private string renderTexturePath;
    [SerializeField] private string titleNodeControllerPath;
    [SerializeField] private string titleCanvasPath;
    [SerializeField] private string eventSystemPath;
    [SerializeField] private string HowToPlayScenePath;
    
    private GameObject mainCameraObject;
    private GameObject subCameraObject;
    private GameObject renderTextureObject;
    private GameObject titleNodeControllerObject;
    private GameObject titleCanvasObject;
    private GameObject eventSystemObject;
    private GameObject HowToPlaySceneObject;

    private TitleNodeController titleNodeControllerScript;
    private TitleCanvas titleCanvasScript;

	// Use this for initialization
	void Start () {
	    mainCameraObject = Instantiate(Resources.Load<GameObject>(mainCameraPath));
        mainCameraObject.transform.SetParent(transform);

	    subCameraObject = Instantiate(Resources.Load<GameObject>(subCameraPath));
        subCameraObject.transform.SetParent(transform);

	    renderTextureObject = Instantiate(Resources.Load<GameObject>(renderTexturePath));
        renderTextureObject.transform.SetParent(transform);

	    titleNodeControllerObject = Instantiate(Resources.Load<GameObject>(titleNodeControllerPath));
        titleNodeControllerObject.transform.SetParent(transform);

	    titleCanvasObject = Instantiate(Resources.Load<GameObject>(titleCanvasPath));
        titleCanvasObject.transform.SetParent(transform);

	    eventSystemObject = Instantiate(Resources.Load<GameObject>(eventSystemPath));
        eventSystemObject.transform.SetParent(transform);

	    //HowToPlaySceneObject = Instantiate(Resources.Load<GameObject>(HowToPlayScenePath));
     //   HowToPlaySceneObject.transform.SetParent(transform);

        titleNodeControllerScript = titleNodeControllerObject.GetComponent<TitleNodeController>();
        titleCanvasScript = titleCanvasObject.GetComponent<TitleCanvas>();
    }

    // ポップアップ的にシーンを遷移する
    public void PopupChangeScene(AppliController._eSceneID sceneID, float fadeInTime, float fadeOutTime) {
        // シーン遷移
        transform.root.gameObject.GetComponent<AppliController>().FadeInOut(fadeInTime, fadeOutTime, () => {
            // ----- フェード中に行う処理

            // ノードの位置を初期位置へ戻す
            titleNodeControllerScript.InitNodesPosition();

            // タイトルで使用している UI を非アクティブにする
            titleCanvasScript.DisableTitleUI();
        });
    }
}
