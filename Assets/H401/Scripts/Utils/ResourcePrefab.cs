using UnityEngine;
using System.Collections;

namespace ResourcePrefab {
    [System.Serializable]
    public class RPrefab {
        [SerializeField] public string Name = null;
        private GameObject obj = null;
        private bool Ready = true;

        public GameObject Get() {
            if(!Ready) {
                return null;
            }

            if(obj == null) {
                obj = Resources.Load<GameObject>(Name);
            }

            if(obj == null) {
                Ready = false;
                Debug.LogError("Failed Load Prefab : " + Name);
            }            
            return obj;
        }
        
    }
}