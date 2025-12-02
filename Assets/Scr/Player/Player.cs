using MessagePipe;
using Scr.Player.Action;
using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Player {
    public class Player : SerializedMonoBehaviour {
        
        [Title("踏みつけ判定")] 
        
        [SerializeField] 
        [LabelText("判定オフセット")]
        private float _trampleOffeSet = 0.2f;

        [SerializeField]
        [LabelText("踏みつけダメージ")]
        private int _trampleDamage = 1;

        private IObjectResolver _resolver;
        
        private IPublisher<TakeDamageEventBus> _damagePublisher;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            _damagePublisher = _resolver.Resolve<IPublisher<TakeDamageEventBus>>();
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