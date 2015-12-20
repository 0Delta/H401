using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour {

    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;
 //   private GameObject buttonPrefab = null;    //ボタンのプレハブ
//    private GameObject gameController = null;
//    public void SetGameController(GameObject game) { gameController = game; }
    private LevelController levelController;
    public void SetLevelController(LevelController lev) { levelController = lev; }

    //ボタン用配列
//    private GameObject[] buttonObjects;
    /*private LevelButton[] buttonScripts;*/
    public int NextLevel
    {
        set { levelController.NextLevel = value; }
    }
    private MapField[] fieldScripts;

    private Text fieldText = null;

    void Awake()
    {
        //ここのマジックナンバーはどうにかしたいが

//        gameController = GameObject.Find("GameController");
    }

	// Use this for initialization
    void Start()
    {
        //levelController.NextLevel = -1;

        //このあたりをコントローラ側からセットするように
        //levelController = GetComponentInParent<LevelController>();
        levelController.NextLevel = -1;
        float rot = levelController.LyingAngle;


        //テキストオブジェクトと関連つけ
        fieldText = gameObject.transform.FindChild("FieldNameImage").FindChild("fieldNameText").GetComponent<Text>();


        //ボタンを並べてリンクを付ける
        //ボタンからメッシュデータに変更されました
        fieldScripts = levelController.levelChange.mapField;

        /*buttonScripts = GetComponentsInChildren<LevelButton>();*/
        int i = 0;
        foreach(var map in fieldScripts)
        {

            map.mapNum = i;
            i++;
        }
    
        MapField.SetPanel(this);

        transform.Rotate(new Vector3(0.0f, 0.0f, rot));

        //tweenとかで出現エフェクト等
        transform.localScale = new Vector3(popScale,popScale,popScale);
        transform.DOScale(1.0f, popTime).OnComplete(() => { levelController.LevelState = _eLevelState.LIE; });
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
        transform.DOScale(popScale, popTime)
            .OnComplete(levelController.EndComplete);
    }

    public void ChangeText(int stage)
    {
        fieldText.text = levelController.GetFieldName(stage);
    }

}
