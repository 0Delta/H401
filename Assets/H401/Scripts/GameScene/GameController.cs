using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]private string nodeControllerPath = null;
    
    private GameObject nodeControllerObject = null;
    private NodeController _nodeController = null;

    public NodeController nodeController { get { return _nodeController; } }

    private GameObject _frameControllerObject = null;
    public GameObject frameControllerObject {
        get {
            if (_frameControllerObject == null)
                _frameControllerObject = transform.FindChild("FrameController").gameObject;
            return _frameControllerObject;
        }
    }

    private GameObject _arrowControllerObject = null;
    public GameObject arrowControllerObject {
        get {
            if (_arrowControllerObject == null)
                _arrowControllerObject = transform.FindChild("ArrowController").gameObject;
            return _arrowControllerObject;
        }
    }
	// Use this for initialization
	void Start () {
        nodeControllerObject = Instantiate(Resources.Load<GameObject>(nodeControllerPath));
        _nodeController = nodeControllerObject.GetComponent<NodeController>();
        nodeControllerObject.transform.SetParent(transform);

        //_arrowControllerObject = transform.FindChild("ArrowController").gameObject;
        //_frameControllerObject = transform.FindChild("FrameController").gameObject;
	}
}
