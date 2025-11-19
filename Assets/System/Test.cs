using UnityEngine;

namespace System {
    public class Test : MonoBehaviour {

        [SerializeField]
        private float _moveForce = 0.3f;
        
        private void Update() {
            var next = transform.position;
            next.x += _moveForce * Time.deltaTime;
            transform.position = next;
        }
    }
}