using UnityEngine;
using System.Collections;

public class GameScene : MonoBehaviour {

    [SerializeField]private string gameControllerPath;
    [SerializeField]private string eventSystemPath;
    [SerializeField]private string gameUIPath;
    [SerializeField]private string levelTablePath;
    [SerializeField]private string mainCameraPath;
    [SerializeField]private string lightPath;
    [SerializeField]private string probePath;

    private GameObject gameControllerObject;
    private GameObject eventSystemObject;
    private GameObject gameUIObject;
    private GameObject levelTableObject;
    private GameObject mainCameraObject;
    private GameObject lightObject;
    private GameObject probeObject;
    private AudioSource[] audioSources;

    private GameController      _gameController;
    private GameUI              _gameUI;
    private LevelTables         _levelTables;
    private Camera              _mainCamera;
//    private Light               _directionalLight;

    public GameController   gameController { get { return _gameController; } }
    public GameUI           gameUI { get { return _gameUI; } }
    public LevelTables      levelTables { get { return _levelTables; } }
    public Camera           mainCamera { get { return _mainCamera; } }
//    public Light            directionalLight{get{return _directionalLight;}}

    public enum _eGameSceneBGM
    {
        GAME,
        FEVER,
    }

	// Use this for initialization
	void Start () {
        levelTableObject =  Instantiate(Resources.Load<GameObject>(levelTablePath));
        levelTableObject.transform.SetParent(transform);   
        _levelTables = levelTableObject.GetComponent<LevelTables>();
        
        mainCameraObject =  Instantiate(Resources.Load<GameObject>(mainCameraPath));
        mainCameraObject.transform.SetParent(transform);
        _mainCamera = mainCameraObject.GetComponent<Camera>();

        probeObject = Instantiate(Resources.Load<GameObject>(probePath));
        probeObject.transform.SetParent(transform);

        lightObject = Instantiate(Resources.Load<GameObject>(lightPath));
        lightObject.transform.SetParent(transform);
        //_directionalLight = lightObject.GetComponent<Light>();
        
        eventSystemObject =  Instantiate(Resources.Load<GameObject>(eventSystemPath));
        eventSystemObject.transform.SetParent(transform);

        gameUIObject =  Instantiate(Resources.Load<GameObject>(gameUIPath));
        gameUIObject.transform.SetParent(transform);
        _gameUI = gameUIObject.GetComponent<GameUI>();

        gameControllerObject = Instantiate(Resources.Load<GameObject>(gameControllerPath));
        _gameController = gameControllerObject.GetComponent<GameController>();
        gameControllerObject.transform.SetParent(transform);

        audioSources = GetComponents<AudioSource>();
	}

    public void PlayBGM(_eGameSceneBGM bgmNum) {
        audioSources[(int)bgmNum].Play();
    }
    
    public void StopBGM(_eGameSceneBGM bgmNum) {
        audioSources[(int)bgmNum].Stop();
    }
}
