using UnityEngine;
using System.Collections;

public class OptionManager : MonoBehaviour {

    [SerializeField] private FadeTime fadeTimes;

	// Use this for initialization
	void Start () {
	
	}

    public void ReturnTitleScene() {
        transform.parent.GetComponent<TitleScene>().ReturnTitleScene(AppliController._eSceneID.OPTION, fadeTimes.inTime, fadeTimes.outTime);
    }
}
