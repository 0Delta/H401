using UnityEngine;
using System.Collections;

public class ReturnButton : MonoBehaviour {
    
    public void OnClick() {
        transform.root.GetComponent<AppliController>().GetCurrentScene().GetComponent<TitleScene>().ReturnTitleScene();
    }
}
