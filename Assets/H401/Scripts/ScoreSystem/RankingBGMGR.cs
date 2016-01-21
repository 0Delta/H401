using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UniRx;

public class RankingBGMGR : MonoBehaviour {

    [SerializeField]    public string RenderTexturePrefabName = null;
    [SerializeField]    public string TitleNodeControllerPrefabName = null;
    [SerializeField]    public string SubCameraPrefabName = null;
    [SerializeField]    public string TitleBacksPrefabName = null;
    [SerializeField]    public Vector3 BGPosition = new Vector3(0f, 0f, 0f);
    [SerializeField]    private Color filterColor = new Color(0f, 0f, 0f);
    private Color filterColorBak = new Color(0f, 0f, 0f);
    private GameObject RenderTexturePrefab;
    private GameObject TitleNodeControllerPrefab;
    private GameObject SubCameraPrefab;
    private GameObject TitleBacksPrefab;

    bool InstantiateChild(string Name,out GameObject RetObj)
    {
        try
        {
            RetObj = Instantiate(Resources.Load(Name)) as GameObject;
            RetObj.transform.SetParent(transform, true);
        }
        catch (Exception excep)
        {
            Debug.LogError("Exception ! : " + Name);
            Debug.LogException(excep);
            RetObj = null;
            return false;
        }
        return true;
    }


    // Use this for initialization
    void Start() {

        InstantiateChild(RenderTexturePrefabName, out RenderTexturePrefab);
        RenderTexturePrefab.transform.localPosition = BGPosition;
        InstantiateChild(TitleNodeControllerPrefabName, out TitleNodeControllerPrefab);
        InstantiateChild(SubCameraPrefabName, out SubCameraPrefab);
        try
        {
            TitleBacksPrefab = Instantiate(Resources.Load(TitleBacksPrefabName)) as GameObject;
            TitleBacksPrefab.transform.SetParent(transform.FindChild("Contents").transform, false);
            TitleBacksPrefab.transform.localScale = new Vector3(1.01f, 1.01f, 1f);
        }
        catch (Exception excep)
        {
            Debug.LogError("Exception ! : " + TitleBacksPrefabName);
            Debug.LogException(excep);
        }

        try
        {
            RenderTexturePrefab.transform.localPosition = BGPosition;
            //TitleNodeControllerPrefab.transform.position += new Vector3(100, 0, 0);
            //SubCameraPrefab.transform.position += new Vector3(100, 0, 0);
        }
        catch
        {
            // Failed Load BG
        }

        Observable
            .NextFrame(FrameCountType.EndOfFrame)
            .Subscribe(_ => {
                try
                {
                    filterColorBak = RenderTexturePrefab.GetComponentInChildren<MeshRenderer>().material.GetColor("_TexColor");
                    RenderTexturePrefab.GetComponentInChildren<MeshRenderer>().material.SetColor("_TexColor", filterColor);     // ポップアップ中はタイトルにフィルターをかける
                    var Cont = TitleNodeControllerPrefab.GetComponentInChildren<TitleNodeController>();
                    Cont.InitNodesPosition();
                    Cont.isMoveNodes = false;                      // ノードの位置を初期位置へ戻す
                }
                catch { }

            }).AddTo(this);

    }
	
	// Update is called once per frame
	void Update () {
	
	}


    public void OnDestroy()
    {
        RenderTexturePrefab.GetComponentInChildren<MeshRenderer>().material.SetColor("_TexColor", filterColorBak);     // ポップアップ中はタイトルにフィルターをかける
    }
}
