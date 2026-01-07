using UnityEngine;

namespace Scr.Utility {
    public readonly struct TakeDamageEventBus {
        
        public int Damage { get; }
        
        public GameObject Target { get; }
        
        public TakeDamageEventBus (int damage, GameObject target) {
            Damage = damage;
            Target = target;
        }
        
    }
}