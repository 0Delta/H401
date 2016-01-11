using UnityEngine;
using System.Collections;
using DG.Tweening;
public class MapField : MonoBehaviour {

    [SerializeField]private Color fieldColor;
    [SerializeField]private float colorDuration;
 
    static private LevelPanel levelPanel;
    static public void SetPanel(LevelPanel panel) { levelPanel = panel; }

    private int _mapNum = 0;
    public int mapNum { get { return _mapNum; } set { _mapNum = value; } }
    /*private float _rateCnt = 0.0f;
    public float rateCnt { get { return _rateCnt; } set { _rateCnt = value; } }*/
    private MeshRenderer mRenderer;//Material mat;

	// Use this for initialization
	void Start () {
        //mat = GetComponent<Material>();
        mRenderer = GetComponent<MeshRenderer>();
        mRenderer.material.SetColor("_EmissionColor", Color.black);
	}
	
    void OnDestroy()
    {
        mRenderer.material.DOKill();
    }
	// Update is called once per frame
	void Update (){

	}
    void RedirectedOnTriggerEnter(Collider collider)
    {
        //処理を記述
        SetLevel();

    }

    void RedirectedOnTriggerStay(Collider collider)
    {
        //処理を記述
    }


    public void SetLevel()
    {
        levelPanel.NextLevel = _mapNum;
        print("レベル選択：" + _mapNum.ToString());
        levelPanel.ChangeText(_mapNum);
        //色変えとかここに
        transform.parent.parent.gameObject.GetComponent<LevelChange>().SetArrowPos(mapNum);//ChangeMapColor(mapNum);
        //SetColor();

    }
    void OnTriggerEnter(Collider col)
    {
        SetLevel();
    }

    public void SetColor(bool bColor)
    {
        if (!mRenderer)
            mRenderer = GetComponent<MeshRenderer>();
        Color color = bColor ? fieldColor : Color.black;
        mRenderer.material.EnableKeyword("_Emission");
        mRenderer.material.DOColor(color,"_EmissionColor", colorDuration).OnComplete( () => { SetColor(!bColor); });
    }
}
