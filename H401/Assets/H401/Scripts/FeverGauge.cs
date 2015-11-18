using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FeverGauge : MonoBehaviour {

    [SerializeField]private Image FGImage;
    [SerializeField]private float GAUGE_MAX = 1.0f;   //最大値
    [SerializeField]private float decreaseRatio = 0.0f;
    
    private float feverValue;   //現在フィーバー値

    private _eFeverState feverState;
    private _eFeverState prevState;

    //private FeverLevelInfo feverLevel;
	// Use this for initialization
	void Start () {
        feverValue = 0.0f;
        FGImage.fillAmount = 0.0f;

        feverState = _eFeverState.NORMAL;
        prevState = _eFeverState.NORMAL;
	}
	
	// Update is called once per frame
	void Update () {

        prevState = feverState;
        if (feverState == _eFeverState.FEVER)
        {

            feverValue -= decreaseRatio;

            if (feverValue < 0.0f)
            {
                feverState = _eFeverState.NORMAL;
            }
        }
        ChangeState();

        FGImage.fillAmount = feverValue;
	}
    public void Gain(int nodeNum, int cap, int path2, int path3)
    {
        if(nodeNum != 0)
            feverValue += 0.05f;
        //MAXになったらフィーバーモードへ
        //今はとりあえず0に戻す

        if(feverValue > GAUGE_MAX)
        {
            feverState = _eFeverState.FEVER;
        }

    }

    void ChangeState()
    {
        if(feverState != prevState)
        {
            switch(feverState)
            {
                case _eFeverState.NORMAL:
                    feverValue = 0.0f;
                    break;
                case _eFeverState.FEVER:
                    feverValue = GAUGE_MAX;
                    break;
            }
        }
    }
}
