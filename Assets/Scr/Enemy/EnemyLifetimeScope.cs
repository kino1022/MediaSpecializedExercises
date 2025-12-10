using RinaSymbol;
using Scr.Player;
using Scr.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Scr.Enemy {
    public class EnemyLifetimeScope : SymbolLifetimeScope {

        protected override void Configure(IContainerBuilder builder) {
            
            var grounded = gameObject.GetComponentFromWhole<IGroundedManger>();

            if (grounded is not null) {
                builder
                    .RegisterComponent(grounded)
                    .As<IGroundedManger>();
            }
            
            var rigidBody = gameObject.GetComponentFromWhole<Rigidbody>();

            if (rigidBody is not null) {
                builder
                    .RegisterComponent(rigidBody)
                    .As<Rigidbody>();
            }
            
            var animator = gameObject.GetComponentFromWhole<Animator>();

            if (animator is not null) {
                builder
                    .RegisterComponent(animator)
                    .As<Animator>();
            }
            
            var movementHolder = gameObject.GetComponentFromWhole<IEnemyMovementHolder>();
            
            if (movementHolder is not null) {
                builder
                    .RegisterComponent(movementHolder)
                    .As<IEnemyMovementHolder>();
            }
        }
    }
}