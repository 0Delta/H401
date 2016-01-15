using UnityEngine;
using System.Collections;

public class HowToPlayReturnButton : MonoBehaviour {
    
    public void OnClick() {
        transform.parent.GetComponent<HowToPlayManager>().ReturnTitleScene();
    }
}
