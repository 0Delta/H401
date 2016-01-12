using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class LevelButton : MonoBehaviour {

    static private LevelPanel levelPanel;
    static public void SetPanel(LevelPanel panel) { levelPanel = panel; }

    private int levelNumber;
    public void RegistLevelNumber(int level){levelNumber = level;} 

	// Use this for initialization
	void Start () {
        GetComponent<Button>().onClick.AddListener(SetLevel);
	}
	
    void SetLevel()
    {
        levelPanel.NextLevel = levelNumber;
        print("レベル選択：" + levelNumber.ToString());
        levelPanel.ChangeText(levelNumber);
        //色変えとかここに
    }
}
