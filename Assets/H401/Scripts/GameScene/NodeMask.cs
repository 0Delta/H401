using UnityEngine;

public class NodeMask : MonoBehaviour {
    SpriteRenderer sr = null;

    // Use this for initialization
    void Awake () {
        sr = GetComponent<SpriteRenderer>();
    }
    public bool Enabled {
        set { sr.enabled = value; }
    }
    public void SetSprite(Sprite sp)
    {
        sr.sprite = sp;
    }
}
