using Scr.Utility;
using Sirenix.OdinInspector;
using VContainer;
using VContainer.Unity;

namespace Scr.Player.Action {
    public class ActionInstaller : SerializedMonoBehaviour , IInstaller{

        public void Install(IContainerBuilder builder) {

            var move = gameObject.GetComponentFromWhole<IMoveAction>();
            
            if (move is not null) {
                builder
                    .RegisterComponent(move)
                    .As<IMoveAction>();
            }
        }
    }
}