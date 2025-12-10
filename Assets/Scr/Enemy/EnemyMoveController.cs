using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scr.Enemy {


    public interface IEnemyMovementHolder {
        Vector3 Movement { get; }
        
        void SetMovement(Vector3 movement);
    }
    
    [RequireComponent(typeof(Rigidbody), typeof(Animator))]
    public class EnemyMoveController : SerializedMonoBehaviour, IEnemyMovementHolder {

        [SerializeField]
        [LabelText("初期移動方向")]
        private Vector3 _startMovement = Vector3.left;
        
        [SerializeField]
        [LabelText("最大移動速度")]
        private float _maxMoveSpeed = 5.0f;

        [SerializeField] 
        [LabelText("移動速度")]
        private float _moveSpeed = 2.0f;

        private IObjectResolver _resolver;

        private Rigidbody _rigidBody;

        private Animator _animator;
        
        public Vector3 Movement => _startMovement;

        public void SetMovement(Vector3 movement) => _startMovement = movement.normalized;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            
            // DI が提供されていない場合や Resolve に失敗した場合に備えてフォールバックを用意
            _resolver ??= gameObject.GetComponentFromWhole<LifetimeScope>()?.Container;

            if (_resolver != null) {
                // Resolve が null を返す可能性がある実装もあるため、戻り値が null なら GetComponent を使う
                _animator = _resolver.Resolve<Animator>() ?? GetComponent<Animator>();
                _rigidBody = _resolver.Resolve<Rigidbody>() ?? GetComponent<Rigidbody>();
            } else {
                _animator = GetComponent<Animator>();
                _rigidBody = GetComponent<Rigidbody>();
            }

            // 最終的にどちらも必須なので念のため警告を出す
            if (_rigidBody == null) {
                Debug.LogWarning($"[{nameof(EnemyMoveController)}] Rigidbody が見つかりません。コンポーネントが存在することを確認してください。", this);
            }

            if (_animator == null) {
                Debug.LogWarning($"[{nameof(EnemyMoveController)}] Animator が見つかりません。コンポーネントが存在することを確認してください.", this);
            }
            
        }

        private void FixedUpdate() {
            // null 安全
            if (_rigidBody == null) return;

            // Rigidbody の正しいプロパティは linearVelocity（velocity は非推奨）
            float speedLimit = _maxMoveSpeed - Mathf.Abs(_rigidBody.linearVelocity.x);
            
            if (speedLimit < 0) {
                _rigidBody.AddForce(_startMovement.normalized * _moveSpeed, ForceMode.Acceleration);
            }
            
            _rigidBody.AddForce(_moveSpeed * speedLimit * _startMovement.normalized, ForceMode.Force);
        }

    }
}