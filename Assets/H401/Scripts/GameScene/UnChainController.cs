using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnChainController : MonoBehaviour
{

    //枝途切れ字のオブジェクトはこっちで管理する
    [SerializeField]
    private string unChainCubePath = null;
    [SerializeField]
    private float linkDistance;     //ノードとこのオブジェクトの距離
    private GameObject unChainCubePrefab = null;    //枝未完成協調表示のためのオブジェクト

    private List<UnChainObject> unChainCubeList;

    // Use this for initialization
    void Start()
    {
        unChainCubePrefab = Resources.Load<GameObject>(unChainCubePath);

        unChainCubeList = new List<UnChainObject>();

    }
    //public delegate bool isSame (Vec2Int pos,_eLinkDir dir);

    public void AddObj(Node node, _eLinkDir linkTo)
    {

        //リストに同じものがあれば新規作成はしないように
        UnChainObject uc = unChainCubeList.Find(x =>
        {
            if (x.linkVec == linkTo && x.nodeVec.x == node.NodeID.x && x.nodeVec.y == node.NodeID.y)
                return true;
            else
                return false;
        });
        if (uc != null)
        {
            //同じ物だったらチェック済にして消さないように
            uc.bChecked = true;
            return;
        }

        //なかった場合、リストに追加
        GameObject newCube = Instantiate(unChainCubePrefab);

        newCube.transform.position = new Vector3(0.0f, linkDistance, 0.0f);
        float rotAngle = 60.0f * (int)linkTo + 30.0f;
        //        newCube.transform.Rotate(new Vector3(0.0f, 0.0f, -rotAngle), Space.World);
        newCube.transform.RotateAround(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f), -rotAngle);
        newCube.transform.position += node.transform.position;
        newCube.transform.SetParent(transform);
        //tween等で出現時アニメーション
        UnChainObject uco = newCube.GetComponent<UnChainObject>();
        uco.linkVec = linkTo;
        uco.nodeVec = node.NodeID;

        unChainCubeList.Add(uco);
    }
    public void Remove()
    {
        List<UnChainObject> delList = new List<UnChainObject>();
        foreach (var cube in unChainCubeList)
        {

            //キューブにtweenを設定して消去
            if (cube.bChecked == false)
            {
                delList.Add(cube);
                cube.Vanish();
            }

            cube.bChecked = false;
        }

        //foreach中にリストをいじるとエラーになるようなので
        //別リストに渡して消去するように
        foreach (var delcube in delList)
        {
            unChainCubeList.Remove(delcube);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    //スライド操作時にスライドノードを指定して消去
    public void RemoveWithNode(Vec2Int nodeID)
    {
        //IDが一致するものを消去

        List<UnChainObject> delList;
        delList = unChainCubeList.FindAll(x =>
        {
            if (x.nodeVec.x == nodeID.x && x.nodeVec.y == nodeID.y)
                return true;
            else
                return false;
        });

        foreach (var ucObj in delList)
        {
            ucObj.Vanish();
            unChainCubeList.Remove(ucObj);
        }
    }
}
