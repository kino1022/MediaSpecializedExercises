using MessagePipe;
using R3;
using RinaInput.Controller.Module;
using Scr.Utility;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace Scr.Player.Action {
    /// <summary>
    /// しっぽ攻撃の処理を行うクラス
    /// </summary>
    public class TailAttackAction : PlayerActionBehaviour {

        //入力検知のためのモジュール
        //基本的な機能はInputSystemと同様なので割愛
        [OdinSerialize]
        [LabelText("攻撃入力")]
        private IInputModule<float> _inputModule;

        //攻撃範囲を指定する変数を用意
        [SerializeField]
        [LabelText("攻撃範囲")]
        private float _attackLange = 1.5f;

        private IPublisher<TakeDamageEventBus> _publisher;
        
        protected override void OnPostStart() {
            base.OnPostStart();

            _publisher = _resolver.Resolve<IPublisher<TakeDamageEventBus>>();

            RegisterInput();
        }

        //攻撃処理のメソッド
        private void OnTailAttack() {
            
            //対応したアニメーションを作成
            _animator.Play("Bear_tailAttack");
            
            //攻撃範囲内のオブジェクトを検出
            var hitColliders = Physics.OverlapSphere(
                transform.position,
                _attackLange
            );

            //攻撃範囲内のオブジェクトに対してダメージ処理を実行
            foreach (var hit in hitColliders) {
                
                Debug.Log(hit.name);
                
                //hitしたオブジェクトに対してダメージを通知する
                _publisher.Publish(new TakeDamageEventBus (1, hit.gameObject));
            }
        }

        private void RegisterInput() {

            _inputModule
                .Stream
                .Subscribe(x => {
                    Debug.Log("しっぽ攻撃が入力されました");
                    //InputActionPhase.Startedに反応する処理に相当
                    if (x.Phase is InputActionPhase.Started) {
                        //攻撃処理本体を呼び出す
                        OnTailAttack();
                    }
                })
                .AddTo(this);
        }
    }
}
