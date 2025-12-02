using RinaInput.Signal;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using VContainer;

namespace Scr.Player.Action {
    public class ArtherJumpAction : JumpActionBehaviour {
        
        [OdinSerialize]
        [LabelText("移動アクション")]
        [ReadOnly]
        private IMoveAction _moveAction;

        protected override void OnPostStart() {
            base.OnPostStart();
            
            _moveAction = _resolver.Resolve<IMoveAction>();
        }

        protected override void OnPressed(InputSignal<float> signal) {
            base.OnPressed(signal);
            
            if (_playable.Playable is false) return;
            
            _moveAction.SetEnable(false);
        }
        
    }
}