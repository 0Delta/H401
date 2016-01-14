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
    private SpriteRenderer sRenderer;

	// Use this for initialization
	void Start () {
        sRenderer = GetComponent<SpriteRenderer>();
        sRenderer.material.SetColor("_EmissionColor", Color.black);
	}
	
    void OnDestroy()
    {
        sRenderer.material.DOKill();
    }

    //2DになったのでRayCastを受ける必要がなくなった
    void OnMouseDown()
    {
        SetLevel();
    }
    public void SetLevel()
    {
        levelPanel.NextLevel = _mapNum;
        levelPanel.ChangeText(_mapNum);
        //色変えとかここに
        transform.parent.parent.gameObject.GetComponent<LevelChange>().SetArrowPos(mapNum);
    }

    public void SetColor(bool bColor)
    {
        if (!sRenderer)
            sRenderer = GetComponent<SpriteRenderer>();
        Color color = bColor ? fieldColor : Color.black;
        sRenderer.material.EnableKeyword("_Emission");
        sRenderer.material.DOColor(color,"_EmissionColor", colorDuration).OnComplete( () => { SetColor(!bColor); });
    }
}
