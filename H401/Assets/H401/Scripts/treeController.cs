using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class treeController : MonoBehaviour {

    // 完成した木とかの演出をここでするように
    private GameObject[] treeObjects;

    [SerializeField]private float popTime = 0.0f;      //出現してから待機時間
    [SerializeField]private float moveTime = 0.0f;     //待機後上へ消えていく時間
    [SerializeField]private float popPostionY = 0.0f;
    [SerializeField]private float popSize = 0.0f;
    [SerializeField]private float movePositionY = 0.0f;
    [SerializeField]private GameObject treePrefab = null;

    [SerializeField]private Color emissionColor;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //ノード配列をもらって、（スクリプトのない）オブジェクトだけをコピーして登録
    public void SetTree(List<GameObject> trees)
    {
        //登録
        treeObjects = new GameObject[trees.Count];

        int i = 0;
        foreach(var node in trees)
        {
            //場所
            treeObjects[i] = (GameObject)Instantiate(treePrefab, node.transform.position + new Vector3(0.0f,0.0f,-1.0f), node.transform.rotation);
            Material mat = treeObjects[i].GetComponent<MeshRenderer>().material;
            mat = node.GetComponent<MeshRenderer>().material;
            mat.EnableKeyword("_EMISSION");
            mat.DOColor(emissionColor, "_EmissionColor", popTime);
            treeObjects[i].transform.parent = this.transform;
            i++;
        }

        //tweenでその場に待機、その後上へ消えていくように

        //GetComponent<MeshRenderer>().material.DOColor(emissionColor, "_EmissionColor", popTime);
        this.transform.DOScale(popSize, popTime);
        this.transform.DOMoveY(this.transform.position.y + popPostionY, popTime)
            .OnComplete(() =>
            {
                this.transform.DOMoveY(this.transform.position.y + movePositionY, moveTime)
                    .OnComplete(() =>
                    {
                        Destroy(this.gameObject);
                    });
            });
        
    }

}
