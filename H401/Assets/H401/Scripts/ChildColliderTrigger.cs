using UnityEngine;
using System.Collections;

public class ChildColliderTrigger : MonoBehaviour
{
    GameObject parent;

    // Use this for initialization
    void Start()
    {
        parent = gameObject.transform.parent.gameObject;
    }

    void OnTriggerEnter(Collider collider)
    {
        parent.SendMessage("RedirectedOnTriggerEnter", collider);
    }

    void OnTriggerStay(Collider collider)
    {
        parent.SendMessage("RedirectedOnTriggerStay", collider);
    }
}