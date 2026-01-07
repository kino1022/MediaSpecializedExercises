using System;
using Scr.Utility;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scr.GameManager {
    public class DeadBlock : SerializedMonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            var player = other.gameObject.GetComponentFromWhole<Player.Player>();
            if (player is not null) {
                player.Die();
            }
        }
    }
}