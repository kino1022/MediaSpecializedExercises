using R3;
using RinaInput.Controller.Module;
using RinaInput.Signal;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using VContainer;

namespace Scr.Player.Action {
    public abstract class JumpActionBehaviour : PlayerActionBehaviour {

        [OdinSerialize]
        [LabelText("ジャンプ入力")]
        protected IInputModule<float> _input;

        [OdinSerialize]
        [LabelText("着地マネージャ")]
        [ReadOnly]
        protected IGroundedManger _grounded;

        protected override void OnPostStart() {
            base.OnPostStart();
            
            _grounded = _resolver.Resolve<IGroundedManger>();
            
            RegisterInput();
        }

        private void RegisterInput() {
            _input
                .Stream
                .Subscribe(x => {
                    if (x.Phase is InputActionPhase.Started) {
                        OnPressed(x);
                    }
                    else if (x.Phase is InputActionPhase.Canceled) {
                        OnReleased(x);
                    }
                })
                .AddTo(this);
        }
        
        protected virtual void OnPressed (InputSignal<float> signal) {}
        
        protected virtual void OnReleased (InputSignal<float> signal) {}
        
    }
}