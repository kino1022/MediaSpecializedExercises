using MessagePipe;
using Scr.GameManager;
using Scr.Player.Action;
using Scr.Utility;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using VContainer;

namespace Scr.Player {
    
    public struct OnDeadEventBus {
    }
    
    public class Player : SerializedMonoBehaviour {
        
        [Title("踏みつけ判定")] 
        
        [SerializeField] 
        [LabelText("判定オフセット")]
        private float _trampleOffeSet = 0.2f;

        [SerializeField]
        [LabelText("踏みつけダメージ")]
        private int _trampleDamage = 1;

        private IObjectResolver _resolver;
        
        private IPublisher<OnDeadEventBus> _deadPublisher;
        
        private IPublisher<TakeDamageEventBus> _damagePublisher;
        
        private IPublisher<GetCoinEventBus> _getcoinPublisher;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            _damagePublisher = _resolver.Resolve<IPublisher<TakeDamageEventBus>>();
            _deadPublisher = _resolver.Resolve<IPublisher<OnDeadEventBus>>();
            _getcoinPublisher = _resolver.Resolve<IPublisher<GetCoinEventBus>>();
            var cam = FindAnyObjectByType<CinemachineCamera>();
            if (cam is not null) {
                cam.Follow = transform;
            }
        }

        public void Die() {
            _deadPublisher.Publish(new OnDeadEventBus());
        }

        public void GetCoin() {
            _getcoinPublisher ??= _resolver.Resolve<IPublisher<GetCoinEventBus>>();
            _getcoinPublisher.Publish(new GetCoinEventBus());
        }

        private void OnCollisionEnter(Collision other) {
            if (other.gameObject.layer == 7) {
                Vector3 point = other.contacts[0].point;
                point -= new Vector3(transform.position.x, transform.position.y + _trampleOffeSet, transform.position.z);
                if (point.y <= 0) {
                    _damagePublisher.Publish(new TakeDamageEventBus(_trampleDamage, other.gameObject));
                    var jump = GetComponentsInChildren<JumpActionBehaviour>();
                }
            }
        }
    }
}