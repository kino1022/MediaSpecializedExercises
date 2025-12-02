using MessagePipe;
using Scr.GameManager;
using Scr.Utility;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;

namespace Scr.FireBall {
    public class FireBallController : SerializedMonoBehaviour {
        
        [SerializeField]
        private Vector3 _movePower = Vector3.zero;

        [SerializeField]
        [LabelText("最大高度")]
        private float _maxHeight = 1.0f;
        
        private Rigidbody _rigidbody;
        
        private IPublisher<TakeDamageEventBus> _publisher;

        private void Start() {
            _rigidbody = gameObject.GetComponentFromWhole<Rigidbody>();
            
            _rigidbody.AddForce(_movePower, ForceMode.Impulse);

            //以下禁術
            var manager = GameObject.FindAnyObjectByType(typeof(GameManagerLifetimeScope));

            _publisher = manager
                .GetComponent<GameManagerLifetimeScope>()
                .Container
                .Resolve<IPublisher<TakeDamageEventBus>>();

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

        public void SetMovement(float movement) {
            _rigidbody ??= gameObject.GetComponentFromWhole<Rigidbody>();
            _rigidbody.AddForce(_movePower * movement, ForceMode.Impulse);
        }

        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.layer == 7) {
                _publisher.Publish(new TakeDamageEventBus(1, collision.gameObject));
            }
        }
    }
}