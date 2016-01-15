using UnityEngine;
using System.Collections;

public class OptionReturnButton : MonoBehaviour {
    
    public void OnClick() {
        transform.parent.GetComponent<OptionManager>().ReturnTitleScene();
    }
}
