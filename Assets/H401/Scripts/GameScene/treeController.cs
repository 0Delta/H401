using UnityEngine;
using System.Collections;
using DG.Tweening;
using System.Collections.Generic;

public class treeController : MonoBehaviour {

    // 完成した木とかの演出をここでするように
    private GameObject[] treeObjects;

    [SerializeField] private float popTime = 0.0f;      //出現してから待機時間
    [SerializeField] private float moveTime = 0.0f;     //待機後上へ消えていく時間
    [SerializeField] private float popPostionY = 0.0f;
    [SerializeField] private float popSize = 0.0f;
    [SerializeField] private float movePositionY = 0.0f;
    [SerializeField] private string treePrefabPath = null;
    [SerializeField] private Color emissionColor;

    [SerializeField] private float effectPopPosZ = 0.0f;
    [SerializeField] private int[] effectOverScore;
    [SerializeField] private string[] particlePaths;

    static private GameObject treeNodePrefab = null;
    static private GameObject[] particlePrefabs;

    void Awake()
    {
        if (!treeNodePrefab)
            treeNodePrefab = Resources.Load<GameObject>(treePrefabPath);

        particlePrefabs = new GameObject[particlePaths.Length];
        for(int i = 0; i < particlePaths.Length; ++i)
            particlePrefabs[i] = Resources.Load<GameObject>(particlePaths[i]);
    }

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //ノード配列をもらって、（スクリプトのない）オブジェクトだけをコピーして登録
    public void SetTree(List<GameObject> trees, int score)
    {
        //登録
        treeObjects = new GameObject[trees.Count];

        int i = 0;
        foreach(var node in trees)
        {
            //場所
            treeObjects[i] = Instantiate(treeNodePrefab);
            treeObjects[i].transform.position = node.transform.position + new Vector3(0.0f,0.0f,-1.0f);
            treeObjects[i].transform.rotation = node.transform.rotation;
            //(GameObject)Instantiate(treePrefab, node.transform.position + , node.transform.rotation);
            Material mat = treeObjects[i].GetComponent<MeshRenderer>().material = node.GetComponent<MeshRenderer>().material;
            mat.EnableKeyword("_EMISSION");
            mat.DOColor(emissionColor, "_EmissionColor", popTime);
            treeObjects[i].transform.parent = this.transform;

            // エフェクト出現
            Vector3 pos = node.transform.position;
            pos.z -= effectPopPosZ;
            Instantiate(particlePrefabs[0], pos, node.transform.rotation);

            Node nodeScript = node.GetComponent<Node>();
            // 枝先か壁ならエフェクトを出現
            if(nodeScript.Temp.LinkNum == 1 || nodeScript.Temp.LinkNum >= 3 || nodeScript.CheckLinkedWall()) {
                int j = 0;
                for ( ; j < effectOverScore.Length; ++j) {
                    if(score < effectOverScore[j]) {
                        break;
                    }
                }
                --j;
                Instantiate(particlePrefabs[j], pos, node.transform.rotation);
            }

            i++;
        }

        //tweenでその場に待機、その後上へ消えていくように

        //GetComponent<MeshRenderer>().material.DOColor(emissionColor, "_EmissionColor", popTime);
        //this.transform.DOScale(popSize, popTime);
        //this.transform.DOMoveY(this.transform.position.y + popPostionY, popTime)
        //    .OnComplete(() =>
        //    {
        //        this.transform.DOMoveY(this.transform.position.y + movePositionY, moveTime)
        //            .OnComplete(() =>
        //            {
        //                Destroy(this.gameObject);
        //            });
        //    });
        Destroy(this.gameObject);
    }

}
