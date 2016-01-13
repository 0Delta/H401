using UnityEngine;
using System.Collections;

[System.Serializable]
public class NodeTemplate {

    // 設定部分
    public string MaterialName;
    public bool[] LinkDir = new  bool[6];
    
    // 算出部分
    public bool Ready;
    public BitArray Link = new BitArray(6);
    public int LinkNum;
    public int ID = -1;

    public NodeTemplate() {
        MaterialName = "";
        LinkDir = new bool[6];

        // 算出部分
        Ready = false;
        Link = new BitArray(6);
        LinkNum = 0;
        ID = -1;
    }

    // メンバの算出部分を計算する
    public void Calc() {
        int Cnt = 0;
        for(int n = 0; n < 6; n++) {
            if(LinkDir.Length < 6) {
                return;
            }
            if(LinkDir[n])
                Cnt++;
            Link.Set(n, LinkDir[n]);
        }
        LinkNum = Cnt;
        if(LinkNum == 0) {
            Ready = false;
        } else {
            Ready = true;
        }
    }

    // 指定した枝数を持つノードをカウントする
    static public int CountNodeType(NodeTemplate[] TempList, int BranchNum) {
        int ret = 0;
        foreach(var it in TempList) {
            if(it.LinkNum == BranchNum) {
                ret++;
            }
        }
        return ret;
    }

    // 指定した枝数を持つノードのX番目を取得する
    static public NodeTemplate GetTempFromBranchIndex(NodeTemplate[] TempList, int BranchNum, int Index) {
        foreach(var it in TempList) {
            if(it.LinkNum == BranchNum) {
                if(Index <= 0) {
                    return it;
                }
                Index--;
            }
        }
        return null;
    }

    // 指定した枝数を持つノードをランダムで選ぶ
    static public NodeTemplate GetTempFromBranchRandom(NodeTemplate[] TempList, int BranchNum) {
        return GetTempFromBranchIndex(TempList, BranchNum, Mathf.FloorToInt(Random.Range(0f, CountNodeType(TempList, BranchNum))));
    }

    static public void AllCalc(NodeTemplate[] TempList) {
        foreach(var it in TempList) {
            it.Calc();
        }
    }
}