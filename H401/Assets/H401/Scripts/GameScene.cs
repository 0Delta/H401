using UnityEngine;
using System.Collections;

public class GameScene : MonoBehaviour {

    [SerializeField]private string gameControllerPath;
    [SerializeField]private string eventSystemPath;
    [SerializeField]private string gameUIPath;
    [SerializeField]private string levelTablePath;
    [SerializeField]private string mainCameraPath;

    private GameObject gameControllerObject;
    private GameObject eventSystemObject;
    private GameObject gameUIObject;
    private GameObject levelTableObject;
    private GameObject mainCameraObject;

    private GameController      _gameController;
    private GameUI              _gameUI;
    private LevelTables         _levelTables;
    private Camera              _mainCamera;

    public GameController   gameController { get { return _gameController; } }
    public GameUI           gameUI { get { return _gameUI; } }
    public LevelTables      levelTables { get { return _levelTables; } }
    public Camera mainCamera { get { return _mainCamera; } }



	// Use this for initialization
	void Start () {
        gameControllerObject    = Resources.Load<GameObject>(gameControllerPath);
        eventSystemObject       = Resources.Load<GameObject>(eventSystemPath);
        gameUIObject            = Resources.Load<GameObject>(gameUIPath);
        levelTableObject        = Resources.Load<GameObject>(levelTablePath);
        mainCameraObject        = Resources.Load<GameObject>(mainCameraPath);

        gameControllerObject.transform.SetParent(transform);
        eventSystemObject.transform.SetParent(transform);
        gameUIObject.transform.SetParent(transform);
        levelTableObject.transform.SetParent(transform);
        mainCameraObject.transform.SetParent(transform);

        _gameController = gameControllerObject.GetComponent<GameController>();
        _gameUI = gameUIObject.GetComponent<GameUI>();
        _levelTables = levelTableObject.GetComponent<LevelTables>();
        _mainCamera = mainCameraObject.GetComponent<Camera>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
