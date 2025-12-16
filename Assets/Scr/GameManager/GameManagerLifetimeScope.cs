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
            
            var audioSource = gameObject.GetComponentFromWhole<AudioSource>();

            if (audioSource is not null) {
                builder
                    .RegisterComponent(audioSource)
                    .As<AudioSource>();
            }
        }
    }
}