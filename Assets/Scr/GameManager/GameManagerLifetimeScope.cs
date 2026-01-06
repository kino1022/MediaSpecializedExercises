using MessagePipe;
using RinaSymbol;
using Scr.Player;
using Scr.Utility;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using VContainer;
using VContainer.Unity;

namespace Scr.GameManager {
    public class GameManagerLifetimeScope : SymbolLifetimeScope {
        
        private ITimeManager _timeManager;
        
        private IScoreManager _scoreManager;
        
        private ICoinManager _coinManager;
        
        private ILifeManager _lifeManager;

        protected void Start() {
            _scoreManager = Container.Resolve<IScoreManager>();
            
            _timeManager = Container.Resolve<ITimeManager>();
            
            _coinManager = Container.Resolve<ICoinManager>();
            
            _lifeManager = Container.Resolve<ILifeManager>();
            
            _timeManager.InitTimer(300);
            
            _timeManager.StartCount();
            
            DontDestroyOnLoad(gameObject);
        }
        
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);

            builder
                .RegisterMessagePipe();
            
            builder
                .Register<IPlayableManager, PlayableManager>(Lifetime.Singleton)
                .As<IPlayableProvider>();

            builder
                .Register<ITimeManager, TimeManager>(Lifetime.Singleton);

            builder
                .Register<IScoreManager, ScoreManager>(Lifetime.Singleton);
            
            builder
                .Register<ICoinManager, CoinManager>(Lifetime.Singleton);
            
            builder
                .Register<ILifeManager, LifeManager>(Lifetime.Singleton);
            
            var audioSource = gameObject.GetComponentFromWhole<AudioSource>();

            if (audioSource is not null) {
                builder
                    .RegisterComponent(audioSource)
                    .As<AudioSource>();
            }
        }
    }
}