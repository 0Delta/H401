using UnityEngine;
using System.Collections;

public class MapField : MonoBehaviour {

    static private LevelPanel levelPanel;
    static public void SetPanel(LevelPanel panel) { levelPanel = panel; }

    private int _mapNum = 0;
    public int mapNum { get { return _mapNum; } set { _mapNum = value; } }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
