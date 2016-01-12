using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour {

    [SerializeField]private string levelControllerPath = null;
    [SerializeField]private string ojityanPath = null;
    [SerializeField]private string gameInfoCanvasPath = null;
    [SerializeField]private string gamePausePath = null;

    private GameObject levelControllerObject = null;
    private GameObject ojityanObject = null;
    private GameObject gameInfoCanvasObject = null;
    private GameObject gamePauseObject = null;

    private LevelController _levelController = null;
    private GameInfoCanvas  _gameInfoCanvas = null;
    private GameOption      _gamePause = null;
    private Animator        _ojityanAnimator = null;

    public LevelController  levelCotroller { get { return _levelController; } }
    public GameInfoCanvas   gameInfoCanvas { get { return _gameInfoCanvas; } }
    public GameOption       gamePause { get { return  _gamePause; } }
    public Animator         ojityanAnimator { get { return _ojityanAnimator; } }

	// Use this for initialization
	void Start () {
        levelControllerObject   = Instantiate(Resources.Load<GameObject>(levelControllerPath));
        ojityanObject           = Instantiate(Resources.Load<GameObject>(ojityanPath));
        gameInfoCanvasObject    = Instantiate(Resources.Load<GameObject>(gameInfoCanvasPath));
        gamePauseObject         = Instantiate(Resources.Load<GameObject>(gamePausePath));

        levelControllerObject.transform.SetParent(transform);
        ojityanObject.transform.SetParent(transform);    
        gameInfoCanvasObject.transform.SetParent(transform);
        gamePauseObject.transform.SetParent(transform);

        _levelController = levelControllerObject.GetComponent<LevelController>();
        _gameInfoCanvas = gameInfoCanvasObject.GetComponent<GameInfoCanvas>();
        _gamePause = gamePauseObject.GetComponent<GameOption>();
        _ojityanAnimator = ojityanObject.GetComponent<Animator>();
	}
}
