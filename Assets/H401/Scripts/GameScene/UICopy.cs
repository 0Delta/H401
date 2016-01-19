using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UICopy : MonoBehaviour {
    private Image feverBar;
    private Image timeBar;
    private LimitTime lTimeScript;
    private FeverGauge feverScript;
	// Use this for initialization
	void Start () {
        feverBar = transform.FindChild("feverFrame").FindChild("feverBar").gameObject.GetComponent<Image>();
        timeBar = transform.FindChild("timeFrame").FindChild("timeBar").gameObject.GetComponent<Image>();

        GameInfoCanvas giCanvas = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameUI.gameInfoCanvas;
        lTimeScript = giCanvas.limitTime;
        feverScript = giCanvas.feverGauge;
        transform.FindChild("ScoreNum").GetComponent<Text>().text = giCanvas.score.GameScore.ToString();

    }

    // Update is called once per frame
    void Update () {
	    timeBar.fillAmount = lTimeScript.lastRate;
        feverBar.fillAmount = feverScript.feverVal;
	}
}
