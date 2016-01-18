using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ポップアップシーンを列挙するためのクラス
/// </summary>
[System.Serializable]
public class PopupScene{

    [SerializeField]    AppliController._eSceneID SceneID = 0;
    [SerializeField]    private string ScenePath = "";
    private GameObject SceneObject = null;
    private static Transform PearObject = null;
    private static Dictionary<AppliController._eSceneID, GameObject> dic = null;

    /// <summary>
    /// ポップアップシーンを初期化、辞書に追加します
    /// </summary>
    private void Init()
    {
        try {
            SceneObject = GameObject.Instantiate(Resources.Load<GameObject>(ScenePath));
            SceneObject.transform.SetParent(PearObject);
            SceneObject.GetComponent<Canvas>().worldCamera = Camera.main;
        }
        catch { return; }
        dic.Add(SceneID, SceneObject);
    }

    /// <summary>
    /// リスト内のすべてのシーンを初期化します。
    /// </summary>
    /// <param name="lst">ポップアップシーンのリスト</param>
    /// <param name="pear">親とするオブジェクトのTransform</param>
    public static void InitAll(List<PopupScene> lst, Transform pear)
    {
        PearObject = pear;
        dic = new Dictionary<AppliController._eSceneID, GameObject>();
        foreach (var it in lst)
        {
            it.Init();
        }
    }

    /// <summary>
    /// 作成された辞書を取得します
    /// </summary>
    /// <returns>Dictionaryが帰ります</returns>
    public static Dictionary<AppliController._eSceneID, GameObject>  GetDictionaly()
    {
        return dic;
    }
}
