using UnityEngine;
using System.Collections;
using DG.Tweening;

public class LevelChange : MonoBehaviour {


    [SerializeField]private string fieldMapPath = null;
    [SerializeField]private string levelPanelPath = null;
    [SerializeField]private string mapArrowPath = null;
    [SerializeField]private Color normalColor;
    private LevelController _levelController;
    public LevelController levelController { get { return _levelController; } set { _levelController = value; } }
    private MapField[] _mapFields;
    public MapField[] mapField { get { return _mapFields; } }
    private LevelPanel _levelPanel;
    public LevelPanel levelPanel { get { return _levelPanel; } }
    private GameObject mapArrowObj = null;

	// Use this for initialization
    void Start()
    {
        Instantiate(Resources.Load<GameObject>(fieldMapPath)).transform.SetParent(transform);
        GameObject lPanel = Instantiate(Resources.Load<GameObject>(levelPanelPath));

        lPanel.transform.SetParent(transform);

        _mapFields = GetComponentsInChildren<MapField>();
        _levelPanel = lPanel.GetComponentInChildren<LevelPanel>();
        _levelPanel.SetLevelController(_levelController);

        mapArrowObj = Instantiate(Resources.Load<GameObject>(mapArrowPath));
        mapArrowObj.transform.SetParent(transform);
        SetArrowRot();

        //mapfieldから各メッシュをエミッションでちかちかさせる
        int i = 0;
        int cLevel = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameController.nodeController.currentLevel;
        foreach(var mf in _mapFields)
        {
            if (i == cLevel)
                mapArrowObj.transform.position = mf.gameObject.transform.position + new Vector3(0.0f, 1.0f, 0.0f);

            mf.mapNum = i;
            mf.SetColor(true);
            i++;
        }
    }

    void OnDestroy()
    {
        mapArrowObj.transform.DOKill();
    }

    public void Delete()
    {
        GameScene gameScene = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>();
        gameScene.mainCamera.enabled = true;
        _levelController.EndComplete();
    }

    public void SetArrowPos(int selectedNum)
    {
        //Vector3 vBounds =  _mapFields[selectedNum].gameObject.GetComponent<MeshRenderer>().bounds.size;
        mapArrowObj.transform.position = _mapFields[selectedNum].transform.position + new Vector3(0.0f, 1.0f, 0.0f);// + Vector3.Normalize(Camera.main.transform.position - _mapFields[selectedNum].transform.position) * 2.0f + new Vector3(0.0f,1.0f,0.0f);

    }
    public void SetArrowRot()
    {
        mapArrowObj.transform.FindChild("arrow").transform.DORotate(transform.rotation.eulerAngles + new Vector3(0.0f, 30.0f, 0.0f), 0.3f,RotateMode.WorldAxisAdd).SetEase(Ease.Linear).OnComplete( () => { SetArrowRot(); });
    }
}
