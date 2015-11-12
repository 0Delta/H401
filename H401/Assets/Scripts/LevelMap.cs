using UnityEngine;
using System.Collections;

public class LevelMap : MonoBehaviour {

    private LevelTables levelTables;
    public LevelTables LevelTables
    {
        set { levelTables = value; }
    }

    private LevelController levelController;
    public void SetLevelController(LevelController lev) { levelController = lev; }

    //ボタン用配列
    private GameObject[] buttonObjects;
    private LevelButton[] buttonScripts;
    public int NextLevel
    {
        set { levelController.NextLevel = value; }
    }


    [SerializeField] GameObject buttonPrefab = null;

    void Awake()
    {
        //ここのマジックナンバーはどうにかしたいが
        buttonObjects = new GameObject[5];
        buttonScripts = new LevelButton[5];
    }

	// Use this for initialization
    void Start()
    {
        levelController.NextLevel = -1;

        //ボタンを並べてリンクを付ける
        Vector3 pos = transform.position;
        for(int i = 0 ; i < 5 ; i ++)
        {
            pos.y = 42.0f * ((i % 2) == 0 ? 1.0f : -1.0f);
            pos.x += -100.0f + 50.0f * i;

            buttonObjects[i] = (GameObject)Instantiate(buttonPrefab, pos, transform.rotation);
            buttonObjects[i].transform.parent = this.transform;
            buttonScripts[i] = buttonObjects[i].GetComponent<LevelButton>();

            //その他の設定

        }
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void RegistLevel()
    {
        
    }

    //傾きを
}
