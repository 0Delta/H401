using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine.EventSystems;

public class TitleRenderTexture : MonoBehaviour {

    private TitleScene titleSceneScript;

	// Use this for initialization
	void Start () {
	    titleSceneScript = transform.parent.GetComponent<TitleScene>();
        
        Observable
            .EveryUpdate()
            .Where(_ => Input.GetMouseButtonDown(0))
            .Where(_ => titleSceneScript.isPopupScene)
            .Subscribe(_ => {
                // uGUI に当たっているか
                PointerEventData pointer = new PointerEventData(EventSystem.current);
                pointer.position = Input.mousePosition;
                List<RaycastResult> result = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, result);

                if (result.Count > 0)
                    return;

                // RenderTexture に当たっているか
                RaycastHit hit;
                if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
                    titleSceneScript.ReturnTitleScene();
                }
            })
            .AddTo(this.gameObject);
	}
}
