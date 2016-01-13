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
        GameObject scCanbas = Instantiate(giCanvas.score.scoreCanvas.gameObject);

        scCanbas.transform.SetParent(transform);

        scCanbas.transform.rotation = transform.FindChild("ScoreText").rotation;
        scCanbas.transform.position = transform.FindChild("ScoreText").position  + new Vector3(3.0f,0.0f,0.0f);
        scCanbas.transform.localPosition -= new Vector3(scCanbas.GetComponentInChildren<RectTransform>().offsetMax.x * 0.5f, 0.0f, 0.0f);

        scCanbas.transform.localScale = new Vector3(-0.3f,0.3f,0.3f);

    }

    // Update is called once per frame
    void Update () {
	    timeBar.fillAmount = lTimeScript.lastRate;
        feverBar.fillAmount = feverScript.feverVal;
	}
}
