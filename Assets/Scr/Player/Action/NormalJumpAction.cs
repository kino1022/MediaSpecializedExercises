using System;
using RinaInput.Signal;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Scr.Player.Action {
    public class NormalJumpAction : JumpActionBehaviour {
        
        [SerializeField]
        [LabelText("基本ジャンプ力")]
        private float _baseForce = 5.0f;
        
        [SerializeField]
        [LabelText("最大ジャンプ力")]
        private float _holdJumpForce = 30.0f;
        
        [SerializeField]
        [LabelText("最大ホールド時間")]
        private float _maxHoldTime = 0.5f;

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
            if (_isJumping && _playable.Playable && _grounded.IsGrounded) {
                _rigidbody.AddForce(Vector3.up * _baseForce, ForceMode.Impulse);
                _isHolding = true;
                _holdCounter = 0.0f;
                _isJumping = false;
            }
            
            if (_isHolding && _holdCounter < _maxHoldTime) {
                float perforce = _holdJumpForce * Time.fixedDeltaTime;
                _rigidbody.AddForce(Vector3.up * perforce, ForceMode.Acceleration);
                _holdCounter += Time.fixedDeltaTime;
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

        public override void ExecuteJump() {
            _isJumping = true;
        }
    }
}