using UnityEngine;
using System.Collections;

public class LevelButton : MonoBehaviour {

    static private LevelMap levelMap;

    private int levelNumber;
    public void RegistLevelNumber(int level){levelNumber = level;} 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void SetLevelMap(LevelMap map) { levelMap = map; }

    void OnMouseDown()
    {
        levelMap.NextLevel = levelNumber;

        //色変えとかここに
    }
}
