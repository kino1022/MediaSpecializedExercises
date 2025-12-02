using RinaInput.Signal;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.Player.Action {
    public class TailMarioJumpAction : JumpActionBehaviour {
        [SerializeField]
        [LabelText("基本ジャンプ力")]
        private float _baseForce = 5.0f;
        
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
        

        private void FixedUpdate() {

            //初回ジャンプの処理
            if (!_isJumping && _playable.Playable && _grounded.IsGrounded) {
                _rigidbody.AddForce(Vector3.up * _baseForce, ForceMode.Impulse);
                _isHolding = true;
                _holdCounter = 0.0f;
                _isJumping = false;
            }
            
            //長押しで飛距離を上げる処理関連
            if (_isHolding && _holdCounter < _maxHoldTime) {
                float perforce = _holdJumpForce * Time.fixedDeltaTime;
                _rigidbody.AddForce(Vector3.up * perforce, ForceMode.Acceleration);
                _holdCounter += Time.fixedDeltaTime;
            }
            
            //着地してなくて長押しされている場合の処理
            if (_isHolding && _playable.Playable && !_grounded.IsGrounded) {
                var previousVelocity = _rigidbody.linearVelocity;
                _rigidbody.linearVelocity = new Vector3(previousVelocity.x, previousVelocity.y * _airResistance, previousVelocity.z);
            }
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
        }

    }
}