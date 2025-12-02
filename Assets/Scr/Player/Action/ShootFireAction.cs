using R3;
using RinaInput.Controller.Module;
using Scr.FireBall;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Scr.Player.Action {
    public class ShootFireAction : PlayerActionBehaviour {

        [OdinSerialize]
        [LabelText("射撃入力")]
        private IInputModule<float> _input;

        [SerializeField]
        [LabelText("発射するオブジェクト")]
        private FireBallController _shootItem;

        [SerializeField]
        [LabelText("発射ポイント")]
        private GameObject _shootPoint;
        
        protected override void OnPostStart() {
            base.OnPostStart();
            
            RegisterInput();
        }

        private void RegisterInput() {
            _input
                .Stream
                .Subscribe(_ => {
                    _animator.Play("Bear_ throw");
                    var fire = Instantiate(_shootItem, _shootPoint.transform.position, _shootPoint.transform.rotation);
                    fire.SetMovement(transform.localScale.x);
                })
                .AddTo(this);
        }
        
    }
}