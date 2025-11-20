using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.Player {

    public interface IGroundedManger {
        
        bool IsGrounded { get; }
        
    }
    
    public class GroundedManager : SerializedMonoBehaviour, IGroundedManger {
        
        [SerializeField]
        private bool _serializedGrounded = false;
        
        [Title("設定")]
        
        [SerializeField]
        [LabelText("接地判定オフセット")]
        private float _checkOffset = 0.1f;
        
        [SerializeField]
        [LabelText("接地判定半径")]
        private float _checkRadius = 0.4f;
        
        [SerializeField]
        [LabelText("接地判定距離")]
        private float _checkDistance = 0.3f;
        
        private RaycastHit _hit;

        public bool IsGrounded => CheckGrounded();

        private void Awake() {
            //接地チェック用レイの距離の計算
            _checkDistance = _checkRadius / 2;
            //接地チェック用レイの開始地点調整用変数の計算
            _checkOffset = _checkRadius + _checkDistance / 4;
        }

        private void FixedUpdate() {
            _serializedGrounded = CheckGrounded();
        }
        
        private void OnDrawGizmos() {
            //接地チェックのレイキャストを可視化
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.down * (_checkRadius / 2), _checkRadius);
        }
        
        private bool CheckGrounded() {
            //球体レイキャストをプレイヤー足元方向に発射して接触があればTrueを返す
            //接触情報はhitに格納
            return Physics.SphereCast(transform.position + _checkOffset * Vector3.up, _checkRadius, Vector3.down, out _hit, _checkDistance, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore);
        }
    }
}