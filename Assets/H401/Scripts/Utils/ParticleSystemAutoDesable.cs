using UnityEngine;
using System.Collections;

public class ParticleSystemAutoDesable  : MonoBehaviour {
    private ParticleSystem ps;

    void Start () {
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update () {
        if(ps) {
            if(!ps.IsAlive()) {
                gameObject.SetActive(false);
            }
        }
    }
}
