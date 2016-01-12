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
    private MeshRenderer mRenderer;

	// Use this for initialization
	void Start () {
        mRenderer = GetComponent<MeshRenderer>();
        mRenderer.material.SetColor("_EmissionColor", Color.black);
	}
	
    void OnDestroy()
    {
        mRenderer.material.DOKill();
    }

    void RedirectedOnTriggerEnter(Collider collider)
    {
        //処理を記述
        SetLevel();
    }

    public void SetLevel()
    {
        levelPanel.NextLevel = _mapNum;
        levelPanel.ChangeText(_mapNum);
        //色変えとかここに
        transform.parent.parent.gameObject.GetComponent<LevelChange>().SetArrowPos(mapNum);
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
