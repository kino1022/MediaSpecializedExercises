using MessagePipe;
using RinaSymbol;
using Scr.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scr.GameManager {
    public class GameManagerLifetimeScope : SymbolLifetimeScope {
        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);

            builder
                .RegisterMessagePipe();
            
            var audioSource = gameObject.GetComponentFromWhole<AudioSource>();

            if (audioSource is not null) {
                builder
                    .RegisterComponent(audioSource)
                    .As<AudioSource>();
            }
        }
    }
}