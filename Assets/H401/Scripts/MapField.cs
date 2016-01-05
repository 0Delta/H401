using UnityEngine;
using System.Collections;

public class MapField : MonoBehaviour {
 
    static private LevelPanel levelPanel;
    static public void SetPanel(LevelPanel panel) { levelPanel = panel; }

    private int _mapNum = 0;
    public int mapNum { get { return _mapNum; } set { _mapNum = value; } }
    private float _rateCnt = 0.0f;
    public float rateCnt { get { return _rateCnt; } set { _rateCnt = value; } }
    private MeshRenderer mRenderer;//Material mat;


	// Use this for initialization
	void Start () {
        //mat = GetComponent<Material>();
        mRenderer = GetComponent<MeshRenderer>();
        /*
        Camera sCamera = transform.parent.parent.GetComponent<LevelChange>().subCamera;
        Vector2 scPos = sCamera.WorldToScreenPoint(mRenderer.bounds.center);
        //左上が0,0になるように直す
        //右下が1,1になるように割合で渡すように
        scPos.y = ((float)sCamera.pixelHeight - scPos.y) / (float)sCamera.pixelHeight;
        scPos.x /= (float)sCamera.pixelWidth;
        mRenderer.sharedMaterial.SetVector("_HighLightPos", new Vector4(scPos.x,scPos.y,0.0f,0.0f));
        rateCnt = 0.0f;
         * */
	}
	
	// Update is called once per frame
	void Update (){
/*        rateCnt += 0.01f;
        rateCnt %= 1.0f;
        mRenderer.sharedMaterial.SetFloat("_RateCnt", rateCnt);

*/
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

    }
    void OnTriggerEnter(Collider col)
    {
        SetLevel();
    }

    void SetColor()
    {

    }
}
