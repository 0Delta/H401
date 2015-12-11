using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FeverGauge : MonoBehaviour {

    [SerializeField]private Image FGImage;
    [SerializeField]private Vector3 lightPosition;
    [SerializeField]private Color FGEmission;

    private float GAUGE_MAX = 1.0f;   //最大値
    private float decreaseRatio = 0.0f;
    private float gainRatio = 0.0f;
    
    private float feverValue;   //現在フィーバー値

    private _eFeverState _feverState;
    public _eFeverState feverState
    {
        get { return _feverState; }
    }

    [SerializeField]private string FLightPath = null;
    private GameObject FLightPrefab = null;
    private GameObject FLightObject = null;

//    [SerializeField]private GameObject levelTableObject = null;



    //private FeverLevelInfo feverLevel;
	// Use this for initialization
	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();

        feverValue = 0.0f;
        FGImage.fillAmount = 0.0f;

        _feverState = _eFeverState.NORMAL;

        LevelTables ltScript = gameScene.levelTables;
        gainRatio = ltScript.FeverGainRatio;
        decreaseRatio = ltScript.FeverDecreaseRatio;

        FLightPrefab = Resources.Load<GameObject>(FLightPath);
        
	}
	
	// Update is called once per frame
	void Update () {

        if (_feverState == _eFeverState.FEVER)
        {

            feverValue -= decreaseRatio;

            if (feverValue < 0.0f)
            {
                ChangeState(_eFeverState.NORMAL);
            }
        }


        FGImage.fillAmount = feverValue;
	}
    public void Gain(nodeCountInfo nodeCount)
    {
        if (nodeCount.nodes != 0)
            feverValue += gainRatio;
        //MAXになったらフィーバーモードへ
        //今はとりあえず0に戻す

        if (_feverState == _eFeverState.FEVER)
            return;

        if(feverValue > GAUGE_MAX)
        {
            ChangeState(_eFeverState.FEVER);
        }

    }

    void ChangeState(_eFeverState state)
    {
        _feverState = state;
        switch(_feverState)
        {
            case _eFeverState.NORMAL:
                feverValue = 0.0f;
                if (FLightObject != null)
                    Destroy(FLightObject);
                FGImage.material.EnableKeyword("_EMISSION");
                FGImage.material.SetColor("_EmissionColor", Color.black);
                break;
            case _eFeverState.FEVER:
                //中心地点を設定しなければならないらしい
                FLightObject = Instantiate(FLightPrefab);
                FLightObject.transform.position = lightPosition;
                    //lightPosition,transform.rotation);
                FGImage.material.EnableKeyword("_EMISSION");
                FGImage.material.SetColor("_EmissionColor",FGEmission);
                feverValue = GAUGE_MAX;
                break;
        }
    }
}
