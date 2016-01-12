using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

    [SerializeField]private string nodeControllerPath = null;
    
    private GameObject nodeControllerObject = null;
    private NodeController _nodeController = null;

    public NodeController nodeController { get { return _nodeController; } }

	// Use this for initialization
	void Start () {
        nodeControllerObject = Instantiate(Resources.Load<GameObject>(nodeControllerPath));
        _nodeController = nodeControllerObject.GetComponent<NodeController>();
        nodeControllerObject.transform.SetParent(transform);
	}
}
