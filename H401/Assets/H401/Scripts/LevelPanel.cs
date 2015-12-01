using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour {

    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;
    [SerializeField]private string buttonPrefabPath = null;
    private GameObject buttonPrefab = null;    //ボタンのプレハブ
//    private GameObject gameController = null;
//    public void SetGameController(GameObject game) { gameController = game; }
    private LevelController levelController;
    public void SetLevelController(LevelController lev) { levelController = lev; }

    //ボタン用配列
    private GameObject[] buttonObjects;
    private LevelButton[] buttonScripts;
    public int NextLevel
    {
        set { levelController.NextLevel = value; }
    }


    private Text fieldText = null;

    void Awake()
    {
        //ここのマジックナンバーはどうにかしたいが
        buttonObjects = new GameObject[5];
        buttonScripts = new LevelButton[5];

//        gameController = GameObject.Find("GameController");
    }

	// Use this for initialization
    void Start()
    {
        buttonPrefab = Resources.Load<GameObject>(buttonPrefabPath);
        //levelController.NextLevel = -1;

        levelController = GetComponentInParent<LevelController>();
        levelController.NextLevel = -1;
        float rot = levelController.LyingAngle;


        //テキストオブジェクトと関連つけ
        fieldText = transform.FindChild("FieldNameImage").FindChild("fieldNameText").GetComponent<Text>();


        //ボタンを並べてリンクを付ける
        


        for(int i = 0 ; i < 5 ; i ++)
        {
            Vector3 pos = transform.position;
            pos.y += 42.0f * ((i % 2) == 0 ? 1.0f : -1.0f);
            pos.x += -100.0f + 50.0f * i;

            buttonObjects[i] = (GameObject)Instantiate(buttonPrefab, pos, transform.rotation);
            buttonObjects[i].transform.SetParent(transform);
            buttonScripts[i] = buttonObjects[i].GetComponent<LevelButton>();

            //その他の設定
            buttonScripts[i].RegistLevelNumber(i);
        }

        LevelButton.SetPanel(this);

        transform.Rotate(new Vector3(0.0f, 0.0f, rot));

        //tweenとかで出現エフェクト等
        transform.localScale = new Vector3(popScale,popScale,popScale);
        transform.DOScale(1.0f, popTime);//.OnComplete(() => { /*gameController.SetActive(false);*/ });
        
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void RegistLevel()
    {
        
    }

    public void Delete(NodeController nC)
    {
        transform.DOScale(popScale, popTime)
            .OnComplete(() => {
                Destroy(this.transform.parent.gameObject);
                if (levelController.NextLevel != -1)
                    nC.currentLevel = levelController.NextLevel;
                levelController.LevelState = _eLevelState.STAND;
            });
    }

    public void ChangeText(int stage)
    {
        fieldText.text = levelController.GetFieldName(stage);
    }

}
