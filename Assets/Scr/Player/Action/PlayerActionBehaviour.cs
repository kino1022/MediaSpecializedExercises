using MessagePipe;
using R3;
using Scr.Audio;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using VContainer;

namespace Scr.Player.Action {

    public interface IPlayerAction {
        
        ReadOnlyReactiveProperty<bool> OnAction { get; }
        
        
    }
    
    /// <summary>
    /// 全てのプレイヤーアクションの基底クラス
    /// </summary>
    public abstract class PlayerActionBehaviour : SerializedMonoBehaviour {

        protected ReactiveProperty<bool> _onAction = new(false);
        
        [Title("参照")]
        
        [SerializeField]
        [LabelText("アニメーター")]
        [ReadOnly]
        protected Animator _animator;

        [SerializeField]
        [ReadOnly]
        protected Rigidbody _rigidbody;

        [OdinSerialize]
        [LabelText("操作可能フラグ")]
        [ReadOnly]
        protected IPlayableProvider _playable;
        
        protected IPublisher<AudioPlayEventBus> _audioPublisher;

        protected IObjectResolver _resolver;
        
        public ReadOnlyReactiveProperty<bool> OnAction => _onAction;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            OnPreStart();

            _animator = _resolver.Resolve<Animator>();
            
            _rigidbody = _resolver.Resolve<Rigidbody>();
            
            _playable = _resolver.Resolve<IPlayableProvider>();
            
            _audioPublisher = _resolver.Resolve<IPublisher<AudioPlayEventBus>>();

            OnPostStart();
        }
        
        protected virtual void OnPreStart() {}
        
        protected virtual void OnPostStart() {}
    }
}