using UnityEngine;
using System.Collections.Generic;

public class TitleScene : MonoBehaviour {
    
    [SerializeField] private string mainCameraPath;
    [SerializeField] private string subCameraPath;
    [SerializeField] private string renderTexturePath;
    [SerializeField] private string titleNodeControllerPath;
    [SerializeField] private string titleCanvasPath;
    [SerializeField] private string eventSystemPath;

    [SerializeField] private Color normalSceneFilterColor;
    [SerializeField] private Color popupSceneFilterColor;
    
    private GameObject mainCameraObject;
    private GameObject subCameraObject;
    private GameObject renderTextureObject;
    private GameObject titleNodeControllerObject;
    private GameObject titleCanvasObject;
    private GameObject eventSystemObject;

    private TitleNodeController titleNodeControllerScript;
    private TitleCanvas titleCanvasScript;

    private MeshRenderer renderTextureMeshRenderer;

    private bool _isPopupScene;
    public bool isPopupScene {
        get { return _isPopupScene; }
    }
    
    // ※ポップアップシーンの連想配列をつくる
    private Dictionary<AppliController._eSceneID, GameObject> popupSceneTable;
    [SerializeField]private List<PopupScene> popScene;

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

        titleNodeControllerScript = titleNodeControllerObject.GetComponent<TitleNodeController>();
        titleCanvasScript = titleCanvasObject.GetComponent<TitleCanvas>();

        renderTextureMeshRenderer = renderTextureObject.GetComponent<MeshRenderer>();

        _isPopupScene = false;

        // ポップアップシーンを設定
        PopupScene.InitAll(popScene, transform);
        popupSceneTable = PopupScene.GetDictionaly();

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
//            subCameraObject.SetActive(false);
            titleCanvasScript.DisableTitleUI();

            // ポップアップ中はタイトルにフィルターをかける
            renderTextureMeshRenderer.material.SetColor("_TexColor", popupSceneFilterColor);

            // 遷移先シーンをアクティブにする
            popupSceneTable[sceneID].SetActive(true);

            _isPopupScene = true;
        });
    }

    // ポップアップシーンからタイトルへ戻す
    public void ReturnTitleScene() {
        // 遷移元のシーンIDを算出
        AppliController._eSceneID sceneID = AppliController._eSceneID.TITLE;
        foreach(var key in popupSceneTable.Keys) {
            if(popupSceneTable[key].activeSelf) {
                sceneID = key;
                break;
            }
        }

        FadeTime fadeTime = titleCanvasScript.GetFadeTime(sceneID);

        // シーン遷移
        transform.root.gameObject.GetComponent<AppliController>().FadeInOut(fadeTime.inTime, fadeTime.outTime, () => {
            // ----- フェード中に行う処理

            // フィルターの色を元に戻す
            renderTextureMeshRenderer.material.SetColor("_TexColor", normalSceneFilterColor);

            // ポップアップシーンを非アクティブにする
            foreach(var key in popupSceneTable.Keys) {
                popupSceneTable[key].SetActive(false);
            }

            // タイトルで使用しているオブジェクトをアクティブにする
//            subCameraObject.SetActive(true);
            titleCanvasScript.EnableTitleUI();

            // ノードの動きを再開する
            titleNodeControllerScript.isMoveNodes = true;

            _isPopupScene = false;
        });
    }
}
