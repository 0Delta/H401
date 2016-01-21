using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
public class ButtonHighLight : MonoBehaviour {

    private Image image;
	// Use this for initialization
	void Start () {

        image = GetComponent<Image>();
        ScaleMove(true);
	}

    void ScaleMove(bool bFade)
    {
        float fEnd = bFade ? 0.5f : 1.0f;
        image.DOFade( fEnd,1.0f).SetEase(Ease.Linear).SetUpdate(true)
            .OnComplete(() => 
            {
                ScaleMove(!bFade);
            });
    }

    void OnDestroy()
    {
        image.DOKill();
    }
}
