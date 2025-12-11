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
            
            //目標の位置(ターゲットの位置を取得)
            Vector3 targetPos = _target.transform.position;
            
            //高さの補正に対してサイン波を加えてフワフワを表現
            float hoverOffset = Mathf.Sin(Time.fixedTime * _bobbingSpeed) * _bobbingAmount;
            //目標の高さに対して補正値を加算
            targetPos.y += _heightFromTarget + hoverOffset;

            //現在の自分の位置をRigidbodyから取得
            Vector3 currentPos = _rigidBody.position;


            //Vector3.SmoothDampを利用して滑らかに移動するための次の座標を取得
            Vector3 nextPos = Vector3.SmoothDamp(
                currentPos,
                targetPos,
                ref _currentVelocityRef,
                _smoothTime,
                _maxSpeed
            );

            //次の座標に向かうための方向ベクトルを算出
            Vector3 neededVelocity = (nextPos - currentPos) / Time.fixedDeltaTime;
            //算出したベクトルをRigidBodyに放り込む
            _rigidBody.linearVelocity = neededVelocity;

            if (_lookAtTarget) {
                //ターゲットの方を向く処理を実行
                LockTarget();
            }
        }

        public override void Update(float deltaTime) {
            base.Update(deltaTime);
        }

        private void LockTarget() {
            
            //自分の位置からターゲットの位置への方向ベクトルを算出
            Vector3 direction = _target.transform.position - _enemy.transform.position;
            //高低差は無視しておく
            direction.y = 0; 

            if (direction != Vector3.zero) 　{
                //その方向を向くための回転を算出
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                //回転をRigidBodyに実行させる
                _rigidBody.rotation = Quaternion.Slerp(_rigidBody.rotation, targetRotation, Time.fixedDeltaTime * 5.0f);
            }
        }
    }
}