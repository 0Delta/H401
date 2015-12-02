using UnityEngine;
using System.Collections;

public class GameScene : MonoBehaviour {

    [SerializeField]private string gameControllerPath;
    [SerializeField]private string eventSystemPath;
    [SerializeField]private string gameUIPath;
    [SerializeField]private string levelTablePath;
    [SerializeField]private string mainCameraPath;
    [SerializeField]private string lightPath;

    private GameObject gameControllerObject;
    private GameObject eventSystemObject;
    private GameObject gameUIObject;
    private GameObject levelTableObject;
    private GameObject mainCameraObject;
    private GameObject lightObject;

    private GameController      _gameController;
    private GameUI              _gameUI;
    private LevelTables         _levelTables;
    private Camera              _mainCamera;

    public GameController   gameController { get { return _gameController; } }
    public GameUI           gameUI { get { return _gameUI; } }
    public LevelTables      levelTables { get { return _levelTables; } }
    public Camera mainCamera { get { return _mainCamera; } }

    void Awake()
    {
  
    }

	// Use this for initialization
	void Start () {
        levelTableObject =  Instantiate(Resources.Load<GameObject>(levelTablePath));
        levelTableObject.transform.SetParent(transform);   
        _levelTables = levelTableObject.GetComponent<LevelTables>();


        mainCameraObject =  Instantiate(Resources.Load<GameObject>(mainCameraPath));
        mainCameraObject.transform.SetParent(transform);
        _mainCamera = mainCameraObject.GetComponent<Camera>();

        lightObject = Instantiate(Resources.Load<GameObject>(lightPath));
        lightObject.transform.SetParent(transform);
        

        eventSystemObject =  Instantiate(Resources.Load<GameObject>(eventSystemPath));
        eventSystemObject.transform.SetParent(transform);

        gameUIObject =  Instantiate(Resources.Load<GameObject>(gameUIPath));
        gameUIObject.transform.SetParent(transform);
        _gameUI = gameUIObject.GetComponent<GameUI>();

        gameControllerObject = Instantiate(Resources.Load<GameObject>(gameControllerPath));
        _gameController = gameControllerObject.GetComponent<GameController>();
        gameControllerObject.transform.SetParent(transform);


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
