using UnityEngine;
using System.Collections;

public class CreditReturnButton : MonoBehaviour {
    
    public void OnClick() {
        transform.parent.GetComponent<CreditManager>().ReturnTitleScene();
    }
}
