using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class FeverGauge : MonoBehaviour {

    [SerializeField]private Image FGImage;
    [SerializeField]private Vector3 lightPosition;
    [SerializeField]private Color FGEmission;
    private float GAUGE_MAX = 1.0f;   //最大値
    private FeverInfo feverInfo;
    
    private float feverValue;   //現在フィーバー値
    public float feverVal { get { return feverValue; } }

    private _eFeverState _feverState;
    public _eFeverState feverState
    {
        get { return _feverState; }
    }

    [SerializeField]private string FLightPath = null;
    private GameObject FLightPrefab = null;
    private GameObject FLightObject = null;

    private AudioSource audioSource = null;

    private float gainDuration;
    private float fillWaitTime;

    private Tweener fillTweener;

    delegate void gainMethod();

	void Start () {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        Vector2 effectTimeInfo = gameScene.gameController.nodeController.gameObject.GetComponent<GameEffect>().effectTimeInfo;
        gainDuration = effectTimeInfo.x;
        fillWaitTime = effectTimeInfo.y;
        audioSource = GetComponent<AudioSource>();

        feverValue = 0.0f;
        FGImage.fillAmount = 0.0f;

        _feverState = _eFeverState.NORMAL;

        LevelTables ltScript = gameScene.levelTables;
        feverInfo = ltScript.FeverRatio;

        FLightPrefab = Resources.Load<GameObject>(FLightPath);
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

        if (fillTweener != null && fillTweener.IsPlaying())
            fillTweener.ChangeEndValue(feverValue);
        else
            FGImage.fillAmount = feverValue;



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

            feverValue += tempRegain;
        }
        //MAXになったらフィーバーモードへ
        //今はとりあえず0に戻す

        //audioSource.Play();

        if (_feverState == _eFeverState.FEVER)
            return;

        if(feverValue > GAUGE_MAX)
        {
            ChangeState(_eFeverState.FEVER);
        }

        if (fillTweener != null)
            fillTweener.Kill();

        if (feverState != _eFeverState.FEVER)
            StartCoroutine(WaitGain(() => { fillTweener = FGImage.DOFillAmount(feverValue, fillWaitTime); audioSource.Play(); }));
        else
            FGImage.fillAmount = feverValue;
    }

    IEnumerator WaitGain(gainMethod gainM)
    {
        yield return new WaitForSeconds(gainDuration);
        gainM();
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
                
                // ゲーム本編のBGMを再生
                transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().PlayBGM();

                break;
            case _eFeverState.FEVER:
                //中心地点を設定しなければならないらしい
                FLightObject = Instantiate(FLightPrefab);
                FLightObject.transform.position = lightPosition;
                    //lightPosition,transform.rotation);
                FGImage.material.EnableKeyword("_EMISSION");
                FGImage.material.SetColor("_EmissionColor",FGEmission);
                feverValue = GAUGE_MAX;

                // ゲーム本編のBGMを停止
                transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().StopBGM();

                break;
        }
    }
}
