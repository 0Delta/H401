using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
public class fieldImage : MonoBehaviour {
    [SerializeField]private float rotDuration = 0.0f;
	// Use this for initialization
	void Start () {
        transform.DOLocalRotate(new Vector3(0.0f,0.0f,-90.0f),rotDuration * 0.5f)/*.OnComplete(RotTurn)*/.SetLoops(-1,LoopType.Incremental);
        GetComponent<Button>().onClick.AddListener(transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameUI.levelCotroller.TouchChange);

	}

}
