using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.Enemy.State.Asset {
    [Serializable]
    public class LakituThrowBehaviourState : AEnemyBehaviourState {

        [SerializeField]
        private Transform _target;
        
        [SerializeField]
        [LabelText("投げる強さ")]
        private Vector3 _throwForce = new Vector3(0f, 5f, 10f);
        [SerializeField]
        [LabelText("投げる座標")]
        private Transform _throwPoint;
        [SerializeField]
        [LabelText("投げるもの")]
        private Rigidbody _throwPrefab;
        
        [SerializeField]
        [LabelText("プレイヤーの方を向いて投げるか")]
        private bool _towardsPlayer = true;

        public override void Enter() {
            base.Enter();
            ThrowEnemy();
            stateEnd = true;
        }

        private void ThrowEnemy() {
            if (_throwPrefab == null || _throwPoint  == null) return;

            // 1. 生成 (Instantiate)
            Rigidbody paipoInstance = GameObject.Instantiate(
                _throwPrefab,
                _throwPoint.position, 
                _throwPoint.rotation
                );

            // 2. 力を加える方向を計算
            Vector3 finalForce = _throwForce;

            if (_towardsPlayer && _target != null)
            {
                // プレイヤーへの方向ベクトル（Y軸は無視して水平方向のみ）
                Vector3 direction = (_target.position - _throwPoint.position).normalized;
            
                // 設定された「上への力(Y)」は維持しつつ、「水平方向の力(X,Z)」をプレイヤーに向ける
                // _throwForce.z を「前方向への強さ」として扱います
                float forwardForce = _throwForce.x;
                float upwardForce = _throwForce.y;

                // 方向 * 前への強さ + 上への強さ
                finalForce = (direction * forwardForce) + (Vector3.up * upwardForce);
            }
            else
            {
                // プレイヤーを見ない場合は、ジュゲムの向いている方向(forward)基準にする
                // transform.TransformDirection でローカル座標系の力をワールド座標系に変換
                finalForce = _enemy.transform.TransformDirection(_throwForce);
            }

            // 3. 力を加える (Impulseモード推奨)
            // ForceMode.Impulse = 瞬間的な衝撃（投げる、打つ動作に最適）
            paipoInstance.AddForce(finalForce, ForceMode.Impulse);
        }
    }
}