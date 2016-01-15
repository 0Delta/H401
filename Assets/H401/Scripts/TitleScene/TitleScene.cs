using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TitleScene : MonoBehaviour {
    
    [SerializeField] private string mainCameraPath;
    [SerializeField] private string subCameraPath;
    [SerializeField] private string renderTexturePath;
    [SerializeField] private string titleNodeControllerPath;
    [SerializeField] private string titleCanvasPath;
    [SerializeField] private string eventSystemPath;
    [SerializeField] private string optionScenePath;
    [SerializeField] private string howToPlayScenePath;
    [SerializeField] private string creditScenePath;

    [SerializeField] private Color normalSceneFilterColor;
    [SerializeField] private Color popupSceneFilterColor;
    
    private GameObject mainCameraObject;
    private GameObject subCameraObject;
    private GameObject renderTextureObject;
    private GameObject titleNodeControllerObject;
    private GameObject titleCanvasObject;
    private GameObject eventSystemObject;
    private GameObject optionSceneObject;
    private GameObject howToPlaySceneObject;
    private GameObject creditSceneObject;

    private TitleNodeController titleNodeControllerScript;
    private TitleCanvas titleCanvasScript;

    private MeshRenderer renderTextureMeshRenderer;
    
    // ※決め打ちでポップアップシーンの連想配列をつくる(できればなんとかしたい)
    private Dictionary<AppliController._eSceneID, GameObject> popupSceneTable;
    
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

        optionSceneObject = Instantiate(Resources.Load<GameObject>(optionScenePath));
        optionSceneObject.transform.SetParent(transform);
        optionSceneObject.GetComponent<Canvas>().worldCamera = Camera.main;
        
        howToPlaySceneObject = Instantiate(Resources.Load<GameObject>(howToPlayScenePath));
        howToPlaySceneObject.transform.SetParent(transform);
        howToPlaySceneObject.GetComponent<Canvas>().worldCamera = Camera.main;
        
        creditSceneObject = Instantiate(Resources.Load<GameObject>(creditScenePath));
        creditSceneObject.transform.SetParent(transform);
        creditSceneObject.GetComponent<Canvas>().worldCamera = Camera.main;

        titleNodeControllerScript = titleNodeControllerObject.GetComponent<TitleNodeController>();
        titleCanvasScript = titleCanvasObject.GetComponent<TitleCanvas>();

        renderTextureMeshRenderer = renderTextureObject.GetComponent<MeshRenderer>();

        popupSceneTable = new Dictionary<AppliController._eSceneID, GameObject> {
            { AppliController._eSceneID.OPTION,     optionSceneObject },
            { AppliController._eSceneID.HOWTOPLAY,  howToPlaySceneObject },
            { AppliController._eSceneID.CREDIT,     creditSceneObject },
        };
        foreach(GameObject scene in popupSceneTable.Values) {
            scene.SetActive(false);
        }
    }

    // ポップアップ的にシーンを遷移する
    public void PopupChangeScene(AppliController._eSceneID sceneID, float fadeInTime, float fadeOutTime) {
        // シーン遷移
        transform.root.gameObject.GetComponent<AppliController>().FadeInOut(fadeInTime, fadeOutTime, () => {
            // ----- フェード中に行う処理

            // ボタンの Transform を初期値へ戻す
            titleCanvasObject.GetComponent<TitleCanvas>().InitButtonsTransform();

            // ノードの位置を初期位置へ戻す
            titleNodeControllerScript.InitNodesPosition();
            titleNodeControllerScript.isMoveNodes = false;

            // タイトルで使用している不必要なオブジェクトを非アクティブにする
            subCameraObject.SetActive(false);
            titleCanvasScript.DisableTitleUI();

            // ポップアップ中はタイトルにフィルターをかける
            renderTextureMeshRenderer.material.SetColor("_TexColor", popupSceneFilterColor);

            // 遷移先シーンをアクティブにする
            popupSceneTable[sceneID].SetActive(true);
        });
    }

    // ポップアップシーンからタイトルへ戻す
    public void ReturnTitleScene(AppliController._eSceneID sceneID, float fadeInTime, float fadeOutTime) {
        // シーン遷移
        transform.root.gameObject.GetComponent<AppliController>().FadeInOut(fadeInTime, fadeOutTime, () => {
            // ----- フェード中に行う処理

            // フィルターの色を元に戻す
            renderTextureMeshRenderer.material.SetColor("_TexColor", normalSceneFilterColor);

            // ポップアップシーンを非アクティブにする
            popupSceneTable[sceneID].SetActive(false);

            // タイトルで使用しているオブジェクトをアクティブにする
            subCameraObject.SetActive(true);
            titleCanvasScript.EnableTitleUI();

            // ノードの動きを再開する
            titleNodeControllerScript.isMoveNodes = true;
        });
    }
}
