using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.FireBall {
    public class FireBallController : SerializedMonoBehaviour {
        
        [SerializeField]
        private Vector3 _movePower = Vector3.zero;

        [SerializeField]
        [LabelText("最大高度")]
        private float _maxHeight = 1.0f;
        
        private Rigidbody _rigidbody;

        private void Start() {
            _rigidbody = gameObject.GetComponentFromWhole<Rigidbody>();
            
            _rigidbody.AddForce(_movePower, ForceMode.Impulse);
        }

        private void FixedUpdate() {
            if (_rigidbody.linearVelocity.y > _maxHeight) {
                _rigidbody.linearVelocity = new Vector3(
                    _rigidbody.linearVelocity.x,
                    _maxHeight,
                    _rigidbody.linearVelocity.z
                    );
            }
        }
    }
}