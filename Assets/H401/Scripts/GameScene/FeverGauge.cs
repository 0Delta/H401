using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FeverGauge : MonoBehaviour {

    [SerializeField]private Image FGImage;
//    [SerializeField]private Vector3 lightPosition;
    [SerializeField]private Color FGEmission;
    [SerializeField]private string FPanelPath = null;
    [SerializeField]private string FeverLogoPath = null;
    [SerializeField]private float gainDuration;

    private GameObject logoObject;
    private float GAUGE_MAX = 1.0f;   //最大値
    private FeverInfo feverInfo;
    
    private float feverValue;   //現在フィーバー値
    public float feverVal { get { return feverValue; } }

    private _eFeverState _feverState;
    public _eFeverState feverState
    {
        get { return _feverState; }
    }


    private GameObject FPanelPrefab = null;
    private GameObject FPanelObject = null;

    private AudioSource audioSource = null;

    private float gainWaitTime;
    private float fillWaitTime;

    //private Tweener fillTweener;
    private float gainedTime;
    private float nextGain;

    delegate void gainMethod();

	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        Vector2 effectTimeInfo = gameScene.gameController.nodeController.gameObject.GetComponent<GameEffect>().effectTimeInfo;
        gainWaitTime = effectTimeInfo.x;
        fillWaitTime = effectTimeInfo.y;
        audioSource = GetComponent<AudioSource>();

        logoObject = Resources.Load<GameObject>(FeverLogoPath);

        feverValue = 0.0f;
        FGImage.fillAmount = 0.0f;
        nextGain = 0.0f;
        _feverState = _eFeverState.NORMAL;

        LevelTables ltScript = gameScene.levelTables;
        feverInfo = ltScript.FeverRatio;

        FPanelPrefab = Resources.Load<GameObject>(FPanelPath);
	}
	
	// Update is called once per frame
	void Update () {
        //FGImage.
        if (_feverState == _eFeverState.FEVER)
        {

            feverValue -= feverInfo.decreaseRatio;

            if (feverValue < 0.0f)
            {
                ChangeState(_eFeverState.NORMAL);
            }

            FGImage.DOKill();
            FGImage.DOFillAmount(feverValue,gainDuration);

        }

	}
    public void Gain(NodeCountInfo nodeCount)
    {
        if (nodeCount.nodes != 0)
        {
            float tempRegain;
            tempRegain = nodeCount.nodes * feverInfo.gainRatio;
            tempRegain *= 1.0f + nodeCount.nodes * feverInfo.gainPerCap;
            tempRegain *= 1.0f + nodeCount.path2 * feverInfo.gainPerPath2;
            tempRegain *= 1.0f + nodeCount.path3 * feverInfo.gainPerPath3;
            tempRegain *= 1.0f + nodeCount.path4 * feverInfo.gainPerPath4;

            feverValue += nextGain;
            if (feverValue > GAUGE_MAX)
            {
                feverValue = GAUGE_MAX;
                if(feverState == _eFeverState.NORMAL)
                    ChangeState(_eFeverState.FEVER);
            }
            nextGain = tempRegain;
            //feverValue += tempRegain;
            StartCoroutine(WaitGain(() =>
           {
               feverValue += nextGain;
               FGImage.DOKill();
               FGImage.DOFillAmount(feverValue,gainDuration);
               nextGain = 0;
               if (feverValue > GAUGE_MAX)
               {
                   feverValue = GAUGE_MAX;
                   if(feverState == _eFeverState.NORMAL)
                       ChangeState(_eFeverState.FEVER);
               }
               audioSource.Play();
           }));
        }
        //MAXになったらフィーバーモードへ
        //今はとりあえず0に戻す

        //audioSource.Play();

        if (_feverState == _eFeverState.FEVER)
            return;
    }

    IEnumerator WaitGain(gainMethod gainM)
    {
        yield return new WaitForSeconds(gainWaitTime);
        gainM();
    }

    void ChangeState(_eFeverState state)
    {
        GameScene gameScene = transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        _feverState = state;
        switch(_feverState)
        {
            case _eFeverState.NORMAL:
                feverValue = 0.0f;
                if (FPanelObject != null)
                    Destroy(FPanelObject);
                FGImage.material.EnableKeyword("_EMISSION");
                FGImage.material.SetColor("_EmissionColor", Color.black);
                
                // ゲーム本編のBGMを再生
                gameScene.PlayBGM();

                break;
            case _eFeverState.FEVER:
                //中心地点を設定しなければならないらしい
                FPanelObject = Instantiate(FPanelPrefab);
                FPanelObject.transform.SetParent(gameScene.gameUI.gamePause.optionCanvas.transform,false);
                    //lightPosition,transform.rotation);
                FGImage.material.EnableKeyword("_EMISSION");
                FGImage.material.SetColor("_EmissionColor",FGEmission);
                feverValue = GAUGE_MAX;
                LogoPop();
                // ゲーム本編のBGMを停止
                gameScene.StopBGM();

                break;
        }
    }

    private void LogoPop()
    {
        GameObject logo = Instantiate(logoObject);
        logo.transform.position = new Vector3(0.0f, -500.0f, -2.5f);
        logo.transform.DOMoveY(-0.1f,0.8f)
            .OnComplete( () => 
            {
                logo.transform.DOMoveY(0.1f,1.6f)
                    .OnComplete( () => 
                    {
                        logo.transform.DOMoveY(500.0f,0.8f)
                        .OnComplete(() => 
                        {
                            Destroy(logo);
                        });
                    });
            });
    }
}
