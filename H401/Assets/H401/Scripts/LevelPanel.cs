using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LevelPanel : MonoBehaviour {

    private LevelTables levelTables;
    public LevelTables LevelTables
    {
        set { levelTables = value; }
    }

    [SerializeField]private float popTime = 0.0f;

    [SerializeField]private float popScale = 0.0f;

    private GameObject gameController = null;
    public void SetGameController(GameObject game) { gameController = game; }
    private LevelController levelController;
    public void SetLevelController(LevelController lev) { levelController = lev; }

    //ボタン用配列
    private GameObject[] buttonObjects;
    private LevelButton[] buttonScripts;
    public int NextLevel
    {
        set { levelController.NextLevel = value; }
    }

    [SerializeField]private GameObject panel;           //ボタン表示用パネル
    [SerializeField] GameObject buttonPrefab = null;    //ボタンのプレハブ

    [SerializeField]private GameObject debugButton = null;  //デバッグの用ゲームに戻るボタン

    void Awake()
    {
        //ここのマジックナンバーはどうにかしたいが
        buttonObjects = new GameObject[5];
        buttonScripts = new LevelButton[5];

        gameController = GameObject.Find("GameController");
    }

	// Use this for initialization
    void Start()
    {
        //levelController.NextLevel = -1;

        levelController = GameObject.Find("levelController").GetComponent<LevelController>();
        levelController.NextLevel = -1;
        float rot = levelController.LyingAngle;

        //ボタンを並べてリンクを付ける
        //GetComponentInChildren<DebugButton>().SetType(_eDebugState.RETURN);
        


        for(int i = 0 ; i < 5 ; i ++)
        {
            Vector3 pos = transform.position;
            pos.y += 42.0f * ((i % 2) == 0 ? 1.0f : -1.0f);
            pos.x += -100.0f + 50.0f * i;

            buttonObjects[i] = (GameObject)Instantiate(buttonPrefab, pos, transform.rotation);
            buttonObjects[i].transform.SetParent(panel.transform);
            buttonScripts[i] = buttonObjects[i].GetComponent<LevelButton>();

            //その他の設定
            buttonScripts[i].RegistLevelNumber(i);
        }

        LevelButton.SetPanel(this);

        panel.transform.Rotate(new Vector3(0.0f, 0.0f, rot));

        //tweenとかで出現エフェクト等
        panel.transform.localScale = new Vector3(popScale,popScale,popScale);
        panel.transform.DOScale(1.0f, popTime).OnComplete(() => { /*gameController.SetActive(false);*/ });
        
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void RegistLevel()
    {
        
    }

    public void Delete()
    {
        transform.DOScale(popScale, popTime).OnComplete(() => { Destroy(this.transform.parent.gameObject); });
    }
}
