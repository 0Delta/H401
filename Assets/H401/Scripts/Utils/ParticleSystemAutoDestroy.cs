using UnityEngine;
using System.Collections;

public class ParticleSystemAutoDestroy  : MonoBehaviour {
    private ParticleSystem ps;

    void Start () {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update () {
        if(ps) {
            if(!ps.IsAlive()) {
                Destroy(gameObject);
            }
        }
    }
}
