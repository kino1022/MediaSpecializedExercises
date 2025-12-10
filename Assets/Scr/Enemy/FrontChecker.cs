using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Enemy {
    public class FrontChecker : SerializedMonoBehaviour {

        [SerializeField]
        [LabelText("確認用レイの始点")]
        private Transform _rayPosition;

        [SerializeField]
        [LabelText("レイの半径")]
        private float _rayRadius;

        private IObjectResolver _resolver;

        private IEnemyMovementHolder _movement;

        private RaycastHit _hit;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            _movement = _resolver.Resolve<IEnemyMovementHolder>();
        }

        private void FixedUpdate() {
            if (CheckFront()) {
                transform.localScale = new Vector3(_movement.Movement.x, 1.0f, 1.0f);
                _movement.SetMovement(_movement.Movement * -1.0f);
            }
        }

        private bool CheckFront() {
            
            Vector3 frontDir = _rayPosition.position.x - transform.position.x > 0 ? Vector3.right : Vector3.left;

            return Physics.SphereCast(
                _rayPosition.position,
                _rayRadius,
                frontDir,
                out _hit,
                _rayRadius / 2.0f,
                LayerMask.GetMask("Default"),
                QueryTriggerInteraction.Ignore
            );
        }
    }
}