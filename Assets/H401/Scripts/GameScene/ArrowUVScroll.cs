using UnityEngine;
using System.Collections;

public class ArrowUVScroll : MonoBehaviour {

    [SerializeField] private float scrollSpd = 0.0f;    // スクロール速度(0～1の移動量)

    private Material material;
    private Vector2  offset;
    private GameOption gameOption;
	// Use this for initialization
	void Start () {
	    material = GetComponent<SpriteRenderer>().sharedMaterial;
        offset = Vector2.zero;

        gameOption = transform.root.gameObject.GetComponent<AppliController>().GetCurrentScene().GetComponent<GameScene>().gameUI.gamePause;


    }

    // Update is called once per frame
    void Update () {
        if (gameOption.IsPause)
            return;
        offset.y -= scrollSpd;
        if(offset.y < -1.0f)
            offset.y += 2.0f;

	    material.SetTextureOffset("_MainTex", offset);
	}

    void OnApplicationQuit() {
	    material.SetTextureOffset("_MainTex", Vector2.zero);
    }
}
