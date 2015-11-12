using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DebugButton : MonoBehaviour {

    [SerializeField]static private LevelController levelController = null;
    [SerializeField]private _eDebugState lr;

	// Use this for initialization
	void Start () {
        if (levelController == null)
            levelController = GameObject.Find("levelController").GetComponent<LevelController>();
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnClick()
    {
        switch(lr)
        {
            case _eDebugState.RIGHT_LIE:
                levelController.FChangeTest(90);
                break;
            case _eDebugState.LEFT_LIE:
                levelController.FChangeTest(-90);
                break;
            case _eDebugState.RETURN:
                levelController.FChangeEnd();
                break;
        }
    }

    public void SetType(_eDebugState type)
    {
        lr = type;

        switch(type)
        {
            case _eDebugState.LEFT_LIE:
                GetComponent<Button>().onClick.AddListener(() => { levelController.FChangeTest(90); });
                break;
            case _eDebugState.RIGHT_LIE:
                GetComponent<Button>().onClick.AddListener(() => { levelController.FChangeTest(-90); });
                break;
            case _eDebugState.RETURN:
                GetComponent<Button>().onClick.AddListener(() => { levelController.FChangeEnd(); });
                break;
        }
    }
}
