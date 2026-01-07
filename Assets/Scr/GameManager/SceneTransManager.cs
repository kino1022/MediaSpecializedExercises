using System;
using MessagePipe;
using Scr.Player;
using Scr.Stage;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.GameManager {
    public class SceneTransManager : SerializedMonoBehaviour {
        
        [SerializeField]
        private SceneEnum _deadScene = SceneEnum.DeadScene;
        
        [SerializeField]
        private SceneEnum _gameOverScene = SceneEnum.GameOverScene;
        
        private ISubscriber<OnDeadEventBus> _deadSubscriber;
        
        private IDisposable _deadSubscription;
        
        private ISubscriber<GameOverEventBus> _gameOverSubscriber;
        
        private IDisposable _gameOverSubscription;

        private IObjectResolver _resolver;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            _deadSubscriber = _resolver.Resolve<ISubscriber<OnDeadEventBus>>();
            _deadSubscription = _deadSubscriber.Subscribe(OnDeadEventBusHandler);
            _gameOverSubscriber = _resolver.Resolve<ISubscriber<GameOverEventBus>>();
            _gameOverSubscription = _gameOverSubscriber.Subscribe(OnGameOverEventBusHandler);
        }

        private void OnDestroy() {
            
            _deadSubscription?.Dispose();
            
            _gameOverSubscription?.Dispose();
            
        }

        private void OnDeadEventBusHandler(OnDeadEventBus obj) {
            _deadScene.LoadScene();
        }
        
        private void OnGameOverEventBusHandler(GameOverEventBus obj) {
            _gameOverScene.LoadScene();
        }
    }
}