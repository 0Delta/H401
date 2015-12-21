using UnityEngine;
using System.Collections;
using DG.Tweening;

public class fieldImage : MonoBehaviour {
    [SerializeField]private float rotDuration = 0.0f;
	// Use this for initialization
	void Start () {
        gameObject.transform.DORotate(new Vector3(0.0f,0.0f,-90.0f),rotDuration / 2.0f).OnComplete(RotTurn);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    void RotTurn()
    {
        gameObject.transform.DORotate(new Vector3(0.0f, 0.0f, transform.localEulerAngles.z > 180 ? 90 : -90),rotDuration).OnComplete(RotTurn);
    }
}
