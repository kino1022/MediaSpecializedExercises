using RinaInput.Signal;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Player.Action {
    public class TailMarioJumpAction : JumpActionBehaviour {
        
        [SerializeField]
        [LabelText("基本ジャンプ力")]
        private float _baseForce = 5.0f;

        [SerializeField] 
        [LabelText("空中ジャンプ時の倍率")]
        private float _onAirJumpRatio = 0.3f;
        
        [SerializeField]
        [LabelText("最大ジャンプ力")]
        private float _holdJumpForce = 30.0f;
        
        [SerializeField]
        [LabelText("最大ホールド時間")]
        private float _maxHoldTime = 0.5f;

        [SerializeField] [LabelText("押下中の重力補正")]
        private float _airResistance = 0.3f;

        [SerializeField]
        [LabelText("ジャンプ中か")]
        [ReadOnly]
        private bool _isJumping = false;
        
        [SerializeField]
        [LabelText("入力中フラグ")]
        [ReadOnly]
        private bool _isHolding = false;

        [SerializeField]
        [LabelText("ホールド時間カウンター")]
        [ReadOnly]
        private float _holdCounter = 0.0f;

        private IPowerMeterManager _power;

        protected override void OnPreStart() {
            base.OnPreStart();
            
            // PlayerのLifetimeScopeから取得（ローカルなコンポーネント）
            var playerScope = gameObject.GetComponentInParent<VContainer.Unity.LifetimeScope>();
            if (playerScope != null) {
                _power = playerScope.Container.Resolve<IPowerMeterManager>();
            }
            else {
                Debug.LogError($"[TailMarioJumpAction] PlayerのLifetimeScopeが見つかりません");
            }
        }


        private void FixedUpdate() {

            //初回ジャンプの処理
            if (
                //ジャンプフラグが立っており
                _isJumping && 
                //かつ、操作可能フラグが立っており
                _playable.Playable &&
                //接地しているなら
                _grounded.IsGrounded) {
                
                _rigidbody.AddForce(Vector3.up * _baseForce, ForceMode.Impulse);
            }

            //押されていて操作可能かつ、Pゲージが残っているなら
            if (_isJumping && _playable.Playable && _power.Power < 0) {
                //少し上昇させうr
                _rigidbody.AddForce(Vector3.up * _baseForce, ForceMode.Acceleration);
            }

            //下降していて、ジャンプボタンが押されているなら
            if (_rigidbody.linearVelocity.y < 0 && _isJumping) {
                _rigidbody.linearVelocity = new Vector3(
                    _rigidbody.linearVelocity.x,
                    //ベクトルに対して抵抗をかける
                    _rigidbody.linearVelocity.y * _airResistance,
                    _rigidbody.linearVelocity.z);
            }
        }

        private void Update() {
            //update内で飛んでいる際のアニメーション呼び出し
            _animator.SetBool("jump", !_grounded.IsGrounded);
            _animator.SetBool("fly", !_grounded.IsGrounded);
        }

        protected override void OnPressed(InputSignal<float> signal) {
            base.OnPressed(signal);

            if (_playable.Playable is false) return;
            
            _isJumping = true;
        }

        protected override void OnReleased(InputSignal<float> signal) {
            base.OnReleased(signal);
            
            if (_playable.Playable is false) return;

            _isJumping = false;
            
            _isHolding = false;
        }

    }
}