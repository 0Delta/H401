using UnityEngine;
using System.Collections;

public class tset : MonoBehaviour {
    
    public float effectMoveDurationTime = 0.0f;
    public float effectMoveWaitTime = 0.0f;

    private float courceTime = 0.0f;
    private float startSize;

    void Start() {
        startSize = transform.GetComponent<ParticleSystem>().startSize;
    }

    void Update() {
        ParticleSystem ps = transform.GetComponent<ParticleSystem>();

        if(courceTime > effectMoveDurationTime) {
            float time = courceTime - effectMoveDurationTime;
            ps.startSize = Mathf.Lerp(startSize, 0.0f, time / effectMoveWaitTime);
        }

        courceTime += Time.deltaTime;
        
        if(courceTime > effectMoveDurationTime + effectMoveWaitTime) {
            courceTime = 0.0f;
            ps.startSize = startSize;
        }
    }
}
