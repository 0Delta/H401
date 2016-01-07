using UnityEngine;
using System.Collections;

public class LevelChange : MonoBehaviour {


    [SerializeField]private string fieldMapPath = null;
    [SerializeField]private string subCameraPath = null;
    [SerializeField]private string levelPanelPath = null;
    [SerializeField]private Color normalColor;

    private LevelController _levelController;
    public LevelController levelController { get { return _levelController; } set { _levelController = value; } }
    private MapField[] _mapFields;
    public MapField[] mapField { get { return _mapFields; } }
    private LevelPanel _levelPanel;
    public LevelPanel levelPanel { get { return _levelPanel; } }
    private Camera _subCamera;
    public Camera subCamera { get { return _subCamera; } }

	// Use this for initialization
    void Start()
    {
        Instantiate(Resources.Load<GameObject>(fieldMapPath)).transform.SetParent(transform);
        GameObject sCamera = Instantiate(Resources.Load<GameObject>(subCameraPath));//.transform.SetParent(transform);
        _subCamera = sCamera.GetComponent<Camera>(); 
        GameObject lPanel = Instantiate(Resources.Load<GameObject>(levelPanelPath));

        lPanel.transform.SetParent(transform);
        subCamera.transform.SetParent(transform);
        subCamera.transform.Rotate(new Vector3(0.0f,0.0f,-_levelController.LyingAngle));

        _mapFields = GetComponentsInChildren<MapField>();
        _levelPanel = lPanel.GetComponentInChildren<LevelPanel>();
        _levelPanel.SetLevelController(_levelController);

        LevelCanvas lCanvas = lPanel.GetComponent<LevelCanvas>();
        //lCanvas.SetCamera(subCamera.GetComponent<Camera>(),_levelController.LyingAngle);

        //mapfieldから各メッシュをエミッションでちかちかさせる
        int i = 0;
        foreach(var mf in _mapFields)
        {
            if (i == transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameController.nodeController.currentLevel)
                mf.SetColor();
            else
                mf.SetColor(normalColor);
            mf.mapNum = i;
            i++;
        }
    }
	// Update is called once per frame
	void Update () {
        
	}
    public void Delete()
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        gameScene.mainCamera.enabled = true;
        _levelController.EndComplete();
        
    }

    public void ChangeMapColor(int highLightNum)
    {
        int n = 0;
        foreach(var mf in _mapFields)
        {
            if (n == highLightNum)
            {
                n++;
                continue;
            }
            mf.SetColor(normalColor);
            n++;
        }
    }
}
