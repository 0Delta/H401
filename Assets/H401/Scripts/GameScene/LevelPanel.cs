using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class LevelPanel : MonoBehaviour {

    [SerializeField]private float popTime = 0.0f;
    [SerializeField]private float popScale = 0.0f;
    private LevelController levelController;
    public void SetLevelController(LevelController lev) { levelController = lev; }
    
    public int NextLevel
    {
        set { levelController.NextLevel = value; }
    }

    private MapField[] fieldScripts;
    private Image[] fieldImage;
    
	// Use this for initialization
    void Start()
    {
        //このあたりをコントローラ側からセットするように
        levelController.NextLevel = -1;
        float rot = levelController.LyingAngle;

        fieldImage = new Image[4];
        Transform fBG1 = gameObject.transform.FindChild("fieldBG1");
        fieldImage[0] = fBG1.FindChild("easy").GetComponent<Image>();
        fieldImage[1] = fBG1.FindChild("normal").GetComponent<Image>();
        fieldImage[2] = fBG1.FindChild("hard").GetComponent<Image>();
        fieldImage[3] = fBG1.FindChild("veryhard").GetComponent<Image>();

        //ボタンを並べてリンクを付ける
        //ボタンからメッシュデータに変更されました
        fieldScripts = levelController.levelChange.mapField;

        int i = 0;
        foreach(var map in fieldScripts)
        {
            map.mapNum = i;
            i++;
        }

        fieldImage[transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameController.nodeController.currentLevel].color = Color.yellow;
        MapField.SetPanel(this);

        transform.Rotate(new Vector3(0.0f, 0.0f, rot));
        transform.localScale = Vector3.one;

 //       Transform fBG1 = transform.FindChild("fieldBG1");
        fBG1.localPosition += new Vector3(-50.0f, 0.0f, 0.0f);
        fBG1.DOLocalMoveX(fBG1.localPosition.x + 50.0f ,popTime);

        Transform fBG2 = transform.FindChild("fieldBG2");
        fBG2.localPosition += new Vector3(50.0f, 0.0f, 0.0f);
        fBG2.DOLocalMoveX(fBG2.localPosition.x - 50.0f, popTime); ;
    }
	
    public void Delete()
    {
        transform.DOScale(popScale, popTime)
            .OnComplete(levelController.EndComplete);
    }
    
    public void ChangeText(int stage)
    {
        for (int i = 0; i < 4; i++)
            fieldImage[i].color = i == stage ? Color.yellow : Color.white;
    }
}
