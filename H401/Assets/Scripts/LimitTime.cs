using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LimitTime : MonoBehaviour {

    public Image timeImage;

    private float nowTime;  //現在時間
    [SerializeField] private float maxTime = 0.0f;  //時間の最大値(秒？)
    [SerializeField] private float MaxRegainRatio = 0.0f;  //回復する最大値（割合）
    [SerializeField] private float SlipRatio = 0.0f;    //現象割合（秒間）
    [SerializeField] private float RegainPer3Nodes = 0.0f;
    [SerializeField] private float RegainPerCap = 0.0f;
    [SerializeField] private float RegainPer2Path = 0.0f;
    [SerializeField] private float RegainPer3Path = 0.0f;
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        nowTime += Time.deltaTime * SlipRatio;

        SetImage();

        if(nowTime > maxTime)
        {
            //ここにゲームオーバー処理
            print("タイムオーバー");
        }

	}

    //枝の数と種類をもらって時間を割合で回復させる
    public void PlusTime(int nodeNum, int cap, int path2, int path3)
    {
        //計算
        float tempRatio = 0.0f;
        //最大値を超えていたらカット

        //ノード３つ毎に１割増える ３分岐１つ毎に５分増える
        tempRatio = (float)(nodeNum / 3) * RegainPer3Nodes  //ノード３つごとに数％
            + cap * RegainPerCap                            //１パスのノード１つ毎に数％
            + path2 * RegainPer2Path                        //２パスのノード１つ毎に数％
            + path3 * RegainPer3Path;                       //３パスのノード１つ毎に数％

        if (tempRatio > MaxRegainRatio)
            tempRatio = MaxRegainRatio;

        nowTime -= maxTime * tempRatio;
        if(nowTime < 0)
        {
            nowTime = 0.0f;
        }

        //
    }


    private void SetImage()
    {
        timeImage.fillAmount = 1.0f - nowTime / maxTime;
    }
}
