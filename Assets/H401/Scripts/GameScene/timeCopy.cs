using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class timeCopy : MonoBehaviour {
        private Image timeImage;
        private LimitTime lTimeScript;
	// Use this for initialization
	void Start () {
                timeImage = GetComponent<Image>();
                lTimeScript = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameUI.gameInfoCanvas.limitTime;

        }

        // Update is called once per frame
        void Update () {
                timeImage.fillAmount = lTimeScript.lastRate;
	}
}
