using UnityEngine;
using System.Collections;

public class GameInfoCanvas : MonoBehaviour {

    [SerializeField]private string gameInfoPanelPath = null;

    private GameObject gameInfoPanelObject = null;

    private GameInfoPanel _gameInfoPanel = null;

    public GameInfoPanel gameInfoPanel {get {return _gameInfoPanel;}}


	// Use this for initialization
	void Start () {
        gameInfoPanelObject = Resources.Load<GameObject>(gameInfoPanelPath);

        _gameInfoPanel = gameInfoPanelObject.GetComponent<GameInfoPanel>();

        gameInfoPanelObject.transform.SetParent(transform);
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
