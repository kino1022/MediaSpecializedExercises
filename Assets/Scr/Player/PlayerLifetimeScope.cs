using RinaSymbol;
using Scr.Utility;
using UnityEngine;
using UnityEngine.Playables;
using VContainer;
using VContainer.Unity;

namespace Scr.Player {
    public class PlayerLifetimeScope : SymbolLifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            base.Configure(builder);
            
            var animator = gameObject.GetComponentFromWhole<Animator>();

            if (animator is not null) {
                builder
                    .RegisterComponent(animator)
                    .As<Animator>();
            }
            
            var rigid = gameObject.GetComponentFromWhole<Rigidbody>();
            
            if (rigid is not null) {
                builder
                    .RegisterComponent(rigid)
                    .As<Rigidbody>();
            }

            var grounded = gameObject.GetComponentFromWhole<IGroundedManger>();

            if (grounded is not null) {
                builder
                    .RegisterComponent(grounded)
                    .As<IGroundedManger>();
            }

            builder
                .Register<IPlayableManager, PlayableManager>(Lifetime.Singleton)
                .As<IPlayableProvider>();

        }
    }
}