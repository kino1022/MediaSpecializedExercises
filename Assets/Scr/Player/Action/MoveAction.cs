using RinaInput.Controller.Module;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using R3;
using UnityEngine.InputSystem;
using VContainer;

namespace Scr.Player.Action {
    
    public interface IMoveAction : IPlayerAction {
        
        bool IsEnable { get; }
        
        void SetEnable (bool enable);
        
        bool IsSprint { get; }
    }
    
    //追加:移動処理を別コンポーネントに分離
    
    public class MoveAction : PlayerActionBehaviour, IMoveAction {
        
        //本当なら移動量管理クラスを制作して、ダッシュとわけて設計するべきだが過剰設計になると判断し断念。
        //ぶっちゃけ再利用効かないし面倒臭いや

        [OdinSerialize]
        [LabelText("移動入力")]
        private IInputModule<Vector2> _input;
        
        [OdinSerialize]
        [LabelText("ダッシュ入力")]
        private IInputModule<float> _sprintInput;

        [Title("設定")] 
        
        [SerializeField]
        [LabelText("最大移動速度")]
        private float _maxMoveSpeed = 6.0f;
        
        [SerializeField]
        [LabelText("移動速度")]
        private float _moveSpeed = 3.0f;

        [SerializeField] 
        [LabelText("空中での抵抗")] 
        private float _airResistance = 3.0f;

        [SerializeField] 
        [LabelText("ダッシュ速度倍率")]
        private float _sprintRatio = 2.0f;

        [Title("ランタイム")] 
        
        [SerializeField]
        [LabelText("現在の進行方向")]
        private Vector2 _currentDirection = Vector3.zero;
        
        [SerializeField]
        [LabelText("ダッシュ入力")]
        private bool _isSprint = false;
        
        [SerializeField]
        [LabelText("有効フラグ")]
        private bool _isEnable = true;
        
        private IGroundedManger _grounded;

        public bool IsEnable  => _isEnable;
        public bool IsSprint => _isSprint;

        protected override void OnPreStart() {
            base.OnPreStart();
            
            _grounded = _resolver.Resolve<IGroundedManger>();
            
            RegisterMoveInput();
            
            RegisterSprintInput();
            
        }

        private void FixedUpdate() {

            if (_isEnable is false) return;
            
            var movement = new Vector3(_currentDirection.x, 0.0f, 0.0f);

            if (_grounded.IsGrounded is false) {
                movement /= _airResistance;
            }
            
            var speedLimit = _isSprint
                ?_maxMoveSpeed * _sprintRatio - Mathf.Abs(_rigidbody.linearVelocity.x)
                :_maxMoveSpeed - Mathf.Abs(_rigidbody.linearVelocity.x);
            
            _rigidbody.AddForce(_isSprint
                ?_moveSpeed * speedLimit * _sprintRatio * movement 
                :_moveSpeed * speedLimit * movement, 
                ForceMode.Force
                );
            
        }
        
        private void Update() {
            
            if (_currentDirection.x > 0) {
                gameObject.transform.root.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else if (_currentDirection.x < 0) {
                gameObject.transform.root.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
            
            SetAnimation();
        }

        private void RegisterMoveInput() {
            _input
                .Stream
                .Subscribe(x => {
                    Debug.Log($"{x.Phase}, {x.Value}, {x.Time}");
                    
                    if (_playable.Playable is false) return;

                    _currentDirection = x.Value.normalized;
                    
                })
                .AddTo(this);
        }

        private void RegisterSprintInput() {
            _sprintInput
                .Stream
                .Subscribe(x => {
                    
                    Debug.Log($"{x.Phase}, {x.Value}, {x.Time}");
                    
                    if (_playable.Playable is false) return;

                    if (x.Phase is not InputActionPhase.Canceled) {
                        _isSprint = true;
                    }
                    else {
                        _isSprint = false;
                    }
                })
                .AddTo(this);
        }


        private void SetAnimation() {
            
            var rbVelocity = _rigidbody.linearVelocity.x;
            
            //リジッドボディの速度をspeedに入力、速度に応じてアイドル、歩き、走りの切り替え
            _animator.SetFloat("speed",Mathf.Abs(rbVelocity/2));
            
            if (_currentDirection.y < -0.3) _animator.SetBool("squat", true);
            else _animator.SetBool("squat", false);
            
            if (Mathf.Abs(rbVelocity) > 3.0f && rbVelocity * _currentDirection.x < 0) _animator.SetBool("recoil", true);
            else _animator.SetBool("recoil", false);
        }

        public void SetEnable(bool enable) {
            _isEnable = enable;
        }

    }
}