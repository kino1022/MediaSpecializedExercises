using System;
using MessagePipe;
using R3;
using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

namespace Scr.Enemy {
    
    
    public class Enemy : SerializedMonoBehaviour {

        [SerializeField]
        [LabelText("敵の体力")]
        private int _enemyHealth = 3;

        private IObjectResolver _resolver;

        private ISubscriber<TakeDamageEventBus> _damageSubscriber;
        
        private IDisposable _damageSubscription;

        [Inject]
        public void Construct(IObjectResolver resolver) {
            _resolver = resolver;
        }

        private void Start() {
            
            _damageSubscriber = 
                _resolver
                    .Resolve<ISubscriber<TakeDamageEventBus>>();
            
            _damageSubscription =
                _damageSubscriber
                    .Subscribe(OnTakeDamage)
                    .AddTo(this);
            
        }

        private void OnTakeDamage(TakeDamageEventBus bus) {

            Debug.Log("いてぇよ");

            if (!bus.Target.transform.IsChildOf(transform)) {
                return;
            }
            
            _enemyHealth -= bus.Damage;

            if (_enemyHealth <= 0) {
                Destroy(gameObject);
            }
        }

    }
}