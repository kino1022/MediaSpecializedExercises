using R3;
using RinaInput.Controller.Module;
using Scr.Audio;
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
        
        [SerializeField]
        [LabelText("射撃音")]
        private AudioClip _fireAudioClip;
        
        protected override void OnPostStart() {
            base.OnPostStart();
            
            RegisterInput();
        }

        private void RegisterInput() {
            _input
                .Stream
                .Subscribe(_ => {
                    _animator.Play("Bear_ throw");
                    if (_fireAudioClip is not null) {
                        _audioPublisher.Publish(new AudioPlayEventBus(new AudioPlayContext(1), _fireAudioClip, gameObject));
                    }
                    var fire = Instantiate(_shootItem, _shootPoint.transform.position, _shootPoint.transform.rotation);
                    fire.SetMovement(transform.root.localScale.x);
                })
                .AddTo(this);
        }
        
    }
}