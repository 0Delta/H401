using UnityEngine;
using System.Collections;

public class ArrowUVScroll : MonoBehaviour {

    [SerializeField] private float scrollSpd = 0.0f;    // スクロール速度(0～1の移動量)

    private Material material;
    private Vector2  offset;

	// Use this for initialization
	void Start () {
	    material = GetComponent<SpriteRenderer>().sharedMaterial;
        offset = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
        offset.y -= scrollSpd;
        if(offset.y < -1.0f)
            offset.y += 2.0f;

        print(offset);

	    material.SetTextureOffset("_MainTex", offset);
	}
}
