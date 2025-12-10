using System;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Enemy.State.Asset {
    [Serializable]
    public class LakituMoveBehaviourState : AEnemyBehaviourState　{
        
        [Title("Target")]
        [SerializeField] private GameObject _target; // プレイヤー（マリオ）
        [SerializeField] private float _heightFromTarget = 5.0f; // マリオの頭上何メートルに浮くか

        [Title("Movement")]
        [SerializeField] 
        private float _smoothTime = 0.5f; // 追従の遅れ具合
        [SerializeField] 
        private float _maxSpeed = 15.0f;
    
        [Title("Hover")]
        [SerializeField]
        private float _bobbingAmount = 0.5f; // 上下のふわふわ揺れ幅
        [SerializeField] 
        private float _bobbingSpeed = 2.0f;

        [Title("Rotation")]
        [SerializeField] 
        private bool _lookAtTarget = true;
        
        private Rigidbody _rigidBody;
        
        private Vector3 _currentVelocityRef;

        public override void Enter() {
            base.Enter();
            _rigidBody = _resolver.Resolve<Rigidbody>();
            _rigidBody ??= _enemy.GetComponent<Rigidbody>();
            
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public override void FixedUpdate(float deltaTime) {
            base.FixedUpdate(deltaTime);
            // 1. 目標地点の計算
            // XとZはマリオの位置、Yは「マリオの高さ + 指定した高さ + ふわふわ成分」
            Vector3 targetPos = _target.transform.position;
        
            // 高さ(Y)にふわふわ(Sin波)を加える
            float hoverOffset = Mathf.Sin(Time.fixedTime * _bobbingSpeed) * _bobbingAmount;
            targetPos.y += _heightFromTarget + hoverOffset;

            // 2. 現在位置
            Vector3 currentPos = _rigidBody.position;

            // 3. SmoothDampで「次のフレームに居てほしい場所」を計算
            // Vector3.SmoothDampを使えばXYZまとめて計算してくれます
            Vector3 nextPos = Vector3.SmoothDamp(
                currentPos,
                targetPos,
                ref _currentVelocityRef,
                _smoothTime,
                _maxSpeed
            );

            // 4. 速度を計算してRigidbodyに適用
            // (目標位置 - 現在位置) / 経過時間 = 必要な速度
            Vector3 neededVelocity = (nextPos - currentPos) / Time.fixedDeltaTime;
            _rigidBody.linearVelocity = neededVelocity;

            if (_lookAtTarget) {
                LockTarget();
            }
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);
        }

        private void LockTarget() {
            // マリオの方を向く処理
            // Y軸回転のみ行い、変に傾かないようにする
            Vector3 direction = _target.transform.position - _enemy.transform.position;
            direction.y = 0; // 高低差は無視して水平方向のみ向く

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // 向きも少し遅れて滑らかに変えるなら Slerp を使う
                _rigidBody.rotation = Quaternion.Slerp(_rigidBody.rotation, targetRotation, Time.fixedDeltaTime * 5.0f);
            }
        }
    }
}