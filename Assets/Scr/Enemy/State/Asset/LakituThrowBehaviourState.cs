using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.Enemy.State.Asset {
    [Serializable]
    public class LakituThrowBehaviourState : AEnemyBehaviourState {

        [SerializeField]
        private GameObject _target;
        
        [SerializeField]
        [LabelText("投げる強さ")]
        private Vector3 _throwForce = new Vector3(0.0f, 5f, 0.0f);
        [SerializeField]
        [LabelText("投げる座標")]
        private GameObject _throwPoint;
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

            //投げるオブジェクトを生成
            Rigidbody paipoInstance = GameObject.Instantiate(
                _throwPrefab,
                _throwPoint.transform.position, 
                _throwPoint.transform.rotation
                );

            //投げる力を決める
            Vector3 finalForce = _throwForce;

            if (_towardsPlayer && _target != null)
            {
                // プレイヤーへの方向ベクトル（Y軸は無視して水平方向のみ）を取得
                Vector3 direction = (_target.transform.position - _throwPoint.transform.position).normalized;
                direction.z = 0.0f;
                
                //X軸の投げる力を算出
                float forwardForce = _throwForce.x == 0.0f　? 1.0f : _throwForce.x;
                //Y軸の投げる力を算出
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